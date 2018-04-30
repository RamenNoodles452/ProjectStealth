using UnityEngine;
using System.Collections;

// Handles movement
public class SimpleCharacterCore : MonoBehaviour
{
    protected CharacterStats char_stats;
    protected CharacterAnimationLogic char_anims;

    private SpriteRenderer sprite_renderer;
	public IInputManager input_manager;

    //gravity vars
    private const float MAX_VERTICAL_SPEED = 10.0f;
    private const float GRAVITATIONAL_FORCE = -30.0f;

    //jump vars
    private const float JUMP_VERTICAL_SPEED = 6.0f;
    protected const float JUMP_HORIZONTAL_SPEED_MIN = 2.5f;
    protected const float JUMP_HORIZONTAL_SPEED_MAX = 4.0f;

    private const float JUMP_CONTROL_TIME = 0.20f; //maximum duration of a jump if you hold it
    private const float JUMP_DURATION_MIN = 0.10f; //minimum duration of a jump if you tap it
    private const float JUMP_GRACE_PERIOD_TIME = 0.1f; //how long a player has to jump if they slip off a platform
    [SerializeField]
    private bool jump_grace_period; //variable for jump tolerance if a player walks off a platform but wants to jump
    [SerializeField]
    private float jump_grace_period_timer;

    //walk and run vars
    private const float MAX_HORIZONTAL_SPEED = 10.0f;
    protected float WALK_SPEED = 1.0f; //used for cutscenes for PC, guards will walk when not alerted
    protected float SNEAK_SPEED = 2.0f; //default speed, enemies that were walking will use this speed when on guard
    protected float RUN_SPEED = 4.5f;
    protected float ACCELERATION = 6.0f; // acceleration used for velocity calcs when running
    protected float DRAG = 15.0f; // how quickly a character decelerates when running

    //protected float characterAccel = 0.0f;
    private bool start_run; // this bool prevents wonky shit from happening if you turn around during a run

    // ledge logic
    protected bool is_overlooking_ledge;
    protected bool is_against_ledge;
    protected bool fallthrough;

    // Use this for initialization
    public virtual void Start()
    {
		char_stats = GetComponent<CharacterStats>();
        input_manager = GetComponent<IInputManager>();
        char_anims = GetComponent<CharacterAnimationLogic>();

        // character sprite is now a child object. If a chracter sprite has multiple child sprite tho, this might break
        sprite_renderer = GetComponent<SpriteRenderer>();

        //Velocity = new Vector2(0.0f, 0.0f);
        jump_grace_period = false;
		jump_grace_period_timer = JUMP_GRACE_PERIOD_TIME;
        fallthrough = false;
    }

	// Called each frame
    public virtual void Update()
    {
        MovementInput();

        CalculateDirection();
        // set Sprite flip
        if (char_stats.previous_facing_direction != char_stats.facing_direction)
		{
            SetFacing();
		}

        // Give all characters gravity
        HorizontalVelocity();
        VerticalVelocity();
        Collisions();

        if (jump_grace_period == true)
        {
			jump_grace_period_timer = jump_grace_period_timer + Time.deltaTime * TimeScale.timeScale;
			if (jump_grace_period_timer >= JUMP_GRACE_PERIOD_TIME)
			{
                jump_grace_period = false;
			}
        }
    }

	// Called each frame, after update
    public virtual void LateUpdate()
    {
        // previous state assignments
        char_stats.previous_move_state = char_stats.current_move_state;
        char_stats.previous_facing_direction = char_stats.facing_direction;
    }

    public virtual void FixedUpdate()
    {
        //move the character after all calculations have been done
        if (char_stats.current_master_state == CharEnums.MasterState.DefaultState)
        {
            transform.Translate(char_stats.velocity);
            if (fallthrough == true)
            {
                transform.Translate(Vector3.down);
                fallthrough = false;
            }
        }
    }

