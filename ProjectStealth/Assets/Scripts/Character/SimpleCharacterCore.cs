using UnityEngine;
using System.Collections;

public class SimpleCharacterCore : MonoBehaviour
{
    public BoxCollider tempshit;

    protected int previousFacingDirection = 1;
    public int FacingDirection = 1;
    private SpriteRenderer SpriteRenderer;
    public Animator Anim;
    public IInputManager InputManager;
    public Vector2 Velocity;
    private BoxCollider2D characterCollider;
    
    //gravity vars
    public bool OnTheGround = false;
    private const float MAX_VERTICAL_SPEED = 10.0f;
    private const float GRAVITATIONAL_FORCE = -30.0f;

    //jump vars
    private const float JUMP_VERTICAL_SPEED = 8.0f;
    private const float JUMP_HORIZONTAL_SPEED = 3.0f;
    private const float JUMP_RUN_HORIZONTAL_SPEED = 4.0f;
    private const float JUMP_CONTROL_TIME = 0.17f; //maximum duration of a jump if you hold it
    private const float JUMP_DURATION_MIN = 0.08f; //minimum duration of a jump if you tap it
    private const float JUMP_GRACE_PERIOD_TIME = 0.10f; //how long a player has to jump if they slip off a platform
    private bool jumpGracePeriod; //variable for jump tolerance if a player walks off a platform but wants to jump
    private float jumpGracePeriodTime;
    public bool isJumping;
    private float jumpInputTime;
    private bool jumpTurned;
    
    //walk and run vars
    private const float MAX_HORIZONTAL_SPEED = 4.5f;
    protected float WALK_SPEED = 1.0f; //used for cutscenes with Alice, guards will walk when not alerted
    protected float SNEAK_SPEED = 1.5f; //Alice's default speed, enemies that were walking will use this speed when on guard
    protected float RUN_SPEED = 4.0f;
    protected float ACCELERATION = 6.0f; // acceleration used for velocity calcs when running
    protected float DRAG = 15.0f; // how quickly a character decelerates when running
    protected enum moveState { isWalking, isSneaking, isRunning }; // protected
    protected moveState currentMoveState = moveState.isWalking;    // protected
    protected moveState tempMoveState;
    protected moveState prevMoveState;
    private float characterAccel = 0.0f;
    private bool mustDecel; // when a character starts running, they slow down to a stop
    private bool startRun; // this bool prevents wonky shit from happening if you turn around during a run

    // Use this for initialization
    public virtual void Start()
    {
        characterCollider = GetComponent<BoxCollider2D>();
        Anim = GetComponent<Animator>();
        InputManager = GetComponent<IInputManager>();
        SpriteRenderer = GetComponent<SpriteRenderer>();

        Velocity = new Vector2(0.0f, 0.0f);
        jumpGracePeriod = false;
        isJumping = false;
        jumpInputTime = 0.0f;
        mustDecel = false;
    }

