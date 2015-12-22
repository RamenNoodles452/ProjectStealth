using UnityEngine;
using System.Collections;

public class SimpleCharacterCore : MonoBehaviour
{
    protected int previousFacingDirection = 1;
    public int FacingDirection = 1;
    public Animator Anim;
    public IInputManager InputManager;
    public Vector2 Velocity;
    public GameObject BottomCenterPoint;
    private BoxCollider2D characterCollider;
    
    public bool OnTheGround = false;
    const float MAX_FALL_SPEED = 10.0f;
    const float GRAVITATIONAL_FORCE = -30.0f;

    private const float JUMP_SPEED = 8.0f;
    private const float JUMP_CONTROL_TIME = 0.175f;
    private const float JUMP_DURATION_MIN = 0.05f;
    private bool canJump;
    private bool isJumping;
    private float jumpInputTime;

    // Use this for initialization
    public virtual void Start()
    {
        characterCollider = GetComponent<BoxCollider2D>();
        Anim = GetComponent<Animator>();
        InputManager = GetComponent<IInputManager>();

        BottomCenterPoint.transform.position = new Vector2(characterCollider.bounds.center.x, characterCollider.bounds.center.y - characterCollider.bounds.extents.y);
        Velocity = new Vector2(0.0f, 0.0f);
        canJump = false;
        isJumping = false;
        jumpInputTime = 0.0f;
    }

    public virtual void Update()
    {

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
        // If the character is touching the ground, allow jump
        if (OnTheGround)
        {
            canJump = true;
        }
            
        // Jump logic. Keep the Y velocity constant while holding jump for the duration of JUMP_CONTROL_TIME
        if (canJump && InputManager.JumpInputInst)
        {
            isJumping = true;
            canJump = false;
        }

    }

    private void VerticalVelocity()
    {
        Velocity.y = Mathf.Clamp(Velocity.y + GRAVITATIONAL_FORCE * Time.deltaTime * TimeScale.timeScale, -MAX_FALL_SPEED, MAX_FALL_SPEED);

        if (isJumping)
        {
            jumpInputTime = jumpInputTime + Time.deltaTime * Time.timeScale;
            if (jumpInputTime <= JUMP_DURATION_MIN) //|| (InputManager.JumpInputTime + JUMP_CONTROL_TIME > Time.time && InputManager.JumpInput))
            {
                Velocity.y = JUMP_SPEED;
            }
            else
            {
                print(jumpInputTime);
                isJumping = false;
                jumpInputTime = 0.0f;
            }
        }
    }

    private void Collisions()
    {
        //TODO: raycast from characterCollider.bounds.center you can use this for up down left right, no need for special point varsl

        //raycast to find the floor
        RaycastHit2D hit = Physics2D.Raycast(BottomCenterPoint.transform.position, Vector2.down, Mathf.Infinity, CollisionMasks.AllCollisionMask);
        if (Velocity.y < 0.0f && hit.distance <= Mathf.Abs(Velocity.y))
        {
            Velocity.y = -hit.distance;
        }

        if (Velocity.y <= 0.0f)
        {
            if (hit.distance == 0)
            {
                OnTheGround = true;
            }
        }
        else
        {
            OnTheGround = false;
        }
    }
}