    private void HorizontalVelocity()
    {
		if (char_stats.IsGrounded)
        {
            if (char_stats.is_crouching)
            {
                char_stats.velocity.x = 0.0f;
            }
            else
            {
                //movement stuff
                if (char_stats.current_move_state == CharEnums.MoveState.IsWalking)
				{
                    char_stats.velocity.x = char_stats.WALK_SPEED * input_manager.HorizontalAxis;
				}
                else if (char_stats.current_move_state == CharEnums.MoveState.IsSneaking)
                {
                    // if current speed is greater than sneak speed, then decel to sneak speed.
                    if (Mathf.Abs(char_stats.velocity.x) > char_stats.SNEAK_SPEED)
                    {
                        if (char_stats.velocity.x > 0.0f)
						{
                            char_stats.velocity.x = char_stats.velocity.x - DRAG * Time.deltaTime * TimeScale.timeScale;
						}
                        else if (char_stats.velocity.x < 0.0f)
						{
                            char_stats.velocity.x = char_stats.velocity.x + DRAG * Time.deltaTime * TimeScale.timeScale;
						}
                    }
                    else
                    {
                        char_stats.velocity.x = char_stats.SNEAK_SPEED * input_manager.HorizontalAxis;
                    }
                }
                else if (char_stats.current_move_state == CharEnums.MoveState.IsRunning)
                {
                    //smooth damp to 0 if there's no directional input for running or if you're trying to run the opposite direction
                    if (char_stats.character_acceleration == 0.0f || (char_stats.character_acceleration < 0.0f && char_stats.velocity.x > 0.0f) || (char_stats.character_acceleration > 0.0f && char_stats.velocity.x < 0.0f))
                    {
                        if (Mathf.Abs(char_stats.velocity.x) > char_stats.SNEAK_SPEED)
                        {
                            //print("SKID BOIS");
							if (char_stats.velocity.x > 0.0f) 
							{
								char_stats.velocity.x = char_stats.velocity.x - DRAG * Time.deltaTime * TimeScale.timeScale;
							}
                            else if (char_stats.velocity.x < 0.0f)
							{
                                char_stats.velocity.x = char_stats.velocity.x + DRAG * Time.deltaTime * TimeScale.timeScale;
							}
                        }
                        else
                        {
                            char_stats.velocity.x = 0.0f;
                        }
                    }
                    else
					{
                        char_stats.velocity.x = char_stats.velocity.x + char_stats.character_acceleration * Time.deltaTime * TimeScale.timeScale;
					}

                    char_stats.velocity.x = Mathf.Clamp(char_stats.velocity.x, -char_stats.RUN_SPEED, char_stats.RUN_SPEED);
                }
            }
        }
        else // character is in midair
        {
            if (char_stats.jump_turned)
			{
                HorizontalJumpVelNoAccel(char_stats.SNEAK_SPEED);
			}
            else
            {
                if (char_stats.current_move_state == CharEnums.MoveState.IsWalking || char_stats.current_move_state == CharEnums.MoveState.IsSneaking)
                {
                    HorizontalJumpVelAccel();
                }
                else if (char_stats.current_move_state == CharEnums.MoveState.IsRunning)
                {
                    HorizontalJumpVelAccel();
                }
            }
        }

        char_stats.velocity.x = Mathf.Clamp(char_stats.velocity.x, -MAX_HORIZONTAL_SPEED, MAX_HORIZONTAL_SPEED);

        if (Mathf.Approximately(char_stats.velocity.x - 1000000, -1000000)) //TODO: improve this, and all similar comparisons, with a better way.
        {
            char_stats.velocity.x = 0;
        }
    }

    public void HorizontalJumpVelNoAccel(float speed)
    {
		if (char_stats.character_acceleration > 0.0f) 
		{
			char_stats.velocity.x = speed;
		} 
		else if (char_stats.character_acceleration < 0.0f) 
		{
			char_stats.velocity.x = -speed;
		}
    }

    protected void HorizontalJumpVelAccel()
    {
        if (char_stats.character_acceleration < 0.0f && char_stats.velocity.x >= 0.0f)
        {
            char_stats.velocity.x = -char_stats.SNEAK_SPEED;
        }
        else if (char_stats.character_acceleration > 0.0f && char_stats.velocity.x <= 0.0f)
        {
            char_stats.velocity.x = char_stats.SNEAK_SPEED;
        }
        else
        {
            char_stats.velocity.x = char_stats.velocity.x + char_stats.character_acceleration * Time.deltaTime * TimeScale.timeScale;
            char_stats.velocity.x = Mathf.Clamp(char_stats.velocity.x, -JUMP_HORIZONTAL_SPEED_MAX, JUMP_HORIZONTAL_SPEED_MAX);
        }
    }

