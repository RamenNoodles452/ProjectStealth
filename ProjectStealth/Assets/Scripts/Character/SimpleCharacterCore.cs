﻿using UnityEngine;
using System.Collections;

public class SimpleCharacterCore : MonoBehaviour
{
    protected int previousFacingDirection = 1;
    public int FacingDirection = 1;
    private SpriteRenderer SpriteRenderer;
    public Animator Anim;
    public IInputManager InputManager;
    public Vector2 Velocity;
    protected BoxCollider2D characterCollider;
    
    //gravity vars
    public bool OnTheGround = false;
    private const float MAX_VERTICAL_SPEED = 10.0f;
    private const float GRAVITATIONAL_FORCE = -30.0f;

    //jump vars
    private const float JUMP_VERTICAL_SPEED = 6.0f;
    protected const float JUMP_HORIZONTAL_SPEED_MIN = 2.5f;
    protected const float JUMP_HORIZONTAL_SPEED_MAX = 4.0f;
    protected const float JUMP_ACCEL = 4.0f;

    private const float JUMP_CONTROL_TIME = 0.20f; //maximum duration of a jump if you hold it
    private const float JUMP_DURATION_MIN = 0.10f; //minimum duration of a jump if you tap it
    private const float JUMP_GRACE_PERIOD_TIME = 0.10f; //how long a player has to jump if they slip off a platform
    private bool jumpGracePeriod; //variable for jump tolerance if a player walks off a platform but wants to jump
    private float jumpGracePeriodTime;
    public bool isJumping; //TODO: protected
	protected float jumpInputTime;
	public bool jumpTurned; //TODO: protected

    //walk and run vars
    private const float MAX_HORIZONTAL_SPEED = 10.0f;
    protected float WALK_SPEED = 1.0f; //used for cutscenes with Alice, guards will walk when not alerted
    protected float SNEAK_SPEED = 2.0f; //Alice's default speed, enemies that were walking will use this speed when on guard
    protected float RUN_SPEED = 4.5f;
    protected float ACCELERATION = 6.0f; // acceleration used for velocity calcs when running
    protected float DRAG = 15.0f; // how quickly a character decelerates when running
    public enum moveState { isWalking, isSneaking, isRunning }; //TODO: protected
    public moveState currentMoveState = moveState.isWalking; //TODO: protected
    protected moveState prevMoveState;
    protected float characterAccel = 0.0f;
    private bool startRun; // this bool prevents wonky shit from happening if you turn around during a run

    // ledge logic
    public bool lookingOverLedge; // TODO: private
    public bool againstTheLedge; // TODO: private

	// bezier curve vars for getting up ledges and jumping over cover
	protected Vector2 bzrStartPosition;
	protected Vector2 bzrEndPosition;
	protected Vector2 bzrCurvePosition;
	protected float bzrDistance;

    // Use this for initialization
    public virtual void Start()
    {
        characterCollider = GetComponent<BoxCollider2D>();
        Anim = GetComponent<Animator>();
        InputManager = GetComponent<IInputManager>();

        // character sprite is now a child object. If a chracter sprite has multiple child sprite tho, this might break
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        Velocity = new Vector2(0.0f, 0.0f);
        jumpGracePeriod = false;
        isJumping = false;
        jumpInputTime = 0.0f;
    }

    public virtual void Update()
    {
        MovementInput();

        CalculateDirection();
        // set Sprite flip
        if (previousFacingDirection != FacingDirection)
            SetFacing();

        if (jumpGracePeriod == true)
        {
            jumpGracePeriodTime = jumpGracePeriodTime + Time.deltaTime * TimeScale.timeScale;
            if (jumpGracePeriodTime >= JUMP_GRACE_PERIOD_TIME)
                jumpGracePeriod = false;
        }
    }

    public virtual void LateUpdate()
    {
        // prev state assignments
        prevMoveState = currentMoveState;
        previousFacingDirection = FacingDirection;
    }

    public virtual void FixedUpdate()
    {
        // Give all characters gravity
        HorizontalVelocity();
        VerticalVelocity();
        Collisions();

        //move the character after all calculations have been done
        transform.Translate(Velocity);
    }

