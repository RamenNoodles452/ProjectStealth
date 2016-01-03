using UnityEngine;
using System.Collections;

public class SimpleCharacterCore : MonoBehaviour
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
    private const float JUMP_CONTROL_TIME = 0.17f;
    private const float JUMP_DURATION_MIN = 0.08f;
    private bool canJump;
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
        canJump = false;
        isJumping = false;
        jumpInputTime = 0.0f;
        mustDecel = false;
    }

    public virtual void Update()
    {
        RunInput();

        // If the character is touching the ground, allow jump
        if (OnTheGround)
        {
            if (!isJumping)
                canJump = true;
        }

        // Jump logic. Keep the Y velocity constant while holding jump for the duration of JUMP_CONTROL_TIME
        if (canJump && InputManager.JumpInputInst)
        {
            isJumping = true;
            canJump = false;
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
                Velocity.x = JUMP_HORIZONTAL_SPEED * InputManager.HorizontalAxis;
            else if (currentMoveState == moveState.isRunning)
            {
                if (jumpTurned)
                    Velocity.x = JUMP_HORIZONTAL_SPEED * InputManager.HorizontalAxis;
                else
                    Velocity.x = JUMP_RUN_HORIZONTAL_SPEED * InputManager.HorizontalAxis;
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

    private void Collisions()
    {
        //TODO: raycast from characterCollider.bounds.center you can use this for up down left right, no need for special point vars
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
                OnTheGround = false;
        }
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