    private void VerticalVelocity()
    {
        char_stats.velocity.y = char_stats.velocity.y + GRAVITATIONAL_FORCE * Time.deltaTime * TimeScale.timeScale;

        //override the vertical velocity if we're in the middle of jumping
        if (char_stats.is_jumping)
        {
            char_stats.jump_input_time = char_stats.jump_input_time + Time.deltaTime * Time.timeScale;
            if ((input_manager.JumpInput && char_stats.jump_input_time <= JUMP_CONTROL_TIME) || char_stats.jump_input_time <= JUMP_DURATION_MIN)
            {
                char_stats.velocity.y = JUMP_VERTICAL_SPEED;
            }
            else
            {
                char_stats.is_jumping = false;
                char_anims.FallTrigger();
            }
        }

        // if you turned while jumping, turn off the jump var
        if (char_stats.jump_turned && char_stats.velocity.y > 0.0f)
        {
            char_stats.is_jumping = false;
            char_anims.FallTrigger();
        }

        char_stats.velocity.y = Mathf.Clamp(char_stats.velocity.y, -MAX_VERTICAL_SPEED, MAX_VERTICAL_SPEED);

		if (char_stats.IsGrounded && Mathf.Approximately(char_stats.velocity.y - 1000000, -1000000))
        {
            char_stats.velocity.y = 0;
        }
    }

