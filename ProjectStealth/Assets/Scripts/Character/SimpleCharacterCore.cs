using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Handles movement: sneaking, walking, running, jumping, and falling. Also moving platforms.
// Does not handle climbing, taking cover, evade, aerial evade.
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
    private const float HANG_TIME_FACTOR            = -400.0f;  // value to subtract from gravity during apex of a jump

    //jump vars
    protected const float JUMP_HORIZONTAL_SPEED_MIN =   150.0f; // (pixels / seond)
    protected const float JUMP_HORIZONTAL_SPEED_MAX =   240.0f; // (pixels / second)
    private   const float JUMP_VERTICAL_SPEED       =   300.0f; // (pixels / second)
    private   const float JUMP_CONTROL_TIME         =     0.22f; // maximum duration of a jump (in seconds) if you hold it
    private   const float JUMP_DURATION_MIN         =     0.05f; // minimum duration of a jump (in seconds) if you tap it
    private   const float JUMP_GRACE_PERIOD_TIME    =     0.1f; // how long (in seconds) a player has to jump if they slip off a platform
    [SerializeField]
    private bool jump_grace_period; //for jump tolerance if a player walks off a platform but wants to jump
    [SerializeField]
    private float jump_grace_period_timer;

    //walk and run vars
    private const float MAX_HORIZONTAL_SPEED = 600.0f; // pixels per second
    protected float WALK_SPEED   =  60.0f; // used for cutscenes for PC, guards will walk when not alerted (pixels / second)
    protected float SNEAK_SPEED  = 120.0f; // default speed, enemies that were walking will use this speed when on guard (pixels / second)
    protected float RUN_SPEED    = 240.0f; // (pixels / second)
    protected float ACCELERATION = 360.0f; // acceleration used for velocity calcs when running (pixels / second / second)
    protected float DRAG         = 900.0f; // how quickly a character decelerates when running  (pixels / second / second)
    private bool start_run;                // prevents wonky shit from happening if you turn around during a run

    // ledge logic
    protected bool abuts_facing_sticky_ledge; // if the player is at and facing the edge of a platform (which can't be walked off and if they're not running)
    protected bool is_against_ledge;
    protected bool fallthrough;

    private const float APPROXIMATE_EQUALITY_MARGIN = 0.001f; //Mathf.Epsilon;
    private const float AERIAL_SIZE_INCREASE = 10.0f; // pixels added to collision box height while in midair, to avoid corner clipping
    private const float ONE_PIXEL_BUFFER = 1.0f;

    private Queue<Vector3> applied_moves;
    #endregion

    #region virtual overrides
    // Use this for initialization
    public virtual void Start()
    {
        char_stats      = GetComponent<CharacterStats>();
        input_manager   = GetComponent<IInputManager>();
        char_anims      = GetComponent<CharacterAnimationLogic>();
        sprite_renderer = transform.Find( "Sprites" ).GetComponent<SpriteRenderer>();

        applied_moves = new Queue<Vector3>();

        jump_grace_period = false;
        jump_grace_period_timer = JUMP_GRACE_PERIOD_TIME;
        fallthrough = false;

        SetFacing();
    }

    // Called each frame
    public virtual void Update()
    {
        ApplyMovesWithCollision();

        if ( char_stats.current_master_state != CharEnums.MasterState.DefaultState ) { return; }
        MovementInput();
        CalculateDirection();
        SetVelocity();
        Collisions();
        UpdateGracePeriod();

        ApplyVelocity();
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

    }
    #endregion

    /// <summary>
    /// Parses controller input for walking / running / jumping.
    /// </summary>
    protected void MovementInput()
    {
        RunWalkInput();
        JumpInput();
    }

    /// <summary>
    /// Parses run and walk input, and sets state
    /// </summary>
    private void RunWalkInput()
    {
        if ( input_manager.RunInput )
        {
            char_stats.current_move_state = CharEnums.MoveState.IsRunning;
            StartRunning();
        }
        else
        {
            char_stats.current_move_state = CharEnums.MoveState.IsSneaking;
        }
        SetHorizontalAcceleration();
        abuts_facing_sticky_ledge = IsCrouchedAbuttingFacingStickyLedge();
    }

    /// <summary>
    /// Snaps to minimum running speed when the player presses run.
    /// </summary>
    private void StartRunning()
    {
        // if the character comes to a full stop, let them start the run again
        // This also works when turning around
        if ( char_stats.velocity.x == 0.0f )
        {
            start_run = true;
            PlayerStats playerStats = GetComponent<PlayerStats>();
            playerStats.StartWalking();
        }
        if ( start_run == false ) { return; }

        // running automatically starts at the sneaking speed and accelerates from there
        float sign = 1.0f;
        if ( input_manager.HorizontalAxis < 0.0f ) { sign = -1.0f; }

        if ( Mathf.Abs( char_stats.velocity.x ) < SNEAK_SPEED )
        {
            char_stats.velocity.x = SNEAK_SPEED * sign;
            start_run = false;
        }
    }

    /// <summary>
    /// Parses x axis input and sets x axis acceleration
    /// </summary>
    private void SetHorizontalAcceleration()
    {
        // x accel
        float multiplier = 0.0f;
        if      ( input_manager.HorizontalAxis > 0.0f ) { multiplier =  1.0f; }
        else if ( input_manager.HorizontalAxis < 0.0f ) { multiplier = -1.0f; }
        char_stats.acceleration.x = ACCELERATION * multiplier;
    }

    /// <summary>
    /// Parses jump input and sets jump state
    /// </summary>
    private void JumpInput()
    {
        // Jump logic. Keep the Y velocity constant while holding jump for the duration of JUMP_CONTROL_TIME
        if ( ( jump_grace_period || char_stats.IsGrounded ) && input_manager.JumpInputInst )
        {
            if ( input_manager.VerticalAxis < 0.0f )
            {
                //trigger fallthrough
                fallthrough = true;
            }
            else
            {
                char_stats.is_on_ground = false;
                char_stats.is_jumping = true;
                EndJumpGracePeriod();
                char_stats.jump_input_time = 0.0f;
                char_anims.JumpTrigger();
            }
        }
    }

    /// <summary>
    /// Figures out which way the character should be facing, based on their movement, then sets their facing.
    /// </summary>
    private void CalculateDirection()
    {
        // character direction logic
        bool turnAround = false;
        if ( char_stats.IsFacingLeft() && input_manager.HorizontalAxis > 0.0f )
        {
            char_stats.facing_direction = CharEnums.FacingDirection.Right;
            turnAround = true;
        }
        else if ( char_stats.IsFacingRight() && input_manager.HorizontalAxis < 0.0f )
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
        if ( char_stats.previous_facing_direction != char_stats.facing_direction )
        {
            SetFacing();
        }
    }

    /// <summary>
    /// Makes the player graphically face the correct way.
    /// </summary>
    public void SetFacing()
    {
        if ( char_stats.IsFacingLeft() )
        {
            sprite_renderer.flipX = true;
        }
        else
        {
            sprite_renderer.flipX = false;
        }
    }

    /// <summary>
    /// Sets the player's velocity
    /// </summary>
    private void SetVelocity()
    {
        SetHorizontalVelocity();
        SetVerticalVelocity();
    }

    /// <summary>
    /// Sets x axis velocity
    /// </summary>
    private void SetHorizontalVelocity()
    {
        if ( char_stats.IsGrounded )
        {
            if ( char_stats.is_crouching )
            {
                char_stats.velocity.x = 0.0f;
            }
            else
            {
                //movement stuff
                if ( char_stats.IsWalking )
                {
                    char_stats.velocity.x = WALK_SPEED * input_manager.HorizontalAxis;
                }
                else if ( char_stats.IsSneaking )
                {
                    // if current speed is greater than sneak speed, then decel to sneak speed.
                    if ( Mathf.Abs( char_stats.velocity.x ) > SNEAK_SPEED )
                    {
                        IncreaseMagnitude( ref char_stats.velocity.x, -DRAG * Time.deltaTime * Time.timeScale );
                    }
                    else
                    {
                        // TODO: FIX BUG?: smooth?
                        // If a character is moving with 50% axis input, they'll smooth to the sneak speed, then snap to half of it. That seems silly.
                        char_stats.velocity.x = SNEAK_SPEED * input_manager.HorizontalAxis;
                    }
                }
                else if ( char_stats.IsRunning )
                {
                    //smooth damp to 0 if there's no directional input for running or if you're trying to run the opposite direction
                    if ( char_stats.acceleration.x == 0.0f ||
                        ( char_stats.acceleration.x < 0.0f && char_stats.velocity.x > 0.0f ) ||
                        ( char_stats.acceleration.x > 0.0f && char_stats.velocity.x < 0.0f ) )
                    {
                        if ( Mathf.Abs( char_stats.velocity.x ) > SNEAK_SPEED )
                        {
                            //print("SKID BOIS");
                            IncreaseMagnitude( ref char_stats.velocity.x, -DRAG * Time.deltaTime * Time.timeScale );
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

                    char_stats.velocity.x = Mathf.Clamp( char_stats.velocity.x, -RUN_SPEED, RUN_SPEED );
                }
            }
        }
        else // character is in midair
        {
            if ( char_stats.jump_turned )
            {
                SetHorizontalJumpVelocity( SNEAK_SPEED );
            }
            else
            {
                SetHorizontalJumpVelocity();
            }
        }

        char_stats.velocity.x = Mathf.Clamp( char_stats.velocity.x, -MAX_HORIZONTAL_SPEED, MAX_HORIZONTAL_SPEED );

        if ( IsAlmostZero( char_stats.velocity.x ) )
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

    /// <summary>
    /// Snaps the player's horizontal velocity to the given value, in the direction the player is accelerating.
    /// </summary>
    /// <param name="speed">The speed, in pixels per second</param>
    public void SetHorizontalJumpVelocity( float speed )
    {
        float sign = 0.0f;
        if      ( char_stats.acceleration.x < 0.0f ) { sign = -1.0f; }
        else if ( char_stats.acceleration.x > 0.0f ) { sign =  1.0f; }
        char_stats.velocity.x = Mathf.Clamp( speed * sign, -JUMP_HORIZONTAL_SPEED_MAX, JUMP_HORIZONTAL_SPEED_MAX );
    }

    /// <summary>
    /// Sets the player's horizontal velocity according to default jump behaviour.
    /// </summary>
    protected void SetHorizontalJumpVelocity()
    {
        if ( char_stats.acceleration.x < 0.0f && char_stats.velocity.x >= 0.0f )
        {
            char_stats.velocity.x = -SNEAK_SPEED;
        }
        else if ( char_stats.acceleration.x > 0.0f && char_stats.velocity.x <= 0.0f )
        {
            char_stats.velocity.x = SNEAK_SPEED;
        }
        else
        {
            char_stats.velocity.x += char_stats.acceleration.x * Time.deltaTime * Time.timeScale;
        }
        char_stats.velocity.x = Mathf.Clamp( char_stats.velocity.x, -JUMP_HORIZONTAL_SPEED_MAX, JUMP_HORIZONTAL_SPEED_MAX );
    }

    /// <summary>
    /// Returns the minimum x axis jumping speed.
    /// </summary>
    public float GetJumpHorizontalSpeedMin()
    {
        return JUMP_HORIZONTAL_SPEED_MIN;
    }

    /// <summary>
    /// Returns the maximum x axis jumping speed.
    /// </summary>
    public float GetJumpHorizontalSpeedMax()
    {
        return JUMP_HORIZONTAL_SPEED_MAX;
    }

    /// <summary>
    /// Sets y axis velocity
    /// </summary>
    private void SetVerticalVelocity()
    {
        // override the vertical velocity if we're in the middle of jumping
        if ( char_stats.is_jumping )
        {
            char_stats.jump_input_time = char_stats.jump_input_time + Time.deltaTime * Time.timeScale;
            if ( ( input_manager.JumpInput && char_stats.jump_input_time <= JUMP_CONTROL_TIME ) || char_stats.jump_input_time <= JUMP_DURATION_MIN )
            {
                char_stats.velocity.y = JUMP_VERTICAL_SPEED;
            }
            else
            {
                char_stats.velocity.y += ( GRAVITATIONAL_ACCELERATION - HANG_TIME_FACTOR ) * Time.deltaTime * Time.timeScale;
            }

            if( char_stats.velocity.y < 0 )
            {
                // jump has peaked
                char_stats.is_jumping = false;
                char_anims.FallTrigger();
            }
        }
        // if not jumping, be affected by gravity
        else
        {
            char_stats.velocity.y += GRAVITATIONAL_ACCELERATION * Time.deltaTime * Time.timeScale;
        }

        // if you turned while jumping, turn off the jump var
        if ( char_stats.is_jumping && char_stats.jump_turned && char_stats.velocity.y > 0.0f )
        {
            char_stats.is_jumping = false;
            char_anims.FallTrigger(); // only want to call this once per jump.
        }

        char_stats.velocity.y = Mathf.Clamp( char_stats.velocity.y, -MAX_VERTICAL_SPEED, MAX_VERTICAL_SPEED );

        if ( char_stats.IsGrounded && IsAlmostZero( char_stats.velocity.y ) )
        {
            char_stats.velocity.y = 0.0f;
        }
    }

    /// <summary>
    /// Handles collision checking
    /// </summary>
    public virtual void Collisions()
    {
        CheckCollisionHorizontal();
        CheckCollisionVertical();
    }

    /// <summary>
    /// Checks if x axis movement causes collision with anything, and stops movement if it does
    /// </summary>
    private void CheckCollisionHorizontal()
    {
        if ( char_stats.velocity.x == 0.0f ) { return; }

        Vector2 collision_box_size, collision_box_center, direction;
        SetupHorizontalCollision( out collision_box_center, out collision_box_size, out direction );
        char_stats.touched_vault_obstacle = null;

        RaycastHit2D hit = Physics2D.BoxCast( collision_box_center, collision_box_size, 0.0f, direction, 50.0f, CollisionMasks.upwards_collision_mask );
        if ( hit.collider == null ) { return; }
        
        float hit_distance = hit.distance - ONE_PIXEL_BUFFER;
        if ( hit_distance <= Mathf.Abs( char_stats.velocity.x * Time.deltaTime * Time.timeScale ) )
        {
            CollisionType hit_collision_type = hit.collider.GetComponent<CollisionType>();
            if ( hit_collision_type != null )
            {
                if ( hit_collision_type.CanVaultOver == true && char_stats.IsGrounded )
                {
                    char_stats.touched_vault_obstacle = hit.collider;
                }
                if ( ! hit_collision_type.IsBlocking ) { return; }
            }
            // we touched a wall
            Vector3 gap;
            if ( char_stats.velocity.x > 0.0f ) { gap = new Vector3(  hit_distance, 0.0f, 0.0f ); }
            else                                { gap = new Vector3( -hit_distance, 0.0f, 0.0f ); }
            this.gameObject.transform.Translate( gap );
            char_stats.velocity.x = 0.0f;
            OnTouchWall( hit.collider.gameObject );
        }
    }

    /// <summary>
    /// Sets up the collision box used to collide against objects horizontally.
    /// </summary>
    /// <param name="center">The centerpoint of the hitbox</param>
    /// <param name="size">The dimensions of the hitbox</param>
    /// <param name="direction">A unit vector indicating the direction of motion (left or right?)</param>
    private void SetupHorizontalCollision( out Vector2 center, out Vector2 size, out Vector2 direction )
    {
        // Set collision box size
        if ( char_stats.IsGrounded )
        {
            size = new Vector2( 1.0f, char_stats.char_collider.bounds.size.y ); // 1px x height box
        }
        else
        {
            // Extend the hitbox vertically while in the air to avoid corner clipping
            size = new Vector2( 1.0f, char_stats.char_collider.bounds.size.y + AERIAL_SIZE_INCREASE ); // this expands 5 pixels in each direction from the middle.
            // NOTE: This means levels should be designed such that the minimum height of a space is the player character's height + AERIAL_SIZE_INCREASE
        }

        // Set center + direction
        if ( char_stats.velocity.x > 0.0f )
        {
            center = new Vector2( char_stats.char_collider.bounds.max.x - size.x, char_stats.char_collider.bounds.center.y );
            direction = Vector2.right;
        }
        else
        {
            center = new Vector2( char_stats.char_collider.bounds.min.x + size.x, char_stats.char_collider.bounds.center.y );
            direction = Vector2.left;
        }

        // Adjust the hitbox
        // If moving upwards, we want the additional hitbox area to be above the player. If moving downwards, we want it below.
        if ( char_stats.IsInMidair )
        {
            if ( char_stats.velocity.y > 0.0f )
            {
                center += new Vector2( 0.0f, AERIAL_SIZE_INCREASE / 2.0f );
            }
            else
            {
                center -= new Vector2( 0.0f, AERIAL_SIZE_INCREASE / 2.0f );
            }
        }
    }

    /// <summary>
    /// Checks if y axis movement causes collision with anything, and stops movement if it does.
    /// </summary>
    // TODO: refactor & split (maybe top/bottom). DRY.
    private void CheckCollisionVertical()
    {
        CheckCollisionUp();
        CheckCollisionDown();
    }

    /// <summary>
    /// Checks if the player will bump their head on the ceiling
    /// </summary>
    private void CheckCollisionUp()
    {
        if ( char_stats.velocity.y <= 0.0f ) { return; } // immobile or moving down, don't need to check

        Vector2 collision_box_size = new Vector2( char_stats.char_collider.bounds.size.x, 1.0f ); // width x 1 px box
        Vector2 origin             = new Vector2( char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.max.y - collision_box_size.y );
        RaycastHit2D hit           = Physics2D.BoxCast( origin, collision_box_size, 0.0f, Vector2.up, 50.0f, CollisionMasks.upwards_collision_mask );
        if ( hit.collider == null ) { return; }

        float hit_distance = hit.distance - ONE_PIXEL_BUFFER;
        if ( hit_distance <= Mathf.Abs( char_stats.velocity.y * Time.deltaTime * Time.timeScale ) )
        {
            CollisionType collision_type = hit.transform.gameObject.GetComponent<CollisionType>();
            if ( collision_type != null )
            {
                if ( ! collision_type.IsBlocking ) { return; }
            }
            // hit the ceiling, stop upward movement
            this.gameObject.transform.Translate( new Vector3( 0.0f, hit_distance, 0.0f ) );
            char_stats.velocity.y = 0.0f;
            char_stats.is_jumping = false;
            OnTouchCeiling( hit.collider.gameObject );
        }
    }

    /// <summary>
    /// Checks if the player will hit the floor with their feet
    /// </summary>
    private void CheckCollisionDown()
    {
        if ( char_stats.velocity.y >= 0.0f ) { return; } // immobile or moving up, don't need to check

        Vector2 collision_box_size = new Vector2( char_stats.char_collider.bounds.size.x, 1.0f ); // width x 1 px box
        Vector2 origin             = new Vector2( char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.min.y + collision_box_size.y );
        RaycastHit2D hit           = Physics2D.BoxCast( origin, collision_box_size, 0.0f, Vector2.down, 50.0f, CollisionMasks.all_collision_mask );
        if ( hit.collider == null ) // if there is no floor, just fall
        {
             FallingLogic();
             return;
        }

        CollisionType collision_type = hit.transform.gameObject.GetComponent<CollisionType>();
        float hit_distance = hit.distance - ONE_PIXEL_BUFFER;
        if ( hit_distance <= Mathf.Abs( char_stats.velocity.y * Time.deltaTime * Time.timeScale ) )
        {
            // hit the floor, stop falling ... probably
            bool did_touch_ground = true;
            if ( collision_type != null )
            {
                if ( ! collision_type.IsBlocking ) { return; }
                // special types of floor can change behaviour.
                if ( ShuntPlayer( collision_type, hit.collider ) )         { did_touch_ground = false; }
                if ( FallthroughPlatform( collision_type, hit.collider ) ) { did_touch_ground = false; }
            }
            else
            {
                Debug.LogError( "Improper configuration: platform is missing a CollisionType component." );
            }

            if ( did_touch_ground )
            {
                // close the gap between the player's feet and the floor
                transform.Translate( new Vector3( 0.0f, -1.0f * hit_distance, 0.0f ) );
                char_stats.velocity.y = 0.0f;
                OnTouchGround( hit.collider );
            }
        }
        else // didn't collide with anything
        {
            char_stats.on_ground_collider = null;
            FallingLogic();
        }
        CheckPlatformEdge( collision_type, hit.collider ); // TODO: move to horizontal collision, get floor collision data to it efficiently somehow.
    }

    /// <summary>
    /// Shunts the player horizontally off the platform if they land on the edge of a non-ledge platform while falling, and makes them continue to fall.
    /// </summary>
    /// <param name="floor_collision_type">The collision type of the floor.</param>
    /// <param name="floor_collider">The collider of the floor.</param>
    /// <returns>True if the player was shunted off a platform, false otherwise.</returns>
    private bool ShuntPlayer( CollisionType floor_collision_type, Collider2D floor_collider )
    {
        float floor_collider_left  = floor_collider.bounds.min.x;
        float floor_collider_right = floor_collider.bounds.max.x;
        float character_left       = char_stats.char_collider.bounds.min.x;
        float character_right      = char_stats.char_collider.bounds.max.x;

        if ( char_stats.velocity.x > 0.0f && floor_collider_right < char_stats.char_collider.bounds.center.x && floor_collision_type.CanWalkOffRightEdge == false )
        {
            transform.Translate( floor_collider_right - character_left, 0.0f, 0.0f );
            return true;
        }
        if ( char_stats.velocity.x < 0.0f && floor_collider_left > char_stats.char_collider.bounds.center.x && floor_collision_type.CanWalkOffLeftEdge == false )
        {
            transform.Translate( -1.0f * ( character_right - floor_collider_left ), 0.0f, 0.0f );
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the player can fall through the floor, and sets state.
    /// </summary>
    /// <param name="floor_collision_type">The collision type of the floor.</param>
    /// <param name="floor_collider">The collider of the floor.</param>
    /// <returns>True if the player fell through a platform, false otherwise.</returns>
    private bool FallthroughPlatform( CollisionType floor_collision_type, Collider2D floor_collider )
    {
        if ( ! fallthrough ) { return false; }    // The player is not falling through the floor.

        if ( ! floor_collision_type.CanFallthrough ) // The player cannot pass through this floor.
        {
            fallthrough = false;
            return false;
        }

        // make sure that the player character is not straddling a solid platform
        // issue can't fall down when straddling two fallthrough platforms 
        // (but there shouldn't be a need to have two passthrough platforms touch, they can just merge into 1)
        if ( ( floor_collision_type.CanWalkOffRightEdge && char_stats.char_collider.bounds.max.x > floor_collider.bounds.max.x ) ||
             ( floor_collision_type.CanWalkOffLeftEdge  && char_stats.char_collider.bounds.min.x < floor_collider.bounds.min.x ) )
        {
            fallthrough = false;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Stops players at the edge of a ledge, and sets state for edge-sensitive actions.
    /// </summary>
    /// <param name="collision_type">Collision type of the floor platform which we may be at the edge of.</param>
    /// <param name="collider">Collider of the floor platform which we may be at the edge of.</param>
    private void CheckPlatformEdge( CollisionType collision_type, Collider2D collider )
    {
        is_against_ledge = false;
        if ( collision_type == null ) { return; } // no ledge
        if ( char_stats.IsInMidair )  { return; } // player can jump over ledges
        if ( char_stats.current_move_state == CharEnums.MoveState.IsRunning ) { return; } // player can run off ledges

        float distance_to_edge;
        if ( char_stats.IsFacingLeft() )
        {
            if ( collision_type.CanWalkOffLeftEdge )  { return; } // player can walk off walk-off-able ledges (allows characters to walk over connected platforms)
            distance_to_edge = char_stats.char_collider.bounds.min.x - collider.bounds.min.x;
        }
        else
        {
            if ( collision_type.CanWalkOffRightEdge ) { return; } // player can walk off walk-off-able ledges
            distance_to_edge = collider.bounds.max.x - char_stats.char_collider.bounds.max.x;
        }

        // Stop players at the edge of a ledge
        SnapToEdge( distance_to_edge );

        // Is the player abutting and facing a sticky (not slippery) edge?
        is_against_ledge = ( distance_to_edge < ONE_PIXEL_BUFFER );
    }

    /// <summary>
    /// Snaps the player to the edge of a platform. 
    /// For use with platforms players can't walk off, and if players aren't running.
    /// </summary>
    /// <param name="distance_to_edge">The distance to the edge</param>
    private void SnapToEdge( float distance_to_edge )
    {
        float sign = 1.0f;
        if ( char_stats.IsFacingLeft() ) { sign = -1.0f; }
        if ( char_stats.velocity.x * sign <= 0.0f ) { return; } // if player is not moving in the direction they are facing and toward the edge

        // Over the edge, stop. On the platform, snap to ledge edge.
        if ( distance_to_edge <= Mathf.Abs( char_stats.velocity.x * Time.deltaTime * Time.timeScale ) )
        {
            char_stats.velocity.x = 0.0f;
            if ( Mathf.Abs( distance_to_edge ) == distance_to_edge ) { transform.Translate( new Vector3( distance_to_edge * sign, 0.0f, 0.0f ) ); }
        }
    }

    /// <summary>
    /// Called when the character is falling.
    /// Sets up a grace period where the character can still jump.
    /// </summary>
    private void FallingLogic()
    {
        if ( char_stats.IsGrounded && char_stats.is_jumping == false ) { StartJumpGracePeriod(); }
        char_stats.is_on_ground = false;
        is_against_ledge = false;
    }

    /// <summary>
    /// Checks if the character is crouched while touching and facing a sticky (not slippery) edge.
    /// </summary>
    protected virtual bool IsCrouchedAbuttingFacingStickyLedge()
    {
        return ( is_against_ledge && char_stats.is_crouching );
    }

    /// <summary>
    /// Called when a character hits the floor. Resets state.
    /// </summary>
    /// <param name="ground_collider">The collider of the floor.</param>
    private void OnTouchGround( Collider2D ground_collider )
    {
        char_stats.is_on_ground = true;
        char_stats.jump_turned = false;
        char_stats.on_ground_collider = ground_collider;
    }

    /// <summary>
    /// Called when a character bumps into a wall.
    /// </summary>
    public virtual void OnTouchWall( GameObject collisionObject )
    {
        //base class does nothing with this function. gets overridden at the subclass level to handle such occasions
    }

    /// <summary>
    /// Called when a character bumps into the ceiling.
    /// </summary>
    /// <param name="collisionObject"></param>
    public virtual void OnTouchCeiling( GameObject collisionObject )
    {
        //base class does nothing with this function. gets overridden at the subclass level to handle such occasions
        //char_anims.FallTrigger() needs to be incluseded in the override somewhere
    }

    /// <summary>
    /// Starts the jump grace period, where the player is allowed to jump without solid footing.
    /// </summary>
    private void StartJumpGracePeriod()
    {
        jump_grace_period = true;
        jump_grace_period_timer = 0.0f;
    }

    /// <summary>
    /// Ends the jump grace period.
    /// </summary>
    private void EndJumpGracePeriod()
    {
        jump_grace_period = false;
    }

    /// <summary>
    /// Counts down the grace period to jump after falling begins, and removes the state of grace when it runs out.
    /// </summary>
    private void UpdateGracePeriod()
    {
        if ( jump_grace_period )
        {
            jump_grace_period_timer = jump_grace_period_timer + Time.deltaTime * Time.timeScale;
            if ( jump_grace_period_timer >= JUMP_GRACE_PERIOD_TIME )
            {
                EndJumpGracePeriod();
            }
        }
    }

    /// <summary>
    /// Determines whether the specified number is almost zero.
    /// </summary>
    /// <returns><c>true</c> if the number is almost zero; otherwise, <c>false</c>.</returns>
    /// <param name="number">The number to check</param>
    private bool IsAlmostZero( float number )
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
    private bool ApproximatelyEquals( float a, float b, float margin_of_error )
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
    /// The END GOAL: Actually move the character after all calculations have been done.
    /// </summary>
    private void ApplyVelocity()
    {
        if ( char_stats.current_master_state == CharEnums.MasterState.DefaultState )
        {
            transform.Translate( char_stats.velocity * Time.deltaTime * Time.timeScale );
            if ( fallthrough == true )
            {
                transform.Translate( Vector3.down ); // move 1 pixel down.
                fallthrough = false;
                char_anims.FallthoughTrigger();
            }
        }
    }

    /// <summary>
    /// Utility function for objects that move the player.
    /// Will cause them to move, respecting collision.
    /// </summary>
    /// <param name="change">The change to apply this frame to the player's position.</param>
    public void MoveWithCollision( Vector3 change )
    {
        applied_moves.Enqueue( change );
        // There used to be all sorts of bugs because we set velocities in multiple places rather than translating to touch walls and setting velocity to 0.
        // Now that everything just checks collision and translates, it all works out, and this is the prefferred method of applying external motion, not the ONLY ALLOWED one.
    }

    /// <summary>
    /// For all applied external sources of motion, actually moves the player, respecting collision.
    /// </summary>
    private void ApplyMovesWithCollision()
    {
        while ( applied_moves.Count > 0 )
        {
            ApplyMoveWithCollision( applied_moves.Dequeue() );
        }
    }

    /// <summary>
    /// Moves the player, respecting collision. For external sources of motion.
    /// </summary>
    /// <param name="change">The change to apply this frame to the player's position.</param>
    private void ApplyMoveWithCollision( Vector3 change )
    {
        // Validation
        if ( change.sqrMagnitude == 0.0f )
        {
            Debug.Log( "Don't be a troll." );
            return;
        }

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if ( collider == null )
        {
            Debug.LogError( "Player doesn't have a collider." );
            return;
        }

        // Collision check.
        Vector2 size = new Vector2( collider.size.x, collider.size.y );
        RaycastHit2D hit = Physics2D.BoxCast( this.gameObject.transform.position, size, 0.0f, change, change.magnitude, CollisionMasks.all_collision_mask );

        if ( hit.collider != null ) // Collided with something.
        {
            if ( hit.distance < ONE_PIXEL_BUFFER ) { return; } // prevent jitter by not moving if you are point-blank with a collider.

            // Move very close to the collider.
            this.gameObject.transform.position += new Vector3( 
                ( hit.distance - ONE_PIXEL_BUFFER ) * Mathf.Cos( Mathf.Atan2( change.y, change.x ) ), 
                ( hit.distance - ONE_PIXEL_BUFFER ) * Mathf.Sin( Mathf.Atan2( change.y, change.x ) ), 0.0f );
        }
        else // Collided with nothing. Move it.
        {
            this.gameObject.transform.position += change;
        }
    }
}
