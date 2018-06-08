using UnityEngine;
using System.Collections;

// Handles movement
public class SimpleCharacterCore : MonoBehaviour
{
	#region vars
    protected CharacterStats char_stats;
    protected CharacterAnimationLogic char_anims;

    private SpriteRenderer sprite_renderer;
	public IInputManager input_manager;

    //gravity vars
	private const float MAX_VERTICAL_SPEED          =   600.0f; // (pixels / second)
	private const float GRAVITATIONAL_ACCELERATION  = -1800.0f; // (pixels / second / second)

    //jump vars
	protected const float JUMP_HORIZONTAL_SPEED_MIN =   150.0f; // (pixels / seond)
	protected const float JUMP_HORIZONTAL_SPEED_MAX =   240.0f; // (pixels / second)
	private   const float JUMP_VERTICAL_SPEED       =   360.0f; // (pixels / second)
	private   const float JUMP_CONTROL_TIME         =     0.2f; // maximum duration of a jump (in seconds) if you hold it
	private   const float JUMP_DURATION_MIN         =     0.1f; // minimum duration of a jump (in seconds) if you tap it
	private   const float JUMP_GRACE_PERIOD_TIME    =     0.1f; // how long (in seconds) a player has to jump if they slip off a platform
    [SerializeField]
	private bool jump_grace_period; //for jump tolerance if a player walks off a platform but wants to jump
    [SerializeField]
    private float jump_grace_period_timer;

    //walk and run vars
	private const float MAX_HORIZONTAL_SPEED = 600.0f; // pixels per second
	protected float WALK_SPEED   =  60.0f; // used for cutscenes for PC, guards will walk when not alerted (pixels / second)
	protected float SNEAK_SPEED  = 120.0f; // default speed, enemies that were walking will use this speed when on guard (pixels / second)
	protected float RUN_SPEED    = 270.0f; // (pixels / second)
	protected float ACCELERATION = 360.0f; // acceleration used for velocity calcs when running (pixels / second / second)
	protected float DRAG         = 900.0f; // how quickly a character decelerates when running  (pixels / second / second)
    private bool start_run;                // prevents wonky shit from happening if you turn around during a run

    // ledge logic
    protected bool is_overlooking_ledge;
    protected bool is_against_ledge;
    protected bool fallthrough;

	private const float APPROXIMATE_EQUALITY_MARGIN = 0.001f; //Mathf.Epsilon;
	#endregion

	#region virtual overrides
    // Use this for initialization
    public virtual void Start()
    {
		char_stats      = GetComponent<CharacterStats>();
        input_manager   = GetComponent<IInputManager>();
        char_anims      = GetComponent<CharacterAnimationLogic>();
        sprite_renderer = GetComponent<SpriteRenderer>();

        jump_grace_period = false;
		jump_grace_period_timer = JUMP_GRACE_PERIOD_TIME;
        fallthrough = false;
    }