	public virtual void Collisions()
    {
        // Horizontal Collision Block
        // box used to collide against horizontal objects. Extend the hitbox vertically while in the air to avoid corner clipping
        Vector2 horizontalBoxSize;
		if (char_stats.IsGrounded) 
		{
			horizontalBoxSize = new Vector2 (0.1f, char_stats.char_collider.bounds.size.y - 0.1f);
		}
		else
		{
            horizontalBoxSize = new Vector2 (0.1f, char_stats.char_collider.bounds.size.y + 10.0f);//15.0f);
		}

        // raycast to collide right
        if (char_stats.velocity.x > 0)
        {
            Vector2 rightHitOrigin = new Vector2(char_stats.char_collider.bounds.max.x - 0.1f, char_stats.char_collider.bounds.center.y);
			if (char_stats.IsInMidair)
            {
				if (char_stats.velocity.y > 0) 
				{
					rightHitOrigin = new Vector2 (char_stats.char_collider.bounds.max.x - 0.1f, char_stats.char_collider.bounds.center.y + 5f);
				}
                else
				{
                    rightHitOrigin = new Vector2(char_stats.char_collider.bounds.max.x - 0.1f, char_stats.char_collider.bounds.center.y - 5f);
				}
            }
            RaycastHit2D rightHit = Physics2D.BoxCast(rightHitOrigin, horizontalBoxSize, 0.0f, Vector2.right, 50.0f, CollisionMasks.upwards_collision_mask);
            if (rightHit.collider != null)
            {
                float rightHitDist = rightHit.distance - 0.05f;
                if (char_stats.velocity.x > 0.0f && rightHitDist <= Mathf.Abs(char_stats.velocity.x))
				{
                    char_stats.velocity.x = rightHitDist;
				}

                // are we touching the right wall?
                if (Mathf.Approximately(rightHitDist - 1000000, -1000000))
                {
                    char_stats.velocity.x = 0;
                    TouchedWall(rightHit.collider.gameObject);
					if (rightHit.collider.GetComponent<CollisionType> ().VaultObstacle == true && char_stats.IsGrounded) 
					{
						char_stats.is_touching_vault_obstacle = rightHit.collider;
					} 
					else 
					{
						char_stats.is_touching_vault_obstacle = null;
					}
                }
                else
                {
                    char_stats.is_touching_vault_obstacle = null;
                }
            }
            else
            {
                char_stats.is_touching_vault_obstacle = null;
            }
        }
        // raycast to collide left
        else if (char_stats.velocity.x < 0)
        {
            Vector2 leftHitOrigin = new Vector2(char_stats.char_collider.bounds.min.x + 0.1f, char_stats.char_collider.bounds.center.y);
			if (char_stats.IsInMidair)
            {
				if (char_stats.velocity.y > 0) 
				{
					leftHitOrigin = new Vector2 (char_stats.char_collider.bounds.min.x + 0.1f, char_stats.char_collider.bounds.center.y + 5f);
				}
				else 
				{
					leftHitOrigin = new Vector2 (char_stats.char_collider.bounds.min.x + 0.1f, char_stats.char_collider.bounds.center.y - 5f);
				}
            }
            RaycastHit2D leftHit = Physics2D.BoxCast(leftHitOrigin, horizontalBoxSize, 0.0f, Vector2.left, 50.0f, CollisionMasks.upwards_collision_mask);
            if (leftHit.collider != null)
            {
                float leftHitDist = leftHit.distance - 0.05f;
				if (char_stats.velocity.x < 0.0f && leftHitDist <= Mathf.Abs (char_stats.velocity.x)) 
				{
					char_stats.velocity.x = -leftHitDist;
				}

                // are we touching the left wall?
                if (Mathf.Approximately(leftHitDist - 1000000, -1000000))
                {
                    char_stats.velocity.x = 0;
                    TouchedWall(leftHit.collider.gameObject);
					if (leftHit.collider.GetComponent<CollisionType>().VaultObstacle == true && char_stats.IsGrounded)
					{
                        char_stats.is_touching_vault_obstacle = leftHit.collider;
					}
                    else
					{
                        char_stats.is_touching_vault_obstacle = null;
					}
                }
                else
                {
                    char_stats.is_touching_vault_obstacle = null;
                }
            }
            else
            {
                char_stats.is_touching_vault_obstacle = null;
            }
        }

        // Vertical Collision Block
        Vector2 verticalBoxSize = new Vector2(char_stats.char_collider.bounds.size.x - 0.1f, 0.1f);

        // raycast to hit the ceiling
        Vector2 upHitOrigin = new Vector2(char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.max.y - 0.1f);
        RaycastHit2D upHit = Physics2D.BoxCast(upHitOrigin, verticalBoxSize, 0.0f, Vector2.up, 50.0f, CollisionMasks.upwards_collision_mask);
        if (upHit.collider != null)
        {
            float hitDist = upHit.distance - 0.05f;
            if (char_stats.velocity.y > 0.0f && hitDist <= Mathf.Abs(char_stats.velocity.y))
			{
                char_stats.velocity.y = hitDist;
			}

            // are we touching the ceiling?
            if (Mathf.Approximately(hitDist - 1000000, -1000000))
            {
                //stop upward movement
                char_stats.is_jumping = false;
                TouchedCeiling(upHit.collider.gameObject);
            }
        }

        // raycast to find the floor
        Vector2 downHitOrigin = new Vector2(char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.min.y + 0.1f);
        RaycastHit2D downHit = Physics2D.BoxCast(downHitOrigin, verticalBoxSize, 0.0f, Vector2.down, 50.0f, CollisionMasks.all_collision_mask);
        if (downHit.collider != null)
        {
            float downHitColliderLeft = downHit.collider.bounds.min.x;
            float downHitColliderRight = downHit.collider.bounds.max.x;
            float characterLeft = char_stats.char_collider.bounds.min.x;
            float characterRight = char_stats.char_collider.bounds.max.x;
            bool touchGround = true; // this is to prevent the game from thinking you touched the ground when you're gonna slip off the side when falling

            float hitDist = downHit.distance - 0.05f;
            if (char_stats.velocity.y < 0.0f && hitDist <= Mathf.Abs(char_stats.velocity.y))
            {
                // if the character is about to clip into the enviornment with the back of their hit box, move them so that they won't clip
                if (char_stats.velocity.x > 0.0f && downHitColliderRight < char_stats.char_collider.bounds.center.x && downHit.transform.gameObject.GetComponent<CollisionType>().WalkOffRight == false)
                {
                    transform.Translate(downHitColliderRight - characterLeft, 0.0f, 0.0f);
                    touchGround = false;
                }
                else if (char_stats.velocity.x < 0.0f && downHitColliderLeft > char_stats.char_collider.bounds.center.x && downHit.transform.gameObject.GetComponent<CollisionType>().WalkOffLeft == false)
                {
                    transform.Translate(-(characterRight - downHitColliderLeft), 0.0f, 0.0f);
                    touchGround = false;
                }
                else // otherwise, touch the ground
                {
                    char_stats.velocity.y = -hitDist;
                }
            }
            //This logic allows characters to walk over connected platforms
			if ((char_stats.facing_direction == CharEnums.FacingDirection.Left && downHit.collider.gameObject.GetComponent<CollisionType>().WalkOffLeft == false) || 
				(char_stats.facing_direction == CharEnums.FacingDirection.Right && downHit.collider.gameObject.GetComponent<CollisionType>().WalkOffRight == false))
            {
                // stop at the edge of a platform
				if (char_stats.IsGrounded && char_stats.current_move_state != CharEnums.MoveState.IsRunning)
                {
                    float rightLedgeDist = downHitColliderRight - characterRight;
                    if (char_stats.velocity.x > 0.0f && rightLedgeDist <= Mathf.Abs(char_stats.velocity.x))
                    {
						if (characterRight < downHitColliderRight) 
						{
							char_stats.velocity.x = rightLedgeDist;
						}
                        else
						{
                            char_stats.velocity.x = 0.0f;
						}
                    }
                    float leftLedgeDist = characterLeft - downHitColliderLeft;
                    if (char_stats.velocity.x < 0.0f && leftLedgeDist <= Mathf.Abs(char_stats.velocity.x))
                    {
                        if (characterLeft > downHitColliderLeft)
						{
                            char_stats.velocity.x = -leftLedgeDist;
						}
                        else
						{
                            char_stats.velocity.x = 0.0f;
						}
                    }
                    // set if character is against the ledge
					if ((rightLedgeDist < 1.0f && char_stats.facing_direction == CharEnums.FacingDirection.Right) || (leftLedgeDist < 1.0f && char_stats.facing_direction == CharEnums.FacingDirection.Left))
					{
                        is_against_ledge = true;
					}
                    else
					{
                        is_against_ledge = false;
					}
                }
            }
            else
            {
                is_against_ledge = false;
            }
            // Approximate since floats are dumb
            if (Mathf.Approximately(hitDist - 1000000, -1000000))
            {
                if (touchGround)
                {
                    char_stats.is_on_ground = true;
                    char_stats.jump_turned = false;
                }
            }
            else
            {
                FallingLogic();
            }
            //Fallthrough platforms
            if (fallthrough == true)
            {
                if (downHit.collider.gameObject.GetComponent<CollisionType>().Fallthrough == false)
                {
                    fallthrough = false;
                }
                else
                {
                    // make sure that the player character is not straddling a solid platform
                    // issue can't fall down when straddling two fallthrough platforms 
                    //(but there shouldn't be a need to have two passthrough platforms touch, they can just merge into 1)
                    if ((downHit.collider.gameObject.GetComponent<CollisionType>().WalkOffRight == true && characterRight > downHitColliderRight) ||
                        (downHit.collider.gameObject.GetComponent<CollisionType>().WalkOffLeft == true && characterLeft < downHitColliderLeft))
                    {
                        fallthrough = false;
                    }
                }
            }
        }
        // if there is no floor, just fall
        else
        {
            FallingLogic();
        }
    }

