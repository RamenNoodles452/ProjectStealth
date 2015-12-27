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
    private const float JUMP_HORIZONTAL_SPEED = 3.0f;
    private const float JUMP_CONTROL_TIME = 0.17f;
    private const float JUMP_DURATION_MIN = 0.08f;
    private bool canJump;
    private bool isJumping;
    private float jumpInputTime;

    //walk and run vars
    private const float MAX_HORIZONTAL_SPEED = 10.0f;
    protected float WALK_SPEED = 1.0f; //used for cutscenes with Alice, guards will walk when not alerted
    protected float SNEAK_SPEED = 1.5f; //Alice's default speed, enemies that were walking will use this speed when on guard
    protected float RUN_SPEED = 4.0f;
    protected enum moveState { isWalking, isSneaking, isRunning };
    protected moveState currentMoveState = moveState.isWalking;

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
    }

    public virtual void Update()
    {
        // If the character is touching the ground, allow jump
        if (OnTheGround)
        {
            canJump = true;

            //movement stuff
            if (currentMoveState == moveState.isWalking)
                Velocity.x = WALK_SPEED * InputManager.HorizontalAxis;
            else if (currentMoveState == moveState.isSneaking)
                Velocity.x = SNEAK_SPEED * InputManager.HorizontalAxis;
            else if (currentMoveState == moveState.isRunning)
                Velocity.x = RUN_SPEED * InputManager.HorizontalAxis;
        }

        // Jump logic. Keep the Y velocity constant while holding jump for the duration of JUMP_CONTROL_TIME
        if (canJump && InputManager.JumpInputInst)
        {
            isJumping = true;
            canJump = false;
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
        Velocity.x = Mathf.Clamp(Velocity.x, -MAX_HORIZONTAL_SPEED, MAX_HORIZONTAL_SPEED);
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
                //print(jumpInputTime);
                isJumping = false;
                jumpInputTime = 0.0f;
            }
        }
    }

    private void Collisions()
    {
        //TODO: raycast from characterCollider.bounds.center you can use this for up down left right, no need for special point vars

        //raycast to find the floor
        RaycastHit2D hit = Physics2D.Raycast(characterCollider.bounds.center, Vector2.down, Mathf.Infinity, CollisionMasks.AllCollisionMask);

        if (hit.collider != null)
        {
            float hitDist = hit.distance - characterCollider.bounds.extents.y;

            if (Velocity.y < 0.0f && hitDist <= Mathf.Abs(Velocity.y))
            {
                print(hitDist);
                Velocity.y = -hitDist;
                OnTheGround = true;
            }

            // Approximate! since floats are dumb
            if (Mathf.Approximately(hitDist - 1000000, -1000000))
            {
                OnTheGround = true;
            }
            else
            {
                OnTheGround = false;
            }
        }
        
    }
}