	// Called each frame
    public virtual void Update()
    {
        MovementInput();
        CalculateDirection();
        HorizontalVelocity();
        VerticalVelocity();
        Collisions();
		UpdateGracePeriod();
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
        if ( char_stats.current_master_state == CharEnums.MasterState.DefaultState )
        {
			transform.Translate( char_stats.velocity * Time.fixedDeltaTime * Time.timeScale );
            if (fallthrough == true)
            {
				transform.Translate( Vector3.down ); // move 1 pixel down.
                fallthrough = false;
            }
        }
    }
	#endregion


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
                if (char_stats.IsWalking)
				{
                    char_stats.velocity.x = char_stats.WALK_SPEED * input_manager.HorizontalAxis;
				}
                else if (char_stats.IsSneaking)
                {
                    // if current speed is greater than sneak speed, then decel to sneak speed.
                    if (Mathf.Abs(char_stats.velocity.x) > char_stats.SNEAK_SPEED)
                    {
						IncreaseMagnitude( ref char_stats.velocity.x, - DRAG * Time.deltaTime * Time.timeScale );
                    }
                    else
                    {
						// TODO: FIX BUG?: smooth?
						// If a character is moving with 50% axis input, they'll smooth to the sneak speed, then snap to half of it. That seems silly.
                        char_stats.velocity.x = char_stats.SNEAK_SPEED * input_manager.HorizontalAxis;
                    }
                }
                else if (char_stats.IsRunning)
                {
                    //smooth damp to 0 if there's no directional input for running or if you're trying to run the opposite direction
                    if ( char_stats.acceleration.x == 0.0f || 
						(char_stats.acceleration.x < 0.0f && char_stats.velocity.x > 0.0f) || 
						(char_stats.acceleration.x > 0.0f && char_stats.velocity.x < 0.0f))
                    {
                        if (Mathf.Abs(char_stats.velocity.x) > char_stats.SNEAK_SPEED)
                        {
                            //print("SKID BOIS");
							IncreaseMagnitude( ref char_stats.velocity.x, - DRAG * Time.deltaTime * Time.timeScale );
                        }
                        else
                        {
                            char_stats.velocity.x = 0.0f;
                        }
                    }
                    else
					{
                        char_stats.velocity.x += char_stats.acceleration.x * Time.deltaTime * Time.timeScale;
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

		if (IsAlmostZero(char_stats.velocity.x))
        {
            char_stats.velocity.x = 0.0f;
        }
    }


	/// <summary>
	/// Magnitude increasing function. Adds the increment to value if value is positive.
	/// Subtracts the increment from value if value is negative.
	/// </summary>
	/// <param name="value">The value to add to or subtract from. 
	///     The sign of value dictates whether the increment will be added or subtracted. 
	///     The result is returned in value.</param>
	/// <param name="increment">The amount to add to value when value is positive OR ZERO. 
	///     The opposite will be added when value is negative.</param>
	private void IncreaseMagnitude( ref float value, float increment )
	{
		if ( value >= 0.0f ) // I guess we'll consider 0 positive?
		{
			value += increment;
		}
		else if ( value < 0.0f )
		{
			value -= increment;
		}
	}

	//TODO: improvement target. Rename, make character_acceleration horizontal/a vector, document
    public void HorizontalJumpVelNoAccel(float speed)
    {
		if (char_stats.acceleration.x > 0.0f) 
		{
			char_stats.velocity.x = speed;
		} 
		else if (char_stats.acceleration.x < 0.0f) 
		{
			char_stats.velocity.x = -speed;
		}
    }

    protected void HorizontalJumpVelAccel()
    {
        if (char_stats.acceleration.x < 0.0f && char_stats.velocity.x >= 0.0f)
        {
            char_stats.velocity.x = -char_stats.SNEAK_SPEED;
        }
        else if (char_stats.acceleration.x > 0.0f && char_stats.velocity.x <= 0.0f)
        {
            char_stats.velocity.x = char_stats.SNEAK_SPEED;
        }
        else
        {
            char_stats.velocity.x += char_stats.acceleration.x * Time.deltaTime * Time.timeScale;
            char_stats.velocity.x = Mathf.Clamp(char_stats.velocity.x, -JUMP_HORIZONTAL_SPEED_MAX, JUMP_HORIZONTAL_SPEED_MAX);
        }
    }

    private void VerticalVelocity()
    {
        char_stats.velocity.y += GRAVITATIONAL_ACCELERATION * Time.deltaTime * Time.timeScale;

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

		if (char_stats.IsGrounded && IsAlmostZero(char_stats.velocity.y))
        {
            char_stats.velocity.y = 0.0f;
        }
    }

	public virtual void Collisions()
    {
		CheckCollisionHorizontal();
		CheckCollisionVertical();
    }

	private void CheckCollisionHorizontal()
	{
		// box used to collide against horizontal objects. Extend the hitbox vertically while in the air to avoid corner clipping
		Vector2 box_size;
		if (char_stats.IsGrounded) 
		{
			box_size = new Vector2 (1.0f, char_stats.char_collider.bounds.size.y); // 1px x height box
		}
		else
		{
			box_size = new Vector2 (1.0f, char_stats.char_collider.bounds.size.y + 10.0f); // this expands 5 pixels in each direction from the middle. //TODO: magic number refactor
		}

		// raycast to collide right
		if (char_stats.velocity.x > 0.0f)
		{
			Vector2 right_hit_origin = new Vector2(char_stats.char_collider.bounds.max.x - box_size.x, char_stats.char_collider.bounds.center.y);
			if (char_stats.IsInMidair)
			{
				if (char_stats.velocity.y > 0.0f) 
				{
					right_hit_origin = new Vector2 (char_stats.char_collider.bounds.max.x - box_size.x, char_stats.char_collider.bounds.center.y + 5.0f); // TODO: magic numbers
				}
				else
				{
					right_hit_origin = new Vector2(char_stats.char_collider.bounds.max.x - box_size.x, char_stats.char_collider.bounds.center.y - 5.0f); //TODO: magic numbers
				}
			}
			char_stats.is_touching_vault_obstacle = null;
			RaycastHit2D right_hit = Physics2D.BoxCast(right_hit_origin, box_size, 0.0f, Vector2.right, 50.0f, CollisionMasks.upwards_collision_mask);
			if (right_hit.collider != null)
			{
				float right_hit_distance = right_hit.distance - 0.05f; // TODO: magic number
				if (char_stats.velocity.x > 0.0f && right_hit_distance <= Mathf.Abs( char_stats.velocity.x * Time.fixedDeltaTime * Time.timeScale ))
				{
					char_stats.velocity.x = right_hit_distance / (Time.fixedDeltaTime * Time.timeScale);
				}

				// are we touching the right wall?
				if (IsAlmostZero(right_hit_distance))
				{
					char_stats.velocity.x = 0.0f;
					TouchedWall(right_hit.collider.gameObject);

					CollisionType right_hit_collision_type = right_hit.collider.GetComponent<CollisionType>();
					if ( right_hit_collision_type != null )
					{
						if (right_hit_collision_type.VaultObstacle == true && char_stats.IsGrounded) 
						{
							char_stats.is_touching_vault_obstacle = right_hit.collider;
						}
				    }
				}
			}
		}

		// raycast to collide left
		// TODO: REFACTOR. DRY violation
		else if (char_stats.velocity.x < 0.0f)
		{
			Vector2 left_hit_origin = new Vector2(char_stats.char_collider.bounds.min.x + box_size.x, char_stats.char_collider.bounds.center.y);
			if (char_stats.IsInMidair)
			{
				if (char_stats.velocity.y > 0.0f) 
				{
					left_hit_origin = new Vector2 (char_stats.char_collider.bounds.min.x + box_size.x, char_stats.char_collider.bounds.center.y + 5.0f); // TODO: magic number
				}
				else 
				{
					left_hit_origin = new Vector2 (char_stats.char_collider.bounds.min.x + box_size.x, char_stats.char_collider.bounds.center.y - 5.0f); //TODO: magic number
				}
			}
			char_stats.is_touching_vault_obstacle = null;
			RaycastHit2D leftHit = Physics2D.BoxCast(left_hit_origin, box_size, 0.0f, Vector2.left, 50.0f, CollisionMasks.upwards_collision_mask);
			if (leftHit.collider != null)
			{
				float left_hit_distance = leftHit.distance - 0.05f;  // TODO: magic number
				if (char_stats.velocity.x < 0.0f && leftHit.distance <= Mathf.Abs (char_stats.velocity.x * Time.fixedDeltaTime * Time.timeScale))
				{
					char_stats.velocity.x = -left_hit_distance / (Time.fixedDeltaTime * Time.timeScale);
				}

				// are we touching the left wall?
				if (IsAlmostZero(left_hit_distance))
				{
					char_stats.velocity.x = 0.0f;
					TouchedWall(leftHit.collider.gameObject);
					CollisionType left_hit_collision_type = leftHit.collider.GetComponent<CollisionType>();
					if (left_hit_collision_type != null)
					{
					    if (left_hit_collision_type.VaultObstacle == true && char_stats.IsGrounded)
						{
							char_stats.is_touching_vault_obstacle = leftHit.collider;
						}
					}
				}
			}
		}
	}

	private void CheckCollisionVertical()
	{
		// Vertical Collision Block
		Vector2 box_size = new Vector2(char_stats.char_collider.bounds.size.x, 1.0f); // width x 1 px box

		// raycast to hit the ceiling
		Vector2 up_hit_origin = new Vector2(char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.max.y - box_size.y);
		RaycastHit2D up_hit = Physics2D.BoxCast(up_hit_origin, box_size, 0.0f, Vector2.up, 50.0f, CollisionMasks.upwards_collision_mask);
		if (up_hit.collider != null)
		{
			float hit_distance = up_hit.distance - 0.05f; //TODO: magic number
			if (char_stats.velocity.y > 0.0f && hit_distance <= Mathf.Abs(char_stats.velocity.y * Time.fixedDeltaTime * Time.timeScale))
			{
				char_stats.velocity.y = hit_distance / (Time.fixedDeltaTime * Time.timeScale);
			}

			// are we touching the ceiling?
			if (IsAlmostZero(hit_distance))
			{
				//stop upward movement
				char_stats.is_jumping = false;
				TouchedCeiling(up_hit.collider.gameObject);
			}
		}

		// raycast to find the floor
		Vector2 down_hit_origin = new Vector2(char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.min.y + box_size.y);
		RaycastHit2D down_hit = Physics2D.BoxCast(down_hit_origin, box_size, 0.0f, Vector2.down, 50.0f, CollisionMasks.all_collision_mask);
		if (down_hit.collider != null)
		{
			float down_hit_collider_left = down_hit.collider.bounds.min.x;
			float down_hit_collider_right = down_hit.collider.bounds.max.x;
			float character_left = char_stats.char_collider.bounds.min.x;
			float character_right = char_stats.char_collider.bounds.max.x;
			bool touch_ground = true; // this is to prevent the game from thinking you touched the ground when you're gonna slip off the side when falling
			CollisionType down_hit_collision_type = down_hit.transform.gameObject.GetComponent<CollisionType>();

			float hit_distance = down_hit.distance - 0.05f; //TODO: magic number
			if (char_stats.velocity.y < 0.0f && hit_distance <= Mathf.Abs(char_stats.velocity.y * Time.fixedDeltaTime * Time.timeScale))
			{
				// if the character is about to clip into the environment with the back of their hit box, move them so that they won't clip
				if ( down_hit_collision_type != null )
				{
					if (char_stats.velocity.x > 0.0f && down_hit_collider_right < char_stats.char_collider.bounds.center.x && down_hit_collision_type.WalkOffRight == false)
					{
						transform.Translate(down_hit_collider_right - character_left, 0.0f, 0.0f);
						touch_ground = false;
					}
					else if (char_stats.velocity.x < 0.0f && down_hit_collider_left > char_stats.char_collider.bounds.center.x && down_hit_collision_type.WalkOffLeft == false)
					{
						transform.Translate(-(character_right - down_hit_collider_left), 0.0f, 0.0f);
						touch_ground = false;
					}
					else // otherwise, touch the ground
					{
						char_stats.velocity.y = -hit_distance / (Time.fixedDeltaTime * Time.timeScale);
					}
				}
				else
				{
					char_stats.velocity.y = -hit_distance / (Time.fixedDeltaTime * Time.timeScale);
				}
			}
			//This logic allows characters to walk over connected platforms
			is_against_ledge = false;
			if ( down_hit_collision_type != null )
			{
				if ((char_stats.facing_direction == CharEnums.FacingDirection.Left && down_hit_collision_type.WalkOffLeft == false) || 
					(char_stats.facing_direction == CharEnums.FacingDirection.Right && down_hit_collision_type.WalkOffRight == false))
				{
					// stop at the edge of a platform
					if (char_stats.IsGrounded && char_stats.current_move_state != CharEnums.MoveState.IsRunning)
					{
						float right_ledge_distance = down_hit_collider_right - character_right;
						if (char_stats.velocity.x > 0.0f && right_ledge_distance <= Mathf.Abs(char_stats.velocity.x * Time.fixedDeltaTime * Time.timeScale))
						{
							if (character_right < down_hit_collider_right) 
							{
								char_stats.velocity.x = right_ledge_distance / (Time.fixedDeltaTime * Time.timeScale);
							}
							else
							{
								char_stats.velocity.x = 0.0f;
							}
						}
						float left_ledge_distance = character_left - down_hit_collider_left;
						if (char_stats.velocity.x < 0.0f && left_ledge_distance <= Mathf.Abs(char_stats.velocity.x * Time.fixedDeltaTime * Time.timeScale))
						{
							if (character_left > down_hit_collider_left)
							{
								char_stats.velocity.x = -left_ledge_distance / (Time.fixedDeltaTime * Time.timeScale);
							}
							else
							{
								char_stats.velocity.x = 0.0f;
							}
						}
						// set if character is against the ledge
						if ((right_ledge_distance < 1.0f && char_stats.facing_direction == CharEnums.FacingDirection.Right) || (left_ledge_distance < 1.0f && char_stats.facing_direction == CharEnums.FacingDirection.Left))
						{
							is_against_ledge = true;
						}
					}
				}
			}

			if (IsAlmostZero(hit_distance))
			{
				if (touch_ground)
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
				if ( down_hit_collision_type != null )
				{
					if (down_hit_collision_type.Fallthrough == false)
					{
						fallthrough = false;
					}
					else
					{
						// make sure that the player character is not straddling a solid platform
						// issue can't fall down when straddling two fallthrough platforms 
						//(but there shouldn't be a need to have two passthrough platforms touch, they can just merge into 1)
						if ((down_hit_collision_type.WalkOffRight && character_right > down_hit_collider_right) ||
							(down_hit_collision_type.WalkOffLeft  && character_left  < down_hit_collider_left ))
						{
							fallthrough = false;
						}
					}
				}
				else
				{
					//fallthrough = false; //TODO: ? is this right?, or should we do nothing?
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
	/// </summary>
    private void CalculateDirection()
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

		// set Sprite flip
		if (char_stats.previous_facing_direction != char_stats.facing_direction)
		{
			SetFacing();
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

        if (char_stats.current_move_state == CharEnums.MoveState.IsRunning)
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
            if (start_run == true && input_manager.HorizontalAxis > 0.0f && Mathf.Abs(char_stats.velocity.x) < char_stats.SNEAK_SPEED)
            {
                char_stats.velocity.x = char_stats.SNEAK_SPEED;
                start_run = false;
            }
            else if (start_run == true && input_manager.HorizontalAxis < 0.0f && Mathf.Abs(char_stats.velocity.x) < char_stats.SNEAK_SPEED)
            {
                char_stats.velocity.x = -char_stats.SNEAK_SPEED;
                start_run = false;
            }
        }
        
        if (input_manager.HorizontalAxis > 0.0f)
		{
            char_stats.acceleration.x = ACCELERATION;
		}
        else if (input_manager.HorizontalAxis < 0.0f)
		{
            char_stats.acceleration.x = -ACCELERATION;
		}
        else
		{
            char_stats.acceleration.x = 0.0f;
		}

        // Jump logic. Keep the Y velocity constant while holding jump for the duration of JUMP_CONTROL_TIME
		if ((jump_grace_period || char_stats.IsGrounded) && input_manager.JumpInputInst)
        {
            if (input_manager.VerticalAxis < 0.0f)
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

	private void UpdateGracePeriod()
	{
		if ( jump_grace_period )
		{
			jump_grace_period_timer = jump_grace_period_timer + Time.deltaTime * Time.timeScale;
			if ( jump_grace_period_timer >= JUMP_GRACE_PERIOD_TIME )
			{
				jump_grace_period = false;
			}
		}
	}

    public float GetJumpHorizontalSpeedMin()
    {
        return JUMP_HORIZONTAL_SPEED_MIN;
    }

    public float GetJumpHorizontalSpeedMax()
    {
        return JUMP_HORIZONTAL_SPEED_MAX;
    }

	/// <summary>
	/// Determines whether  the specified number is almost zero.
	/// </summary>
	/// <returns><c>true</c> if the number is almost zero; otherwise, <c>false</c>.</returns>
	/// <param name="number">The number to check</param>
	private bool IsAlmostZero(float number)
	{
		return ApproximatelyEquals( number, 0.0f, APPROXIMATE_EQUALITY_MARGIN );
	}

	/// <summary>
	/// Determines if two numbers are approximately equal (within a given margin of error)
	/// </summary>
	/// <returns><c>true</c>, if the numbers are almost equal, <c>false</c> otherwise.</returns>
	/// <param name="a">the first number</param>
	/// <param name="b">the second number</param>
	/// <param name="margin_of_error">margin of error</param>
	private bool ApproximatelyEquals(float a, float b, float margin_of_error )
	{
		if ( a >= b - margin_of_error && a <= b + margin_of_error )
		{
			return true;
		}
		else
		{
		    return false;
		}
	}

	/// <summary>
	/// Utility function for objects that move the player.
	/// Will move them, respecting collision.
	/// </summary>
	/// <param name="change">The change to immediately apply to the player's position.</param>
	public void MoveWithCollision( Vector3 change )
	{
		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		if ( collider == null ) 
		{
			Debug.LogError( "Player doesn't have a collider." );
			return;
		}

		Vector2 size = new Vector2( collider.size.x, collider.size.y );
		RaycastHit2D hit = Physics2D.BoxCast( this.gameObject.transform.position, size, 0.0f, change, change.magnitude, CollisionMasks.all_collision_mask );
		if ( hit != null )
		{
			if ( hit.collider != null )
			{
				if ( change.magnitude == 0.0f ) 
				{
					Debug.Log( "Don't be a troll." );
					return;
				}

				this.gameObject.transform.position += (hit.distance / change.magnitude - 0.05f) * change; //TODO: magic number
			}
			else
			{
				this.gameObject.transform.position += change;
			}
		}
		else
		{
		    this.gameObject.transform.position += change;
		}
	}
}