	/// <summary>
	/// Called when the character is falling.
	/// Sets up a grace period where the character can still jump.
	/// </summary>
    void FallingLogic()
    {
        // this block is for jump grace period
		if (char_stats.IsGrounded && char_stats.is_jumping == false)
        {
            jump_grace_period = true;
            jump_grace_period_timer = 0.0f;
        }
        char_stats.is_on_ground = false;
        is_against_ledge = false;
    }

	/// <summary>
	/// Figures out which way the character should be facing, based on their movement, then sets their facing.
	/// Does NOT handle the sprite flipping, that's (allegedly) done by SetFacing
	/// </summary>
    void CalculateDirection()
    {
        // character direction logic
		bool turnAround = false;
		if (char_stats.facing_direction == CharEnums.FacingDirection.Left && input_manager.HorizontalAxis > 0.0f)
        {
			char_stats.facing_direction = CharEnums.FacingDirection.Right;
			turnAround = true;
        }
		else if (char_stats.facing_direction == CharEnums.FacingDirection.Right && input_manager.HorizontalAxis < 0.0f)
        {
			char_stats.facing_direction = CharEnums.FacingDirection.Left;
			turnAround = true;
        }

		if ( turnAround )
		{
			//Anim.SetBool("TurnAround", true);

			if ( char_stats.IsInMidair )
			{
				char_stats.jump_turned = true;
			}
		}
    }

