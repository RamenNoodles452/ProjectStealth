using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

// Handles wall and ceiling climbing
public class MagGripUpgrade : MonoBehaviour
{
    #region vars
    private SpriteRenderer sprite_renderer;
    private IInputManager input_manager;
    private CharacterAnimationLogic char_anims;

    //this allows us to reference player stuff like their movement state
    Player player_script;
    PlayerStats player_stats;
    CharacterStats char_stats;

    private const float WALL_GRAB_DELAY = 0.15f;
    private float wall_grab_delay_timer = 0.15f;

    //Mag Grip variables
    public enum ClimbState { NotClimb, WallClimb, CeilingClimb, Transition, Hanging };
    public ClimbState current_climb_state = ClimbState.NotClimb;
    public bool is_looking_away = false;

    private const float WALL_CLIMB_SPEED = 70.0f;  // pixels / second
    private const float WALL_SLIDE_SPEED = 160.0f; // pixels / second

    //TODO: do something about duplicate code
    // ledge logic
    private bool is_overlooking_ledge; //TODO: consider renaming more descriptively
    private bool is_against_ledge;     //TODO: consider renaming more descriptively

    private bool is_touching_top;
    private bool is_touching_bottom;
    private bool can_crouch_up_from_hang;
    private bool can_stand_up_from_hang;

    //consts
    protected const float JUMP_ACCELERATION = 240.0f; // base acceleration for jump off the wall with no input (pixels / second / second)
    #endregion

    #region Monobehaviour overrides
    // Use this for early initialization
    void Awake()
    {
        player_script   = GetComponent<Player>();
        sprite_renderer = GetComponentInChildren<SpriteRenderer>();
        player_stats    = GetComponent<PlayerStats>();
        char_stats      = GetComponent<CharacterStats>();
        input_manager   = GetComponent<IInputManager>();
        char_anims      = GetComponent<CharacterAnimationLogic>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimers();
        ParseInput();
        MovePlayer();
    }
    #endregion

    /// <summary>
    /// Updates timers for time-sensitive inputs
    /// </summary>
    private void UpdateTimers()
    {
        //wall grab delay timer
        if ( wall_grab_delay_timer < WALL_GRAB_DELAY )
        {
            wall_grab_delay_timer = wall_grab_delay_timer + Time.deltaTime * Time.timeScale;
        }
    }

    /// <summary>
    /// Parses user input
    /// </summary>
    private void ParseInput()
    {
        if ( char_stats.current_master_state != CharEnums.MasterState.ClimbState ) { return; }

        if      ( current_climb_state == ClimbState.WallClimb )    { ClimbMovementInput(); }
        else if ( current_climb_state == ClimbState.CeilingClimb ) { CeilingClimbMovementInput(); }
        else if ( current_climb_state == ClimbState.Hanging )      { HangMovementInput(); }
    }

    /// <summary>
    /// Parses climb movement input.
    /// </summary>
    void ClimbMovementInput()
    {
        float character_top = char_stats.char_collider.bounds.max.y;
        float character_bottom = char_stats.char_collider.bounds.min.y;

        if ( char_stats.current_master_state != CharEnums.MasterState.ClimbState ) { return; }

        LookAway();
        LedgeLook();

        // Jump logic.
        if ( input_manager.JumpInputInst )
        {
            if ( is_looking_away )
            {
                StopClimbing();
                JumpAway();
            }
            //ledge climb logic
            /*
            else if ( is_overlooking_ledge )
            {
                // if we're looking below, drop
                if ( grab_collider.bounds.min.y == character_bottom )
                {
                    StopClimbing();
                    char_anims.DropFromWallTrigger();
                }
                // if we're looking above, climb up
                else if ( grab_collider.bounds.max.y == character_top )
                {
                    WallToGroundStart();
                }
            }*/
        }
    }

    /// <summary>
    /// Parses movement input when the player is climbing on the ceiling.
    /// </summary>
    private void CeilingClimbMovementInput()
    {
        float character_top = char_stats.char_collider.bounds.max.y;
        float character_bottom = char_stats.char_collider.bounds.min.y;

        LookAway();
        LedgeLook();

        // TODO: jump logic
        if ( input_manager.JumpInputInst )
        {
            StopClimbing();
        }
    }

    /// <summary>
    /// Parses movement input when the player is hanging from a wall.
    /// </summary>
    private void HangMovementInput()
    {
        LookAway();

        // When hanging, player is ALWAYS locked at the edge, don't need to move or do confirmation checks.
        if ( input_manager.VerticalAxis > 0.0f && can_stand_up_from_hang )
        {
            // Pull yourself up and stand on platform.
            float x = char_stats.STANDING_COLLIDER_SIZE.x; // right
            if ( char_stats.IsFacingLeft() ) { x = -char_stats.STANDING_COLLIDER_SIZE.x; }
            transform.Translate( new Vector3( x, char_stats.STANDING_COLLIDER_SIZE.y + 1.0f, 0.0f ) );

            current_climb_state = ClimbState.NotClimb;
            char_stats.current_master_state = CharEnums.MasterState.DefaultState;
        }
        else if ( input_manager.VerticalAxis < 0.0f )
        {
            // Drop.
            current_climb_state = ClimbState.NotClimb;
            char_stats.current_master_state = CharEnums.MasterState.DefaultState;
            char_anims.FallTrigger();
        }
        else if ( input_manager.JumpInputInst )
        {
            // Jump away from the wall.
            StopClimbing();
            JumpAway();
        }
    }