    private void HorizontalVelocity()
    {
        if(OnTheGround)
        {
            //movement stuff
            if (currentMoveState == moveState.isWalking)
                Velocity.x = WALK_SPEED * InputManager.HorizontalAxis;
            else if (currentMoveState == moveState.isSneaking)
            {
                // if current speed is greater than sneak speed, then decel to sneak speed.
                if(Mathf.Abs(Velocity.x) > SNEAK_SPEED)
                {
                    if (Velocity.x > 0.0f)
                        Velocity.x = Velocity.x - DRAG * Time.deltaTime * TimeScale.timeScale;
                    else if (Velocity.x < 0.0f)
                        Velocity.x = Velocity.x + DRAG * Time.deltaTime * TimeScale.timeScale;
                }
                else
                {
                    Velocity.x = SNEAK_SPEED * InputManager.HorizontalAxis;
                }
            }
            else if (currentMoveState == moveState.isRunning)
            {   
                //smooth damp to 0 if there's no directional input for running or if you're trying to run the opposite direction
                if (characterAccel == 0.0f || (characterAccel < 0.0f && Velocity.x > 0.0f) || (characterAccel > 0.0f && Velocity.x < 0.0f))
                {
                    if (Mathf.Abs(Velocity.x) > SNEAK_SPEED)
                    {
                        //print("SKID BOIS");
                        if (Velocity.x > 0.0f)
                            Velocity.x = Velocity.x - DRAG * Time.deltaTime * TimeScale.timeScale;
                        else if(Velocity.x < 0.0f)
                            Velocity.x = Velocity.x + DRAG * Time.deltaTime * TimeScale.timeScale;
                    }
                    else
                    {
                        Velocity.x = 0.0f;
                    }
                }
                else
                    Velocity.x = Velocity.x + characterAccel * Time.deltaTime * TimeScale.timeScale;

                Velocity.x = Mathf.Clamp(Velocity.x, -RUN_SPEED, RUN_SPEED);
            }
        }
        else
        {
            if (jumpTurned)
                HorizontalJumpVelNoAccel(SNEAK_SPEED);
            else
            {
                if (currentMoveState == moveState.isWalking || currentMoveState == moveState.isSneaking)
                {
                    HorizontalJumpVelAccel();
                }
                else if (currentMoveState == moveState.isRunning)
                {
                    HorizontalJumpVelAccel();
                }
            }
        }

        Velocity.x = Mathf.Clamp(Velocity.x, -MAX_HORIZONTAL_SPEED, MAX_HORIZONTAL_SPEED);

        if (Mathf.Approximately(Velocity.x - 1000000, -1000000))
        {
            Velocity.x = 0;
        }
    }

    protected void HorizontalJumpVelNoAccel(float speed)
    {
        if (characterAccel > 0.0f)
            Velocity.x = speed;
        else if (characterAccel < 0.0f)
            Velocity.x = -speed;
    }

    protected void HorizontalJumpVelAccel()
    {
        if (characterAccel < 0.0f && Velocity.x >= 0.0f)
        {
            Velocity.x = -SNEAK_SPEED;
        }
        else if (characterAccel > 0.0f && Velocity.x <= 0.0f)
        {
            Velocity.x = SNEAK_SPEED;
        }
        else
        {
            Velocity.x = Velocity.x + characterAccel * Time.deltaTime * TimeScale.timeScale;
            Velocity.x = Mathf.Clamp(Velocity.x, -JUMP_HORIZONTAL_SPEED_MAX, JUMP_HORIZONTAL_SPEED_MAX);
        }

    }

    private void VerticalVelocity()
    {
        Velocity.y = Velocity.y + GRAVITATIONAL_FORCE * Time.deltaTime * TimeScale.timeScale;

        //override the vertical velocity if we're in hte middle of jumping
        if (isJumping)
        {
            jumpInputTime = jumpInputTime + Time.deltaTime * Time.timeScale;
            if ((InputManager.JumpInput && jumpInputTime <= JUMP_CONTROL_TIME) || jumpInputTime <= JUMP_DURATION_MIN)
            {
                Velocity.y = JUMP_VERTICAL_SPEED;
            }
            else
            {
                isJumping = false;
            }
        }

        // if you turned while jumping, turn off the jump var
        if (jumpTurned && Velocity.y > 0.0f)
        {
            isJumping = false;
        }

        Velocity.y = Mathf.Clamp(Velocity.y, -MAX_VERTICAL_SPEED, MAX_VERTICAL_SPEED);

        if (OnTheGround && Mathf.Approximately(Velocity.y - 1000000, -1000000))
        {
            Velocity.y = 0;
        }
    }