    public void SetFacing()
    {
		if (char_stats.facing_direction == CharEnums.FacingDirection.Left)
		{
            sprite_renderer.flipX = true;
		}
        else
		{
            sprite_renderer.flipX = false;
		}
    }

    protected void MovementInput()
    {
		LookingOverLedge(); // checks if you are

        if (input_manager.RunInput)
		{
            char_stats.current_move_state = CharEnums.MoveState.IsRunning;
		}
        else
		{
            char_stats.current_move_state = CharEnums.MoveState.IsSneaking;
		}

        if(char_stats.current_move_state == CharEnums.MoveState.IsRunning)
        {
            // if the character comes to a full stop, let them start the run again
            // This also works when turning around
			if (char_stats.velocity.x == 0.0f) 
			{
				start_run = true;
				PlayerStats playerStats = GetComponent<PlayerStats> ();
				playerStats.StartWalking ();
			}

            // running automatically starts at the sneaking speed and accelerates from there
            if (start_run == true && input_manager.HorizontalAxis > 0 && Mathf.Abs(char_stats.velocity.x) < char_stats.SNEAK_SPEED)
            {
                char_stats.velocity.x = char_stats.SNEAK_SPEED;
                start_run = false;
            }
            else if (start_run == true && input_manager.HorizontalAxis < 0 && Mathf.Abs(char_stats.velocity.x) < char_stats.SNEAK_SPEED)
            {
                char_stats.velocity.x = -char_stats.SNEAK_SPEED;
                start_run = false;
            }
        }
        
        if (input_manager.HorizontalAxis > 0)
		{
            char_stats.character_acceleration = ACCELERATION;
		}
        else if (input_manager.HorizontalAxis < 0)
		{
            char_stats.character_acceleration = -ACCELERATION;
		}
        else
		{
            char_stats.character_acceleration = 0.0f;
		}

        // Jump logic. Keep the Y velocity constant while holding jump for the duration of JUMP_CONTROL_TIME
		if ((jump_grace_period || char_stats.IsGrounded) && input_manager.JumpInputInst)
        {
            if (input_manager.VerticalAxis < 0)
            {
                //trigger fallthrough
                fallthrough = true;
                char_anims.FallTrigger();
            }
            else
            {
                char_stats.is_jumping = true;
                jump_grace_period = false;
                char_stats.jump_input_time = 0.0f;
                char_anims.JumpTrigger();
            }
        }
    }

	/// <summary>
	/// Checks if the character is overlooking a ledge, and sets isLookingOverLedge appropriately.
	/// </summary>
    public virtual void LookingOverLedge()
    {
        // check if you're looking over the ledge
        if (is_against_ledge && input_manager.VerticalAxis < 0.0f)
		{
            is_overlooking_ledge = true;
		}
        else
		{
            is_overlooking_ledge = false;
		}
    }

    public virtual void TouchedWall(GameObject collisionObject)
    {
        //base class does nothing with this function. gets overridden at the subclass level to handle such occasions
    }

    public virtual void TouchedCeiling(GameObject collisionObject)
    {
        //base class does nothing with this function. gets overridden at the subclass level to handle such occasions
    }

    public float GetJumpHoriSpeedMin()
    {
        return JUMP_HORIZONTAL_SPEED_MIN;
    }

    public float GetJumpHoriSpeedMax()
    {
        return JUMP_HORIZONTAL_SPEED_MAX;
    }
}