    /// <summary>
    /// Looks away if the player inputs away from the wall
    /// </summary>
    public void LookAway()
    {
        if ( current_climb_state == ClimbState.WallClimb || 
             current_climb_state == ClimbState.Hanging )
        {
            if ( ( char_stats.IsFacingRight() && input_manager.HorizontalAxis < -0.5f ) ||
                 ( char_stats.IsFacingLeft() && input_manager.HorizontalAxis > 0.5f) )
            {
                char_anims.WallLookAway( true );
                is_looking_away = true;
            }
            else
            {
                char_anims.WallLookAway( false );
                is_looking_away = false;
            }
        }
        else if ( current_climb_state == ClimbState.CeilingClimb )
        {
            if ( input_manager.VerticalAxis < 0.0f )
            {
                is_looking_away = true;
            }
            else
            {
                is_looking_away = false;
            }
        }
    }

    // Jumps away from the wall the player is climbing / hanging from.
    private void JumpAway()
    {
        char_stats.is_jumping = true;
        char_stats.jump_input_time = 0.0f;
        char_stats.AboutFace();
        char_anims.JumpTrigger();

        if ( char_stats.IsFacingRight() )
        {
            char_stats.acceleration.x = JUMP_ACCELERATION;
        }
        else
        {
            char_stats.acceleration.x = -JUMP_ACCELERATION;
        }

        player_script.SetHorizontalJumpVelocity( ( player_script.GetJumpHorizontalSpeedMin() + player_script.GetJumpHorizontalSpeedMax() ) / 2.0f );
        player_script.SetFacing();
    }

    /// <summary>
    /// 
    /// </summary>
    void LedgeLook()
    {
        // TODO: rewrite.
        if ( current_climb_state == ClimbState.WallClimb )
        {
            // If you are at the top or botton end of the wall, and move toward the end? (or into the wall)?
            if ( is_against_ledge && ( Mathf.Abs( input_manager.VerticalAxis ) > 0.0f /*||
                ( input_manager.HorizontalAxis > 0.0f && char_stats.char_collider.bounds.center.x < grab_collider.bounds.center.x ) ||
                ( input_manager.HorizontalAxis < 0.0f && char_stats.char_collider.bounds.center.x > grab_collider.bounds.center.x )*/ ) )
            {
                is_overlooking_ledge = true;
            }
            else
            {
                is_overlooking_ledge = false;
            }
        }
        else if ( current_climb_state == ClimbState.CeilingClimb )
        {

        }
    }

    /// <summary>
    /// Moves the player up or down walls, or across ceilings, and handles state transitions.
    /// </summary>
    private void MovePlayer()
    {
        if ( char_stats.current_master_state != CharEnums.MasterState.ClimbState ) { return; }

        if ( current_climb_state == ClimbState.Hanging )
        {
            // already handled.
        }
        else if ( current_climb_state == ClimbState.WallClimb )
        {
            ClimbWall();
        }
        else if ( current_climb_state == ClimbState.CeilingClimb )
        {
            ClimbCeiling();
        }
    }

    /// <summary>
    /// Sets state and cleans up when the player stops climbing.
    /// </summary>
    public void StopClimbing()
    {
        current_climb_state = ClimbState.NotClimb;
        //reset the delay before we can wall grab again
        wall_grab_delay_timer = 0.0f;
        //set the master state to the default state. It'll transition into any other state from there. 
        char_stats.current_master_state = CharEnums.MasterState.DefaultState;

        //TODO: temp?
        //char_anims.WallSlideTouchGround();
        // Reset animation state
        char_anims.ResetWallClimb();
    }

    /// <summary>
    /// Gets the player off of the wall.
    /// </summary>
    void WallToGroundStart()
    {
        input_manager.JumpInputInst = false;
        input_manager.JumpInput = false;
        //input_manager.IgnoreInput = true; // ! CAUTION ! WARNING ! DANGER !
        // I'm forcing a state transition promise here rather than trusting animation logic to unset it because of the risk involved. - GabeV
        player_stats.FreezePlayer( 0.5f );

        // variable sterilization
        char_stats.is_jumping = false;
        char_stats.is_on_ground = true;

        char_anims.WallToGroundTrigger();
        current_climb_state = ClimbState.Transition;

        char_stats.is_crouching = false;
        char_stats.CrouchingHitBox();
        char_anims.SetCrouch();
    }

    /// <summary>
    /// 
    /// </summary>
    public void WallToGroundStop()
    {
        current_climb_state = ClimbState.NotClimb;
        input_manager.IgnoreInput = false;
        is_overlooking_ledge = false;
        is_against_ledge = false;


        if ( input_manager.VerticalAxis >= 0.0f )
        {
            char_stats.StandingHitBox();
        }

        char_stats.current_master_state = CharEnums.MasterState.DefaultState;
        char_stats.current_move_state = CharEnums.MoveState.IsSneaking;
        // TODO: never use getcomponent., need null check
        //transform.position = new Vector3( transform.position.x, grab_collider.bounds.max.y + GetComponent<Collider2D>().bounds.size.y / 2.0f + GetComponent<Collider2D>().offset.y + 1.0f + 10.0f, transform.position.z );
    }