	public virtual void Collisions()
    {
        // Horizontal Collision Block
        // box used to collide against horizontal objects. Extend the hitbox vertically while in the air to avoid corner clipping
        Vector2 horizontalBoxSize;
		if (OnTheGround)
			horizontalBoxSize = new Vector2 (0.01f, characterCollider.bounds.size.y - 0.01f);
		else
			horizontalBoxSize = new Vector2 (0.01f, characterCollider.bounds.size.y + 25.0f);//15.0f);

        // raycast to collide right
        Vector2 rightHitOrigin = new Vector2(characterCollider.bounds.center.x + characterCollider.bounds.extents.x - 0.01f, characterCollider.bounds.center.y);
        RaycastHit2D rightHit = Physics2D.BoxCast(rightHitOrigin, horizontalBoxSize, 0.0f, Vector2.right, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        float rightHitDist = Mathf.Infinity;
        if (rightHit.collider != null)
        {
            rightHitDist = rightHit.distance - 0.005f;
            if (Velocity.x > 0.0f && rightHitDist <= Mathf.Abs(Velocity.x))
                Velocity.x = rightHitDist;
        }

        // raycast to collide left
        Vector2 leftHitOrigin = new Vector2(characterCollider.bounds.center.x - characterCollider.bounds.extents.x + 0.01f, characterCollider.bounds.center.y);
        RaycastHit2D leftHit = Physics2D.BoxCast(leftHitOrigin, horizontalBoxSize, 0.0f, Vector2.left, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        float leftHitDist = Mathf.Infinity;
        if (leftHit.collider != null)
        {
            leftHitDist = leftHit.distance - 0.005f;
            if (Velocity.x < 0.0f && leftHitDist <= Mathf.Abs(Velocity.x))
                Velocity.x = -leftHitDist;
        }

        // are we touching the wall?
        RaycastHit2D centerRightHit = Physics2D.Raycast(characterCollider.bounds.center, Vector2.right, Mathf.Infinity, CollisionMasks.WallGrabMask);
        RaycastHit2D centerLeftHit = Physics2D.Raycast(characterCollider.bounds.center, Vector2.left, Mathf.Infinity, CollisionMasks.WallGrabMask);
        if (centerRightHit.collider != null && rightHit.collider != null && Mathf.Approximately(rightHitDist - 1000000, -1000000) && centerRightHit.collider.Equals(rightHit.collider))
            TouchedWall(centerRightHit.collider.gameObject);

        else if (centerLeftHit.collider != null && leftHit.collider != null && Mathf.Approximately(leftHitDist - 1000000, -1000000) && centerLeftHit.collider.Equals(leftHit.collider))
            TouchedWall(centerLeftHit.collider.gameObject);

		// Vertical Collision Block
        Vector2 verticalBoxSize = new Vector2(characterCollider.bounds.size.x - 0.01f, 0.01f);

        // raycast to hit the ceiling
        Vector2 upHitOrigin = new Vector2(characterCollider.bounds.center.x, characterCollider.bounds.center.y + characterCollider.bounds.extents.y - 0.01f);
        RaycastHit2D upHit = Physics2D.BoxCast(upHitOrigin, verticalBoxSize, 0.0f, Vector2.up, Mathf.Infinity, CollisionMasks.UpwardsCollisionMask);
        if (upHit.collider != null)
        {
            float hitDist = upHit.distance - 0.005f;
            if (Velocity.y > 0.0f && hitDist <= Mathf.Abs(Velocity.y))
                Velocity.y = hitDist;
        }

        // raycast to find the floor
        Vector2 downHitOrigin = new Vector2(characterCollider.bounds.center.x, characterCollider.bounds.center.y - characterCollider.bounds.extents.y + 0.01f);
        RaycastHit2D downHit = Physics2D.BoxCast(downHitOrigin, verticalBoxSize, 0.0f, Vector2.down, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        if (downHit.collider != null)
        {
            float downHitColliderLeft = downHit.collider.bounds.min.x;
            float downHitColliderRight = downHit.collider.bounds.max.x;
            float characterLeft = characterCollider.bounds.min.x;
            float characterRight = characterCollider.bounds.max.x;
            bool touchGround = true; // this is to prevent the game from thinking you touched the ground when you're gonna slip off the side when falling

            float hitDist = downHit.distance - 0.005f;
            if (Velocity.y < 0.0f && hitDist <= Mathf.Abs(Velocity.y))
            {
                // if the character is about to clip into the enviornment with the back of their hit box, move them so that they won't clip
                if (Velocity.x > 0.0f && downHitColliderRight < characterCollider.bounds.center.x)
                {
                    transform.Translate(downHitColliderRight - characterLeft, 0.0f, 0.0f);
                    touchGround = false;
                }
                else if (Velocity.x < 0.0f && downHitColliderLeft > characterCollider.bounds.center.x)
                {
                    transform.Translate(-(characterRight - downHitColliderLeft), 0.0f, 0.0f);
                    touchGround = false;
                }
                else // otherwise, touch the ground
                {
                    Velocity.y = -hitDist;
                }
            }
            // Approximate! since floats are dumb
            if (Mathf.Approximately(hitDist - 1000000, -1000000))
            {
                if (touchGround)
                {
                    OnTheGround = true;
                    jumpTurned = false;
                }
            }
            else
            {
                // this block is for jump tolerance
                if (OnTheGround && isJumping == false)
                {
                    jumpGracePeriod = true;
                    jumpGracePeriodTime = 0.0f;
                }
                OnTheGround = false;
                againstTheLedge = false;
            }

            // stop at the edge of a platform
            if (OnTheGround && currentMoveState != moveState.isRunning)
            {
                float rightLedgeDist = downHitColliderRight - characterRight;
                if (Velocity.x > 0.0f && rightLedgeDist <= Mathf.Abs(Velocity.x))
                {
                    if (characterRight < downHitColliderRight)
                        Velocity.x = rightLedgeDist;
                    else
                        Velocity.x = 0.0f;
                }

                float leftLedgeDist = characterLeft - downHitColliderLeft;
                if (Velocity.x < 0.0f && leftLedgeDist <= Mathf.Abs(Velocity.x))
                {
                    if (characterLeft > downHitColliderLeft)
                        Velocity.x = -leftLedgeDist;
                    else
                        Velocity.x = 0.0f;
                }

                // set if character is against the ledge
                if ((rightLedgeDist < 1.0f && FacingDirection == 1) || (leftLedgeDist < 1.0f && FacingDirection == -1))
                    againstTheLedge = true;
                else
                    againstTheLedge = false;
            }
        }
        // if there is no floor, just fall
        else
            OnTheGround = false;
    }

    void CalculateDirection()
    {
        // character direction logic
        if (FacingDirection == -1 && InputManager.HorizontalAxis > 0)
        {
            FacingDirection = 1;
            //Anim.SetBool("TurnAround", true);

            if (!OnTheGround)
            {
                jumpTurned = true;
            }
        }
        else if (FacingDirection == 1 && InputManager.HorizontalAxis < 0)
        {
            FacingDirection = -1;
            //Anim.SetBool("TurnAround", true);

            if (!OnTheGround)
            {
                jumpTurned = true;
            }
        }
    }

    protected void SetFacing()
    {
        if (FacingDirection == -1)
            SpriteRenderer.flipX = true;
        else
            SpriteRenderer.flipX = false;
    }

    protected void MovementInput()
    {
		LookingOverLedge();

        if(InputManager.RunInput)
            currentMoveState = moveState.isRunning;
        else
            currentMoveState = moveState.isSneaking;

        if(currentMoveState == moveState.isRunning)
        {
            // if the character comes to a full stop, let them start the run again
            // This also works when turning around
            if (Velocity.x == 0.0f)
                startRun = true;

            // running automatically starts at the sneaking speed and accelerates from there
            if (startRun == true && InputManager.HorizontalAxis > 0 && Mathf.Abs(Velocity.x) < SNEAK_SPEED)
            {
                Velocity.x = SNEAK_SPEED;
                startRun = false;
            }
            else if (startRun == true && InputManager.HorizontalAxis < 0 && Mathf.Abs(Velocity.x) < SNEAK_SPEED)
            {
                Velocity.x = -SNEAK_SPEED;
                startRun = false;
            }
        }
        
        if (InputManager.HorizontalAxis > 0)
            characterAccel = ACCELERATION;
        else if (InputManager.HorizontalAxis < 0)
            characterAccel = -ACCELERATION;
        else
            characterAccel = 0.0f;

        // Jump logic. Keep the Y velocity constant while holding jump for the duration of JUMP_CONTROL_TIME
        if ((jumpGracePeriod || OnTheGround) && InputManager.JumpInputInst)
        {
            isJumping = true;
            jumpGracePeriod = false;
            jumpInputTime = 0.0f;
        }
    }

    public virtual void LookingOverLedge()
    {
        // check if you're looking over the ledge
        if (againstTheLedge && InputManager.VerticalAxis < 0.0f)
            lookingOverLedge = true;
        else
            lookingOverLedge = false;
    }

    public virtual void TouchedWall(GameObject collisionObject)
    {
        //base class does nothing with this function. gets overridden at the subclass level to handle such occasions
    }

    protected Vector2 BezierCurveMovement(float distance, Vector2 start, Vector2 end, Vector2 curvePoint)
	{
		Vector2 ab = Vector2.Lerp(start, curvePoint, distance);
		Vector2 bc = Vector2.Lerp(curvePoint, end, distance);
		return Vector2.Lerp(ab, bc, distance);
	}

    

}