    public virtual void Update()
    {
        RunInput();

        // Jump logic. Keep the Y velocity constant while holding jump for the duration of JUMP_CONTROL_TIME
        if ((jumpGracePeriod || OnTheGround) && InputManager.JumpInputInst)
        {
            isJumping = true;
            jumpGracePeriod = false;
            jumpInputTime = 0.0f;
            jumpTurned = false;
        }

        CalculateDirection();
        // set Sprite flip
        if (previousFacingDirection != FacingDirection)
            SetFacing();

        // prev state assignments
        prevMoveState = currentMoveState;
        previousFacingDirection = FacingDirection;
        if (jumpGracePeriod == true)
        {
            jumpGracePeriodTime = jumpGracePeriodTime + Time.deltaTime * TimeScale.timeScale;
            if (jumpGracePeriodTime >= JUMP_GRACE_PERIOD_TIME)
                jumpGracePeriod = false;
        }
            
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
                Velocity.x = SNEAK_SPEED * InputManager.HorizontalAxis;
            }
            else if (currentMoveState == moveState.isRunning)
            {   
                //smooth damp to 0 if there's no directional input for running or if you're trying to run the opposite direction
                if (characterAccel == 0.0f || (characterAccel < 0.0f && Velocity.x > 0.0f) || (characterAccel > 0.0f && Velocity.x < 0.0f))
                {
                    if (Mathf.Abs(Velocity.x) > SNEAK_SPEED)
                    {
                        //print("SKID BOIS");
                        Velocity.x = Mathf.SmoothDamp(Velocity.x, 0.0f, ref DRAG, 0.15f);
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
            if (currentMoveState == moveState.isWalking || currentMoveState == moveState.isSneaking)
            {
                HorizontalJumpVel(JUMP_HORIZONTAL_SPEED);
            }
            else if (currentMoveState == moveState.isRunning)
            {
                if (jumpTurned)
                    HorizontalJumpVel(JUMP_HORIZONTAL_SPEED);
                else
                    HorizontalJumpVel(JUMP_RUN_HORIZONTAL_SPEED);
            }
        }

        Velocity.x = Mathf.Clamp(Velocity.x, -MAX_HORIZONTAL_SPEED, MAX_HORIZONTAL_SPEED);

        if (Mathf.Approximately(Velocity.x - 1000000, -1000000))
        {
            Velocity.x = 0;
        }
    }

    private void VerticalVelocity()
    {
        Velocity.y = Mathf.Clamp(Velocity.y + GRAVITATIONAL_FORCE * Time.deltaTime * TimeScale.timeScale, -MAX_VERTICAL_SPEED, MAX_VERTICAL_SPEED);

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
    }

    private void HorizontalJumpVel(float moveSpeed)
    {
        if (characterAccel > 0.0f)
            Velocity.x = moveSpeed;
        else if (characterAccel < 0.0f)
            Velocity.x = -moveSpeed;
    }

    private void Collisions()
    {
        // box used to collide against horizontal objects. Extend the hitbox vertically while in the air to avoid corner clipping
        Vector2 horizontalBoxSize;
        if (OnTheGround)
            horizontalBoxSize = new Vector2(characterCollider.bounds.extents.x - 0.01f, characterCollider.bounds.size.y - 0.01f);
        else
            horizontalBoxSize = new Vector2(characterCollider.bounds.extents.x - 0.01f, characterCollider.bounds.size.y + 20.0f);


        // raycast to collide right
        Vector2 rightHitOrigin = characterCollider.bounds.center + new Vector3(characterCollider.bounds.extents.x / 2.0f, 0.0f);
        RaycastHit2D rightHit = Physics2D.BoxCast(rightHitOrigin, horizontalBoxSize, 0.0f, Vector2.right, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        if (rightHit.collider != null)
        {
            float hitDist = rightHit.distance - 0.005f;
            if (Velocity.x > 0.0f && hitDist <= Mathf.Abs(Velocity.x))
                Velocity.x = hitDist;
        }

        // raycast to collide left
        Vector2 leftHitOrigin = characterCollider.bounds.center + new Vector3(-characterCollider.bounds.extents.x / 2.0f, 0.0f);
        RaycastHit2D leftHit = Physics2D.BoxCast(leftHitOrigin, horizontalBoxSize, 0.0f, Vector2.left, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        if (leftHit.collider != null)
        {
            float hitDist = leftHit.distance - 0.005f;
            if (Velocity.x < 0.0f && hitDist <= Mathf.Abs(Velocity.x))
                Velocity.x = -hitDist;
        }

        Vector2 verticalBoxSize = new Vector2(characterCollider.bounds.size.x - 0.01f, characterCollider.bounds.extents.y - 0.01f);
        /*
        if (OnTheGround)
            verticalBoxSize = new Vector2(characterCollider.bounds.size.x - 0.01f, characterCollider.bounds.extents.y - 0.01f);
        else
            verticalBoxSize = new Vector2(characterCollider.bounds.extents.x - 0.01f, characterCollider.bounds.extents.y - 0.01f);
        */

        // raycast to hit the ceiling
        Vector2 upHitOrigin = characterCollider.bounds.center + new Vector3(0.0f, characterCollider.bounds.extents.y / 2.0f, 0.0f);
        /*
        if (!OnTheGround && Mathf.Abs(Velocity.x) > 0.0f && FacingDirection == 1)
            upHitOrigin = characterCollider.bounds.center + new Vector3(characterCollider.bounds.extents.x / 2.0f, characterCollider.bounds.extents.y / 2.0f, 0.0f);
        else if (!OnTheGround && Mathf.Abs(Velocity.x) > 0.0f && FacingDirection == -1)
            upHitOrigin = characterCollider.bounds.center + new Vector3(-characterCollider.bounds.extents.x / 2.0f, characterCollider.bounds.extents.y / 2.0f, 0.0f);
        else
            upHitOrigin = characterCollider.bounds.center + new Vector3(0.0f, characterCollider.bounds.extents.y / 2.0f, 0.0f);
        */

        RaycastHit2D upHit = Physics2D.BoxCast(upHitOrigin, verticalBoxSize, 0.0f, Vector2.up, Mathf.Infinity, CollisionMasks.UpwardsCollisionMask);
        if (upHit.collider != null)
        {
            float hitDist = upHit.distance - 0.005f;
            if (Velocity.y > 0.0f && hitDist <= Mathf.Abs(Velocity.y))
                Velocity.y = hitDist;
        }

        // raycast to find the floor
        Vector2 downHitOrigin = characterCollider.bounds.center + new Vector3(0.0f, -characterCollider.bounds.extents.y / 2.0f, 0.0f);
        /*
        if (!OnTheGround && Mathf.Abs(Velocity.x) > 0.0f && FacingDirection == 1)
            downHitOrigin = characterCollider.bounds.center + new Vector3(characterCollider.bounds.extents.x / 2.0f, -characterCollider.bounds.extents.y / 2.0f, 0.0f);
        else if (!OnTheGround && Mathf.Abs(Velocity.x) > 0.0f && FacingDirection == -1)
            downHitOrigin = characterCollider.bounds.center + new Vector3(-characterCollider.bounds.extents.x / 2.0f, -characterCollider.bounds.extents.y / 2.0f, 0.0f);
        else
            downHitOrigin = characterCollider.bounds.center + new Vector3(0.0f, -characterCollider.bounds.extents.y / 2.0f, 0.0f);
        */
        RaycastHit2D downHit = Physics2D.BoxCast(downHitOrigin, verticalBoxSize, 0.0f, Vector2.down, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        if (downHit.collider != null)
        {
            float hitDist = downHit.distance - 0.005f;
            if (Velocity.y < 0.0f && hitDist <= Mathf.Abs(Velocity.y))
                Velocity.y = -hitDist;

            // Approximate! since floats are dumb
            if (Mathf.Approximately(hitDist - 1000000, -1000000))
                OnTheGround = true;
            else
            {
                // this block is for jump tolerance
                if (OnTheGround && isJumping == false)
                {
                    jumpGracePeriod = true;
                    jumpGracePeriodTime = 0.0f;
                }
                OnTheGround = false;
            }
        }

        // stop at the edge of a platform

        //DEBUGGING STUFF
        tempshit.size = horizontalBoxSize;
        tempshit.transform.position = rightHitOrigin;

    }

    void CalculateDirection()
    {
        // character direction logic
        if (FacingDirection == -1 && InputManager.HorizontalAxis > 0)
        {
            FacingDirection = 1;
            //Anim.SetBool("TurnAround", true);

            if (!OnTheGround)
                jumpTurned = true;
        }
        else if (FacingDirection == 1 && InputManager.HorizontalAxis < 0)
        {
            FacingDirection = -1;
            //Anim.SetBool("TurnAround", true);

            if (!OnTheGround)
                jumpTurned = true;
        }
    }

    void SetFacing()
    {
        if (FacingDirection == -1)
            SpriteRenderer.flipX = true;
        else
            SpriteRenderer.flipX = false;
    }

    void RunInput()
    {
        if (InputManager.RunInputInst && currentMoveState != moveState.isRunning)
        {
            tempMoveState = currentMoveState;
            currentMoveState = moveState.isRunning;
            startRun = true;
            mustDecel = true;
        }

        if (mustDecel == true)
        {
            if (InputManager.HorizontalAxis == 0.0f && Velocity.x == 0.0f)
            {
                if (OnTheGround)
                {
                    currentMoveState = tempMoveState;
                    mustDecel = false;
                }
            }
            else
            {
                // if the character comes to a full stop, let them start the run again
                // This also works when turnning around
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
        }
        
        if (InputManager.HorizontalAxis > 0)
            characterAccel = ACCELERATION;
        else if (InputManager.HorizontalAxis < 0)
            characterAccel = -ACCELERATION;
        else
            characterAccel = 0.0f;
    }

}