    /// <summary>
    /// 
    /// </summary>
    void GroundToWallStart()
    {
        char_anims.ResetJumpDescend();
        if ( char_stats.IsFacingLeft() )
        {
            char_stats.facing_direction = CharEnums.FacingDirection.Right;
            sprite_renderer.flipX = false;
        }
        else
        {
            char_stats.facing_direction = CharEnums.FacingDirection.Left;
            sprite_renderer.flipX = true;
        }
        input_manager.JumpInputInst = false;
        input_manager.JumpInput = false;
        input_manager.IgnoreInput = true;
        // variable sterilization
        char_stats.is_jumping = false;
        char_anims.GroundToWallTrigger();
        current_climb_state = ClimbState.Transition;

        char_stats.is_on_ground = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void GroundToWallStop()
    {
        current_climb_state = ClimbState.WallClimb;
        input_manager.IgnoreInput = false;
        is_overlooking_ledge = false;
        is_against_ledge = false;

        char_stats.current_master_state = CharEnums.MasterState.ClimbState;
        char_stats.current_move_state = CharEnums.MoveState.IsSneaking;
        char_stats.velocity.x = 0.0f;

        char_stats.ResetJump();
        char_stats.StandingHitBox();
        char_stats.is_crouching = false;
    }

    /// <summary>
    /// Function that initiates a wallclimb from crouching on the ground against a ledge
    /// </summary>
    public void WallClimbFromLedge()
    {
        Vector2 box_size = new Vector2(char_stats.char_collider.bounds.size.x - 0.1f, 0.1f);
        Vector2 origin = new Vector2(char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.center.y - char_stats.char_collider.bounds.extents.y + 0.1f);
        RaycastHit2D hit = Physics2D.BoxCast(origin, box_size, 0.0f, Vector2.down, 25.0f, CollisionMasks.standard_collision_mask);
        if ( hit.collider == null ) { return; }
        
        //check the ledge to see if it's tall enough to grab onto
        RaycastHit2D grabCheck;
        Vector2 point;
        Vector2 direction;
        if ( char_stats.IsFacingRight() )
        {
            point = new Vector2(char_stats.char_collider.bounds.center.x + char_stats.char_collider.bounds.size.x, char_stats.char_collider.bounds.min.y - char_stats.char_collider.bounds.size.y);
            direction = Vector2.left;
        }
        else
        {
            point = new Vector2(char_stats.char_collider.bounds.center.x - char_stats.char_collider.bounds.size.x, char_stats.char_collider.bounds.min.y - char_stats.char_collider.bounds.size.y);
            direction = Vector2.right;
        }
        grabCheck = Physics2D.Raycast( point, direction, char_stats.char_collider.bounds.size.x );

        if ( grabCheck.collider == null )       { AbortWallClimbFromLedge(); return; }  // didn't hit anything, too short.
        if ( hit.collider == null )             { AbortWallClimbFromLedge(); return; }  // no floor.

        CustomTileData tile_data = Utils.GetCustomTileData( hit.collider );
        CollisionType collision_type = null;
        if ( tile_data != null ) { collision_type = tile_data.collision_type; }
        Collider2D collider = hit.collider;
        if ( tile_data != null ) { collider = tile_data.gameObject.GetComponent<Collider2D>(); }

        if ( collision_type == null )           { AbortWallClimbFromLedge(); return; }  // invalid configuration
        if ( ! collision_type.IsWallClimbable ) { AbortWallClimbFromLedge(); return; }  // unclimbable object
        if ( grabCheck.collider != collider )   { AbortWallClimbFromLedge(); return; }  // hit a different object, too short

        char_stats.current_master_state = CharEnums.MasterState.ClimbState;
        GroundToWallStart();
    }

    /// <summary>
    /// When climbing a wall from a ledge fails, this is called.
    /// </summary>
    private void AbortWallClimbFromLedge()
    {
        //TODO: head shake animation to notify that the ledge is too short
    }

    /// <summary>
    /// If the player jumps into the right kind of geometry just right, grab the edge of it, and hang off it.
    /// </summary>
    public void InitiateLedgeGrab()
    {
        // TODO: make can stand up member var
        can_stand_up_from_hang = false;
        can_crouch_up_from_hang = false;

        #region Validation
        // Validation
        if ( ! char_stats.IsInMidair )                    { return; } // must be in midair
        if ( current_climb_state != ClimbState.NotClimb ) { return; } // don't start climbing if you already are
        #endregion

        #region Check if player can grab ledge
        // Platform's top MUST be equal to (or higher than) val's top when she grabs hold. So, we cast from her top in the direction she is facing.
        //   (prevents: grabbing thin air above a chest-high wall)

        float character_top = char_stats.char_collider.bounds.max.y;
        float character_left = char_stats.char_collider.bounds.min.x;
        float character_right = char_stats.char_collider.bounds.max.x;
        // Don't care about bottom for this.
        // There is no minimum combined height requirement for the climbable surface.

        Vector2 direction = Vector2.right;
        float x = character_right;
        if ( char_stats.IsFacingLeft() )
        {
            x = character_left;
            direction = Vector2.left;
        }
        Vector2 origin = new Vector2( x, character_top );
        float distance = Mathf.Abs( char_stats.velocity.x ) * Time.deltaTime * Time.timeScale + 1.0f;

        RaycastHit2D hit = Physics2D.Raycast( origin, direction, distance, CollisionMasks.ledge_grab_mask );
        if ( hit.collider == null ) { return; } // no ledge.

        // Player's top must be close to the platform's top to grab on.
        // TODO: potentially unwind position for one frame, so this works with exceptionally low framerates and high speeds.
        const float SNAP_MARGIN = 4.0f; // pixels
        float top_to_top_distance = hit.collider.bounds.max.y - character_top;
        if ( top_to_top_distance > SNAP_MARGIN || top_to_top_distance < 0.0f ) { return; }
        #endregion

        #region Test if it is a ledge
        // There must be empty space above the top of the platform.
        //   ( prevents: ledge grab working on flush walls. )
        //   ( player character's width x 1 tile high minimum, 2 tiles high to allow pulling up into standing)
        // There must be empty space above the dangling player.
        //   (prevents: phasing through a block)
        //   ex: [ ][ ]     [X][ ]    [ ][X]
        //       [P][x] NOT [P][X] OR [P][X]

        // Confirm it is a ledge.
        Vector2 player_size = char_stats.char_collider.size;
        float platform_top = hit.collider.bounds.max.y;
        float x1, x2; 
        if ( char_stats.IsFacingRight() )
        {
            x1 = character_left;
            x2 = hit.collider.bounds.max.x - 1.0f;
        }
        else //if ( char_stats.IsFacingLeft() )
        {
            x1 = character_right;
            x2 = hit.collider.bounds.min.x + 1.0f;
        }
        const float MINIMUM_CLEARANCE = 32.0f - 1.0f; // pixels

        Collider2D ledge_test = Physics2D.OverlapArea( new Vector2( x1, platform_top + 1.0f ), new Vector2(x2, platform_top + MINIMUM_CLEARANCE ), CollisionMasks.static_mask );
        if ( ledge_test != null ) { return; } // Not a ledge.
        if ( char_stats.CROUCHING_COLLIDER_SIZE.y <= MINIMUM_CLEARANCE ) { can_crouch_up_from_hang = true; }
        #endregion

        #region Check if the player can pull themselves up into standing position
        // Check if you can pull yourself up from the ledge and into standing position.
        // ex: [ ][ ]
        //     [ ][ ]
        //     [P][X]

        if ( char_stats.IsFacingRight() )
        {
            x1 = character_left;
            // if the platform is too narrow, need to check 1 player width beyond the player's rightmost bound.
            x2 = Mathf.Max( hit.collider.bounds.max.x, x1 + player_size.x * 2.0f );
        }
        else //if ( char_stats.IsFacingLeft() )
        {
            x1 = character_right;
            // If the platform is too narrow, need to check 1 player width beyond the player's leftmost bound.
            x2 = Mathf.Min( hit.collider.bounds.min.x, x1 - player_size.x * 2.0f );
        }
        const float MINIMUM_STANDING_CLEARANCE = 64.0f - 1.0f; // pixels
        Collider2D stand_test = Physics2D.OverlapArea( new Vector2( x1, platform_top ), new Vector2( x2, platform_top + MINIMUM_STANDING_CLEARANCE ), CollisionMasks.static_mask );
        if ( stand_test != null && MINIMUM_STANDING_CLEARANCE >= char_stats.STANDING_COLLIDER_SIZE.y ) { can_stand_up_from_hang = true; } // Can stand up.
        #endregion

        // Grab the ledge
        float side_to_side_distance;
        if ( char_stats.IsFacingRight() )
        {
            side_to_side_distance = hit.collider.bounds.min.x - character_right - 1.0f;
        }
        else //if ( char_stats.IsFacingLeft() )
        {
            side_to_side_distance = hit.collider.bounds.max.x - character_left + 1.0f;
        }

        transform.Translate( new Vector3( side_to_side_distance, top_to_top_distance, 0.0f ) );
        GrabLedge( hit.collider );

    }

    /// <summary>
    /// Grabs and hangs from a ledge.
    /// </summary>
    /// <param name="collision">The collider of the ledge.</param>
    private void GrabLedge( Collider2D collision )
    {
        char_stats.velocity.x = 0.0f;
        char_stats.velocity.y = 0.0f;
        char_stats.ResetJump();
        current_climb_state = ClimbState.Hanging;
        char_stats.current_master_state = CharEnums.MasterState.ClimbState;

        // variable sets to prevent weird turning when grabbing onto a wall
        // if the wall is to our left
        if ( collision.bounds.center.x < char_stats.char_collider.bounds.center.x )
        {
            char_stats.facing_direction = CharEnums.FacingDirection.Left;
            sprite_renderer.flipX = true;
        }
        // if the wall is to our right
        else
        {
            char_stats.facing_direction = CharEnums.FacingDirection.Right;
            sprite_renderer.flipX = false;
        }

        // TODO: set animation
        //char_anims.();
    }

    /// <summary>
    /// If the player jumps into a wall, grab onto it (if they meet requirements)
    /// </summary>
    public void InitiateWallGrab()
    {
        // Validation
        if ( ! player_stats.acquired_mag_grip )           { InitiateLedgeGrab(); return; } // can't climb without this upgrade

        if ( current_climb_state != ClimbState.NotClimb ) { return; } // don't start climbing if you already are
        if ( ! char_stats.IsInMidair )                    { return; } // need to be in midair
        if ( wall_grab_delay_timer < WALL_GRAB_DELAY )    { return; } // wait for it

        float left   = char_stats.char_collider.bounds.min.x;
        float top    = char_stats.char_collider.bounds.max.y;
        float right  = char_stats.char_collider.bounds.max.x;
        float bottom = char_stats.char_collider.bounds.min.y;

        Collider2D[] colliders = Physics2D.OverlapAreaAll( new Vector2( left - 1.0f, top + 1.0f ), new Vector2( right + 1.0f, bottom - 1.0f ), CollisionMasks.static_mask );
        Rect wall_rect = GetClimbableWallRect( colliders );
        if ( wall_rect.size.y == 0.0f )                   { return; } // There is no climbable wall to grab.
        if ( wall_rect.size.y < top - bottom )            { return; } // The wall is too short.
        // Only grab the wall if the player's top won't be higher than the wall's top, and the player's bottom won't be lower than the wall's bottom.
        if ( wall_rect.y < char_stats.char_collider.bounds.max.y ) { return; } // wall top is below player top.
        if ( wall_rect.y - wall_rect.size.y > char_stats.char_collider.bounds.min.y ) { return; } // wall bottom is above player bottom.
        GrabWall( wall_rect );
    }

    /// <summary>
    /// Attaches to a wall.
    /// </summary>
    /// <param name="collision">The collider of the wall. (there can be multiple (3), which one?)</param>
    private void GrabWall( Rect rectangle )
    {
        char_stats.ResetJump();
        current_climb_state = ClimbState.WallClimb;
        char_stats.current_master_state = CharEnums.MasterState.ClimbState;

        char_stats.velocity.x = 0.0f;
        char_stats.velocity.y = 0.0f;

        // prevent weird turning when grabbing onto a wall
        // if the wall is to our left
        if ( rectangle.center.x < char_stats.char_collider.bounds.center.x )
        {
            char_stats.facing_direction = CharEnums.FacingDirection.Left;
            sprite_renderer.flipX = true;
        }
        // if the wall is to our right
        else
        {
            char_stats.facing_direction = CharEnums.FacingDirection.Right;
            sprite_renderer.flipX = false;
        }

        //trigger the signal to start the wall climb animation
        char_anims.WallGrabTrigger();
        char_anims.ResetJumpDescend();
    }

    /// <summary>
    /// if the player jumps into a ceiling, grab onto it (if they meet requirements)
    /// </summary>
    public void InitiateCeilingGrab()
    {
        // Validation
        if ( ! player_stats.acquired_ceiling_grip )       { return; } // need the upgrade to do this

        if ( current_climb_state != ClimbState.NotClimb ) { return; } // don't start climbing if you already are
        if ( ! char_stats.IsInMidair )                    { return; } // need to be in midair
        if ( wall_grab_delay_timer < WALL_GRAB_DELAY )    { return; } // wait for it

        // TODO:

        GrabCeiling();
    }

    /// <summary>
    /// Attaches to the ceiling
    /// </summary>
    /// <param name="collision">The collider of the ceiling. (there can be multiple (2), which one?)</param>
    private void GrabCeiling()
    {
        //char_stats.velocity.x = 0.0f;
        //char_stats.velocity.y = 0.0f;
        //char_anims.CeilingGrabTrigger();
    }

    private void ClimbWall()
    {
        // get all colliders overlapped by player hitbox +1 buffer in direction of motion?
        // wall:    care about up / down.

        // if there is a wall climb collider above/below, and nothing interfering (solid), can move. + presently attached.

        if ( char_stats.current_master_state != CharEnums.MasterState.ClimbState ) { return; }

        Vector2 motion_direction = Vector2.zero;
        if ( input_manager.VerticalAxis > 0.0f ) { motion_direction = new Vector2( 0.0f, 1.0f ); }
        else if ( input_manager.VerticalAxis < 0.0f ) { motion_direction = new Vector2( 0.0f, -1.0f ); }

        // get the space above / below the player collider (+ buffer space) to check.
        float above = WALL_CLIMB_SPEED * Time.deltaTime * Time.timeScale;
        float below = WALL_SLIDE_SPEED * Time.deltaTime * Time.timeScale;
        // shift origin up/down based on above / below weighting, so it will be centered.
        Vector2 origin = new Vector2( char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.center.y + ( above - below ) / 2.0f ); 
        Vector2 box_size = new Vector2( char_stats.char_collider.bounds.size.x + 2.0f, char_stats.char_collider.bounds.size.y + 2.0f + above + below );

        Collider2D[] colliders = Physics2D.OverlapAreaAll( origin - box_size / 2.0f, origin + box_size / 2.0f, CollisionMasks.static_mask );
        if ( colliders.Length == 0 )
        {
            // No wall, fall?
            StopClimbing();
            return;
        }

        Rect climbable_rect = GetClimbableWallRect( colliders );

        // now, check if you can move up, or move down, based on climbable zone restrictions.
        bool can_move_up = false;
        bool can_move_down = false;

        if ( climbable_rect.size.y == 0 ) { StopClimbing(); return; } // no climbable surface

        // use the relevant climbable subzone's restrictions to determine player mobility.
        if ( climbable_rect.y > char_stats.char_collider.bounds.max.y )
        {
            // can move up.
            can_move_up = true;
        }
        if ( climbable_rect.y - climbable_rect.size.y < char_stats.char_collider.bounds.min.y )
        {
            // can move down.
            can_move_down = true;
        }

        // is_against_ledge basically = ! (can move up or down).

        // move?
        if ( can_move_up && input_manager.VerticalAxis > 0.0f )
        {
            if ( ! is_looking_away )
            {
                //char_stats.velocity.y = WALL_CLIMB_SPEED * input_manager.VerticalAxis;
                float player_top = char_stats.char_collider.bounds.max.y;
                if ( climbable_rect.y > player_top + above ) // If there is room to move full distance, move
                {
                    transform.Translate( new Vector3( 0.0f, above, 0.0f ) );
                }
                else // Not enough room, arrive at the top of the climbable area.
                {
                    transform.Translate( new Vector3( 0.0f, climbable_rect.y - player_top, 0.0f ) );

                    // TODO: Check if you can climb up the edge?
                }
            }
        }
        else if ( can_move_down && input_manager.VerticalAxis < 0.0f )
        {
            //char_stats.velocity.y = -WALL_SLIDE_SPEED;
            float player_bottom = char_stats.char_collider.bounds.min.y;
            if ( climbable_rect.y - climbable_rect.size.y < player_bottom - below ) // If there is room to move full distance, move
            {
                transform.Translate( new Vector3( 0.0f, -below ) );
            }
            else // Not enough room, arrive at the bottom of the climbable area.
            {
                transform.Translate( new Vector3( 0.0f, ( climbable_rect.y - climbable_rect.size.y ) - player_bottom, 0.0f ) );

                // Check if there is a platform below the player's feet. If so, get off the wall.
                RaycastHit2D hit = Physics2D.BoxCast( new Vector2( char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.center.y - char_stats.char_collider.size.y / 2.0f + 1.0f ), new Vector2( char_stats.char_collider.size.x, 1.0f ), 0.0f, new Vector2( 0.0f, -1.0f ), 1.0f, CollisionMasks.static_mask );
                if ( hit.collider != null )
                {
                    // If you climb down and touch the ground, stop climbing.
                    WallToGroundStart();
                    char_anims.WallSlideTouchGround();
                    StopClimbing();
                }
            }
        }
    }

    private void ClimbCeiling()
    {
        // TODO:
    }

    #region geometry testing
    private Rect GetClimbableWallRect( Collider2D[] colliders )
    {
        // need to figure out what's up with the colliders.
        // if on correct side / coords & player collider y within bounds of all ys, ok.
        // if on wrong side / coords, respect as new min/max (blocking), unless going up and fallthrough / non-blocking.
        List<Rect> climbable_zone = new List<Rect>();

        //Debug.Log("---0");
        //foreach ( Collider2D collider in colliders )
        //{
        //    Debug.Log( collider );
        //}


        // Build the climbable bounds.
        foreach ( Collider2D collider in colliders )
        {
            CustomTileData tile_data = Utils.GetCustomTileData( collider );
            if ( tile_data == null ) { continue; }
            CollisionType collision_type = tile_data.collision_type;

            // Climbable?
            if ( ! collision_type.IsWallClimbable ) { continue; }

            // Is it ACTUALLY climbable?
            if ( ! ( char_stats.facing_direction == CharEnums.FacingDirection.Left && collider.bounds.max.x < char_stats.char_collider.bounds.min.x ||
                     char_stats.facing_direction == CharEnums.FacingDirection.Right && collider.bounds.min.x > char_stats.char_collider.bounds.max.x ) )
            { continue; }
            // TODO: refine checks to check if within allowable range as well?

            // If it is really climbable, we set up the "climbable zone" to include it.
            // x,y is top left. x+size x is right, y-size y is down.
            AddAndConsolidateEdge( ref climbable_zone, new Rect( collider.bounds.min.x, collider.bounds.max.y, collider.bounds.size.x, collider.bounds.size.y ), false );
        }

        //Debug.Log( "A" );
        //foreach ( Rect rect in climbable_zone )
        //{
        //    Debug.Log( rect );
        //}

        // Reduce the climbable bounds.
        foreach ( Collider2D collider in colliders )
        {
            CustomTileData tile_data = Utils.GetCustomTileData( collider );
            if ( tile_data == null ) { continue; }
            CollisionType collision_type = tile_data.collision_type;

            if ( collision_type.IsWallClimbable &&
                ( char_stats.facing_direction == CharEnums.FacingDirection.Left && collider.bounds.max.x < char_stats.char_collider.bounds.min.x ||
                  char_stats.facing_direction == CharEnums.FacingDirection.Right && collider.bounds.min.x > char_stats.char_collider.bounds.max.x ) )
            {
                //TODO: refine checks to check if within allowable range as well?
                // Climbable wall you are probably climbing, ignore.
                // There can be a block of climbable wall jutting out from another climbable wall, so that's why we need the extra positional checks before it's safe to ignore.
                // X
                // XX
                // X
                continue;
            }
            else if ( collision_type.CanFallthrough )
            {
                // not blocking, ignore.
                // If platform's top is below player's bottom, then treat it as blocking.
                if ( collider.bounds.max.y <= char_stats.char_collider.bounds.min.y )
                {
                    RemoveEdge( ref climbable_zone, new Rect( collider.bounds.min.x, collider.bounds.max.y, collider.bounds.size.x, collider.bounds.size.y ), false );
                }
                continue;
            }
            else
            {
                // TODO: differentiate enemies, etc?
                // Blocking
                RemoveEdge( ref climbable_zone, new Rect( collider.bounds.min.x, collider.bounds.max.y, collider.bounds.size.x, collider.bounds.size.y ), false );
            }
        }

        //Debug.Log( "B" );
        //foreach ( Rect rect in climbable_zone )
        //{
        //    Debug.Log( rect );
        //}

        if ( climbable_zone.Count == 0 ) { return new Rect( 0.0f, 0.0f, 0.0f, 0.0f ); } // no climbable surface

        // How do we parse this climbable zone, which may contain numerous climbable subzones and intervening gaps? 
        // Arbitrarily. Find the contiguous region that overlaps player head-level. It must also at least go down to the player's feet.
        // If one doesn't exist, stop climbing.
        Rect climbable_rect = new Rect( 0.0f, 0.0f, 0.0f, 0.0f );
        foreach ( Rect rect in climbable_zone )
        {
            if ( rect.y >= char_stats.char_collider.bounds.max.y && rect.y - rect.size.y <= char_stats.char_collider.bounds.max.y ) // overlaps head
            {
                if ( /*rect.y >= char_stats.char_collider.bounds.min.y &&*/ rect.y - rect.size.y <= char_stats.char_collider.bounds.min.y ) // extends to or beyond feet.
                {
                    climbable_rect = rect;
                }
            }
        }
        if ( climbable_rect.size.y == 0 ) { return new Rect( 0.0f, 0.0f, 0.0f, 0.0f ); } // no climbable surface

        //Debug.Log( climbable_rect );
        return climbable_rect;
    }

    // TODO:
    private Rect GetClimbableCeilingRect()
    {
        return new Rect( 0.0f, 0.0f, 0.0f, 0.0f );
    }

    /// <summary>
    /// Creates a list of non-contiguous colliders.
    /// </summary>
    /// <param name="aggregator">Input / Output. List of edges, with no overlaps.</param>
    /// <param name="new_rectangle">Representation of the box collider.</param>
    /// <param name="is_horizontal">If true, will check edges along the x axis. Otherwise, will check edges along the y axis.</param>
    private void AddAndConsolidateEdge( ref List<Rect> aggregator, Rect new_rectangle, bool is_horizontal )
    {
        // y is top, y - size is bottom.
        // x is left, x + size is right.
        // Why? To be consistent with unity coords for 2D.

        // Goal: Test if a surface is continuous, and determine its length. 
        // Create a list of non-contiguous colliders. Combine the edges of contiguous colliders into one edge.
        // This only combines line segments, not rects, but can combine them horizontally or vertically.
        // so, we can detect if the edge 1) has any gaps, and 2) is above or below a certain length.
        float new_top    = new_rectangle.y;
        float new_bottom = new_rectangle.y - new_rectangle.size.y;
        float new_left   = new_rectangle.x;
        float new_right  = new_rectangle.x + new_rectangle.size.x;

        if ( new_top == new_bottom && new_left == new_right ) { return; } // 0 dimensions, skip.

        List<int> indecies_to_merge = new List<int>();

        if ( aggregator.Count == 0 ) // only one element, no checks needed.
        {
            aggregator.Add( new_rectangle );
            return;
        }
        
        // Do not allow edge overlaps.
        for ( int i = 0; i < aggregator.Count; i++ )
        {
            Rect rect = aggregator[ i ];
            float existing_top    = rect.y;
            float existing_bottom = rect.y - rect.size.y;
            float existing_left   = rect.x;
            float existing_right  = rect.x + rect.size.x;

            bool is_within, does_overlap;
            if ( is_horizontal ) // only test x axis
            {
                is_within = new_right <= existing_right && new_left >= existing_left;
                does_overlap = ( new_right <= existing_right && new_right >= existing_left ) || ( new_left <= existing_right && new_left >= existing_left );
            }
            else // only test y axis
            {
                is_within = new_top <= existing_top && new_bottom >= existing_bottom;
                does_overlap = ( new_top <= existing_top && new_top >= existing_bottom ) || ( new_bottom <= existing_top && new_bottom >= existing_bottom );
            }

            // IF the new edge is completely within any existing edge, completely ignore it. No need to do any more.
            if ( is_within ) { return; }

            // IF the new edge overlaps this existing edge, merge them.
            if ( does_overlap )
            {
                // Update the dimensions of the edge.
                new_top    = Mathf.Max( existing_top, new_top );       // highest high
                new_bottom = Mathf.Min( existing_bottom, new_bottom ); // lowest low
                new_left   = Mathf.Min( existing_left, new_left );     // lowest low
                new_right  = Mathf.Max( existing_right, new_right );   // highest high
                indecies_to_merge.Add( i ); // merge this edge together with the new edge.
            }

            // IF the new edge neither overlaps nor is contained, just add it after confirming that for all colliders.
        }

        // "Merge" by deleting old constituent edges, if any exist
        if ( indecies_to_merge.Count > 0 )
        {
            for ( int i = aggregator.Count - 1; i >= 0; i-- ) // backwards iteration to prevent meaningful index shifts
            {
                if ( ! indecies_to_merge.Contains( i ) ) { continue; } // slow.
                // this is somewhat slow, could potentially use LinkedList for slightly better performance, but expected input size makes gains marginal.
                aggregator.RemoveAt( i );
            }
        }
        // Then add new (consolidated) edge.
        aggregator.Add( new Rect( new_left, new_top, new_right - new_left, new_top - new_bottom ) );
    }

    /// <summary>
    /// Removes a line segment from the consolidated edge of all relevant colliders.
    /// </summary>
    /// <param name="aggregator">Input / Output. List of edges, with no overlaps.</param>
    /// <param name="new_rectangle">The dimensions of the "area" / line segment to subtract from the edge.</param>
    /// <param name="is_horizontal">If true, will check edges along the x axis. Otherwise, will check edges along the y axis.</param>
    private void RemoveEdge( ref List<Rect> aggregator, Rect new_rectangle, bool is_horizontal )
    {
        float new_top    = new_rectangle.y;
        float new_bottom = new_rectangle.y - new_rectangle.size.y;
        float new_left   = new_rectangle.x;
        float new_right  = new_rectangle.x + new_rectangle.size.x;

        if ( aggregator.Count == 0 ) { return; } // nothing to subtract from.
        if ( new_top == new_bottom && new_left == new_right ) { return; } // 0 dimensions, skip.

        List<int> indecies_to_delete = new List<int>();

        for ( int i = 0; i < aggregator.Count; i++ )
        {
            Rect rect = aggregator[ i ];
            float existing_top    = rect.y;
            float existing_bottom = rect.y - rect.size.y;
            float existing_left   = rect.x;
            float existing_right  = rect.x + rect.size.x;

            bool contains, does_overlap, is_within;
            if ( is_horizontal ) // only test x axis
            {
                contains = new_right >= existing_right && new_left <= existing_left;
                is_within = new_right < existing_right && new_left > existing_left;
                does_overlap = ( new_right <= existing_right && new_right >= existing_left ) || ( new_left <= existing_right && new_left >= existing_left );
            }
            else // only test y axis
            {
                contains = new_top >= existing_top && new_bottom <= existing_bottom;
                is_within = new_top < existing_top && new_bottom > existing_bottom;
                does_overlap = ( new_top <= existing_top && new_top >= existing_bottom ) || ( new_bottom <= existing_top && new_bottom >= existing_bottom );
            }

            if ( ! contains && ! is_within && ! does_overlap ) { continue; } // Can't remove edge from this collider, check others.

            // If this new rect will remove the existing collider, because it is exactly its size or bigger and contains it, just delete it.
            if ( contains )
            {
                indecies_to_delete.Add( i );
                continue;
            }

            // if new rect is within existing rect (not equal, entirely within)
            // split the existing rect in 2: above and below / to the left and to the right.
            // => if either piece is 0 height/width b/c of equality, delete it rather than adding it. Move this into the overlap case to do just that.
            if ( is_within )
            {
                // could split this based on horizontal/vertical, but unused dimension will be 0, so no need.
                float x = existing_left;
                float y = existing_top;
                float width = new_left - existing_left;
                float height = existing_top - new_top;
                aggregator[ i ] = new Rect( x, y, width, height );

                x = new_right;
                y = new_bottom;
                width = existing_right - new_right;
                height = new_bottom - existing_bottom;
                aggregator.Add( new Rect( x, y, width, height ) );
                continue;
            }

            // if new rect overlaps the existing rect, and neither contains the other, and they aren't equal:
            // trim the existing rect.
            if ( does_overlap )
            {
                float x = 0.0f;
                float y = 0.0f;
                float width = 0.0f;
                float height = 0.0f;

                if ( ! is_horizontal )
                {
                    if ( new_bottom > existing_bottom ) // chop off the top
                    {
                        y = new_bottom;
                        height = new_bottom - existing_bottom;
                    }
                    else // chop off the bottom
                    {
                        y = existing_top;
                        height = existing_top - new_top;
                    }
                }
                else
                {
                    if ( new_left > existing_left ) // chop off the right
                    {
                        x = existing_left;
                        width = new_left - existing_left;
                    }
                    else // chop off the left
                    {
                        x = new_right;
                        width = existing_right - new_right;
                    }
                }
                
                aggregator[ i ] = new Rect( x, y, width, height );
            }
        }

        // Delete
        if ( indecies_to_delete.Count > 0 )
        {
            for ( int i = aggregator.Count - 1; i >= 0; i-- ) // backwards iteration to prevent meaningful index shifts
            {
                if ( ! indecies_to_delete.Contains( i ) ) { continue; } // slow.
                // this is somewhat slow, could potentially use LinkedList for slightly better performance, but expected input size makes gains marginal.
                aggregator.RemoveAt( i );
            }
        }
    }
    #endregion

    // TODO:
    private void CanTransitionFromCeilingToWallAbove()
    {
        // check if you are at the corner of a wall and a ceiling.
        // [X][X][X][ ]
        // [X][X][X][ ]
        // [X][P][P][ ]
        // check that the wall is tall enough and contiguous climbable geometry.
        // check that there is enough empty space to transition.
    }

    private void CanTransitionFromCeilingToWallBelow()
    {
        // check if you are at the corner of a wall and a ceiling.
        // [X][X][X]
        // [X][P][P]
        // [X][ ][?]
        // check that the wall is tall enough and contiguous climbable geometry.
        // check that there is enough empty space to transition.
    }

    private void CanTransitionFromWallToCeilingAbove()
    {
        // check if you are at the corner of a wall and a ceiling.
        // [X][X][X]
        // [X][P][ ]
        // [X][P][?]
        // check that the ceiling is wide enough, and contiguous climbable geometry.
        // check that there is enough empty space to transition.
    }

    /*
    private void SlopeTest()
    {
        // raycast from bottom left or right, left or right. Support non-box colliders.

        RaycastHit2D hit;
        float angle = Vector2.Angle( hit.normal, Vector2.up );
        if ( angle <= 45.0f )
        {
            transform.position.x += velocity.x * Mathf.Cos( angle * Mathf.Deg2Rad ) * Time.deltaTime * Time.timeScale;
            transform.position.y += velocity.x * Mathf.Sin( angle * Mathf.Deg2Rad ) * Time.deltaTime * Time.timeScale;
            return;
        }
    }
    */
}
