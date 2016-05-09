using UnityEngine;
using System.Collections;

public class tempfile : MonoBehaviour
{
    protected int previousFacingDirection = 1;
    public int FacingDirection = 1;
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
    private const float JUMP_HORIZONTAL_SPEED = 2.0f;
    private const float JUMP_RUN_HORIZONTAL_SPEED = 3.5f;
    private const float JUMP_CONTROL_TIME = 0.17f; //maximum duration of a jump if you hold it
    private const float JUMP_DURATION_MIN = 0.08f; //minimum duration of a jump if you tap it
    private const float JUMP_GRACE_PERIOD_TIME = 0.10f; //how long a player has to jump if they slip off a platform
    private bool jumpGracePeriod; //variable for jump tolerance if a player walks off a platform but wants to jump
    private float jumpGracePeriodTime;
    private bool isJumping;
    private float jumpInputTime;
    private bool jumpTurned;

    //walk and run vars
    private const float MAX_HORIZONTAL_SPEED = 4.0f;
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
        if (OnTheGround)
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
        //TODO: Make character collide from both corners to detect collisions

        // raycast to collide right
        RaycastHit2D rightHit = Physics2D.Raycast(characterCollider.bounds.center, Vector2.right, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        if (rightHit.collider != null)
        {
            float hitDist = rightHit.distance - characterCollider.bounds.extents.x;
            if (Velocity.x > 0.0f && hitDist <= Mathf.Abs(Velocity.x))
                Velocity.x = hitDist;
        }

        // raycast to collide left
        RaycastHit2D leftHit = Physics2D.Raycast(characterCollider.bounds.center, Vector2.left, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        if (leftHit.collider != null)
        {
            float hitDist = leftHit.distance - characterCollider.bounds.extents.x;
            if (Velocity.x < 0.0f && hitDist <= Mathf.Abs(Velocity.x))
                Velocity.x = -hitDist;
        }

        // raycast to hit the ceiling
        RaycastHit2D upHit = Physics2D.Raycast(characterCollider.bounds.center, Vector2.up, Mathf.Infinity, CollisionMasks.UpwardsCollisionMask);
        if (upHit.collider != null)
        {
            float hitDist = upHit.distance - characterCollider.bounds.extents.y;
            if (Velocity.y > 0.0f && hitDist <= Mathf.Abs(Velocity.y))
                Velocity.y = hitDist;
        }

        // raycast to find the floor
        RaycastHit2D downHit = Physics2D.Raycast(characterCollider.bounds.center, Vector2.down, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        if (downHit.collider != null)
        {
            float hitDist = downHit.distance - characterCollider.bounds.extents.y;
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

        /*
        //vector points to use for vertical raycasting
        Vector2 centerLeft = characterCollider.bounds.center + new Vector3(-characterCollider.bounds.extents.x, 0.0f, 0.0f);
        Vector2 centerRight = characterCollider.bounds.center + new Vector3(characterCollider.bounds.extents.x, 0.0f, 0.0f);

        // raycast to hit the ceiling
        RaycastHit2D upHitLeft = Physics2D.Raycast(centerLeft, Vector2.up, Mathf.Infinity, CollisionMasks.UpwardsCollisionMask);
        RaycastHit2D upHitRight = Physics2D.Raycast(centerRight, Vector2.up, Mathf.Infinity, CollisionMasks.UpwardsCollisionMask);
        if (upHitLeft.collider != null || upHitRight.collider != null)
        {
            float hitDist = 0.0f;
            if (upHitLeft.collider == null)
                hitDist = upHitRight.distance - characterCollider.bounds.extents.y;
            else if (upHitRight.collider == null)
                hitDist = upHitLeft.distance - characterCollider.bounds.extents.y;
            else
            {
                if (upHitLeft.distance < upHitRight.distance)
                    hitDist = upHitLeft.distance - characterCollider.bounds.extents.y;
                else
                    hitDist = upHitRight.distance - characterCollider.bounds.extents.y;
            }

            if (Velocity.y > 0.0f && hitDist <= Mathf.Abs(Velocity.y))
                Velocity.y = hitDist;

        }
        
        // raycast to find the floor
        RaycastHit2D downHitLeft = Physics2D.Raycast(centerLeft, Vector2.down, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        RaycastHit2D downHitRight = Physics2D.Raycast(centerRight, Vector2.down, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        if (downHitLeft.collider != null || downHitRight.collider != null)
        {
            float hitDist = 0.0f;
            if (downHitLeft.collider == null)
                hitDist = downHitRight.distance - characterCollider.bounds.extents.y;
            else if (downHitRight.collider == null)
                hitDist = downHitLeft.distance - characterCollider.bounds.extents.y;
            else
            {
                if (downHitLeft.distance < downHitRight.distance)
                    hitDist = downHitLeft.distance - characterCollider.bounds.extents.y;
                else
                    hitDist = downHitRight.distance - characterCollider.bounds.extents.y;
            }
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
        // if no raycast collisions on either side, you're falling into infinity
        else
        {
            OnTheGround = false;
        }
        */
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
        Vector3 theScale = transform.localScale;
        theScale.x = FacingDirection;
        transform.localScale = theScale;
    }

    void RunInput()
    {
        if (InputManager.RunInputDownInst && currentMoveState != moveState.isRunning)
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
