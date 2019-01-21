using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

// TODO: fallthrough platform top (stand/crouch) -> fallthrough platform bottom (ceiling climb)? :(
// TODO: fallthrough platform bottom (ceiling climb) -> fallthrough platform top (stand/crouch)? :(
// TODO: crouch up (ledge and wall)
// TODO: wall climbing without jumping (from crouch?)
// TODO: animation integration
// TODO: refactor duplicative code (esp. ledge vs. wallclimb top, potentially lots of other similar stuff)

///<summary> Handles wall and ceiling climbing</summary>
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

    private const float PHYSICS_STEP_SIZE   =   0.005f; // pixels
    private const float WALL_CLIMB_SPEED    =  70.0f; // pixels / second
    private const float WALL_SLIDE_SPEED    = 160.0f; // pixels / second
    private const float CEILING_CLIMB_SPEED =  64.0f; // pixels / second

    private bool is_at_top;
    private bool is_at_bottom;
    private bool is_at_left;
    private bool is_at_right;

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
    private void ClimbMovementInput()
    {
        if ( char_stats.current_master_state != CharEnums.MasterState.ClimbState ) { return; }

        LookAway();

        // Jump logic.
        if ( input_manager.JumpInputInst )
        {
            if ( is_looking_away )
            {
                StopClimbing();
                if ( input_manager.VerticalAxis >= 0.0f )
                {
                    JumpAway();
                }
            }

            if ( input_manager.VerticalAxis < 0.0f )
            {
                char_anims.DropFromWallTrigger();
                StopClimbing();
                JumpDown();
            }
            // TODO: consider scaling jump away based on horizontal and vertical inputs between away and down, and defaulting neutral jump to away.
        }
    }

    /// <summary>
    /// Parses movement input when the player is climbing on the ceiling.
    /// </summary>
    private void CeilingClimbMovementInput()
    {
        LookAway();

        if ( input_manager.JumpInputInst && input_manager.VerticalAxis < 0.0f ) // Could remove down requirement. Kept for consistency.
        {
            // TODO: animations
            StopClimbingCeiling();
        }
    }

    /// <summary>
    /// Parses movement input when the player is hanging from a wall.
    /// </summary>
    private void HangMovementInput()
    {
        LookAway();

        is_at_top = true;

        // When hanging, player is ALWAYS locked at the edge, don't need to move or do confirmation checks.
        if ( can_stand_up_from_hang && ( ( char_stats.IsFacingLeft() && input_manager.HorizontalAxis < -0.5f ) || ( char_stats.IsFacingRight() && input_manager.HorizontalAxis > 0.5f ) ) )
        {
            // Pull yourself up and stand on platform.
            float x = char_stats.STANDING_COLLIDER_SIZE.x; // right
            if ( char_stats.IsFacingLeft() ) { x = -char_stats.STANDING_COLLIDER_SIZE.x; }
            transform.Translate( new Vector3( x, char_stats.STANDING_COLLIDER_SIZE.y + 1.0f, 0.0f ) );

            current_climb_state = ClimbState.NotClimb;
            char_stats.current_master_state = CharEnums.MasterState.DefaultState;
        }
        else if ( input_manager.VerticalAxis < 0.5f && input_manager.JumpInputInst ) // Could drop jump button requirement here. Kept for consistency.
        {
            // Drop.
            //TODO: animations
            StopClimbing();
            JumpDown();
        }
        else if ( input_manager.JumpInputInst && is_looking_away ) // Could drop looking away requirement here. Kept for consistency.
        {
            // Jump away from the wall.
            StopClimbing();
            JumpAway();
        }
        // TODO: consider scaling jump away based on horizontal and vertical inputs between away and down, and defaulting neutral jump to away.
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

    /// <summary>
    /// Jumps away from the wall the player is climbing / hanging from.
    /// </summary>
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

        float horizontal_scale = Mathf.Abs( input_manager.HorizontalAxis );
        // horizontal_scale = 1.0f; // restore this line to always use default horizontal acceleration.

        player_script.SetHorizontalJumpVelocity( ( player_script.GetJumpHorizontalSpeedMin() + player_script.GetJumpHorizontalSpeedMax() ) / 2.0f * horizontal_scale );
        player_script.SetFacing();
    }

    /// <summary>
    /// Falls down from the wall the player is climbing / hanging from.
    /// </summary>
    private void JumpDown()
    {
        // TODO: move 1px away if wall climbing (or abort)

        // Fall
        char_stats.is_jumping = false;
        char_stats.jump_input_time = 0.0f;
        char_anims.FallTrigger();
        player_script.SetHorizontalJumpVelocity( 0.0f );
    }

    // TODO: Support jumping down and away?

    /// <summary>
    /// Moves the player up or down walls, or across ceilings, and handles state transitions.
    /// </summary>
    private void MovePlayer()
    {
        if ( char_stats.current_master_state != CharEnums.MasterState.ClimbState )
        {
            CheckNonClimbMoves();
            return;
        }

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
    /// Parses input from non-climbing states, which may transition into a climbing state.
    /// </summary>
    private void CheckNonClimbMoves()
    {
        GrabWallFromFloorEdge();
        GrabCeilingFromFloorAbove();
    }

    /// <summary>
    /// Go from the standing at the edge of a platform to on the wall below that platform, if possible.
    /// </summary>
    private void GrabWallFromFloorEdge()
    {
        if ( !player_stats.acquired_mag_grip ) { return; }

        // if you are at the edge of a sticky edge, and press down, check for walls.
        if ( player_script.IsCrouchedAbuttingFacingStickyLedge() && input_manager.VerticalAxis < 0.0f )
        {
            // we know the player's edge is 0.5 pixels from the edge.
            float left;
            float top    = transform.position.y + char_stats.char_collider.size.y / 2.0f + char_stats.char_collider.offset.y;
            //float bottom = transform.position.y - char_stats.char_collider.size.y / 2.0f + char_stats.char_collider.offset.y; // only approximate
            float width  = 1.0f;
            float height = char_stats.STANDING_COLLIDER_SIZE.y + char_stats.char_collider.size.y;
            // where we will put the player, approximately
            float new_left, new_right;
            float new_top    = top - char_stats.char_collider.size.y - 1.0f;
            float new_bottom = new_top - char_stats.STANDING_COLLIDER_SIZE.y;
            CharEnums.FacingDirection new_facing = char_stats.facing_direction;

            if ( char_stats.IsFacingLeft() )
            {
                left = transform.position.x - char_stats.char_collider.size.x / 2.0f + char_stats.char_collider.offset.x - 0.5f;
                new_right = left - 0.5f - PHYSICS_STEP_SIZE;
                new_left = new_right - char_stats.STANDING_COLLIDER_SIZE.x;
                new_facing = CharEnums.FacingDirection.Right;
            }
            else //if ( char_stats.IsFacingRight() )
            {
                left = transform.position.x + char_stats.char_collider.size.x / 2.0f + char_stats.char_collider.offset.x + 0.5f - width;
                new_left = left + 1.5f + PHYSICS_STEP_SIZE;
                new_right = new_left + char_stats.STANDING_COLLIDER_SIZE.x;
                new_facing = CharEnums.FacingDirection.Left;
            }

            // Check that there is enough climbable wall
            Collider2D[] colliders = Physics2D.OverlapAreaAll( new Vector2( left, top ), new Vector2( left + width, top - height ), CollisionMasks.static_mask );
            Rect rect = GetClimbableWallRect( colliders, new_left, new_right, new_top, new_bottom, new_facing );
            if ( rect.height < char_stats.STANDING_COLLIDER_SIZE.y ) { return; }
            // get the precise coordinate to use for the top.
            Debug.Log( rect );
            new_top = rect.y;
            // don't need new_bottom anymore, so won't update

            // Check that there is enough empty space to move: from where head is to where feet will end up, off the edge of the platform
            // [ ][P]   [P][ ]
            // [ ][P]   [P][ ]
            // [ ][X]   [X][ ]
            // [ ][X]   [X][ ]
            float empty_x = new_left;
            float empty_y = top;
            float empty_width  = char_stats.STANDING_COLLIDER_SIZE.x;
            float empty_height = char_stats.STANDING_COLLIDER_SIZE.y + char_stats.char_collider.size.y + 1.0f;
            if ( Utils.AreaContainsBlockingGeometry( empty_x, empty_y, empty_width, empty_height ) ) { return; }

            // Move
            float old_left = transform.position.x - char_stats.char_collider.size.x / 2.0f + char_stats.char_collider.offset.x;
            float old_top  = transform.position.y + char_stats.char_collider.size.y / 2.0f + char_stats.char_collider.offset.y;
            // Align tops of different sized hitboxes by getting the difference between the player's position (center) and the top for each box.
            float hitbox_height_offset = ( char_stats.char_collider.size.y / 2.0f + char_stats.char_collider.offset.y ) - ( char_stats.STANDING_COLLIDER_SIZE.y / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.y );
            float hitbox_width_offset  = ( char_stats.char_collider.size.x / 2.0f + char_stats.char_collider.offset.x ) - ( char_stats.STANDING_COLLIDER_SIZE.x / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.x );
            Vector2 delta = new Vector2( new_left - old_left + hitbox_width_offset, new_top - old_top + hitbox_height_offset );

            char_stats.current_master_state = CharEnums.MasterState.ClimbState;
            char_stats.is_crouching = false;
            char_stats.velocity.x = 0.0f;
            char_stats.velocity.y = 0.0f;
            ChangeClimbingState( delta, ClimbState.WallClimb, new_facing );
            player_stats.FreezePlayer( 0.5f );
        }
    }

    /// <summary>
    /// Go from standing on top of a fallthrough platform that is ceiling climbable to under it, if possible.
    /// </summary>
    private void GrabCeilingFromFloorAbove() // TODO:
    {
        if ( ! player_stats.acquired_ceiling_grip ) { return; }

        // if you are above a fallthrough platform, and press down, check for ceiling.
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
    /// If the player jumps into the right kind of geometry just right, grab the edge of it, and hang off it.
    /// </summary>
    public void InitiateLedgeGrab()
    {
        // left/right edge has no meaning here. Block behaviour that depends on it.
        is_at_left = false;
        is_at_right = false;
        // always considered at top
        is_at_top = true;
        // defaults, overridable
        is_at_bottom = false;
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

        float character_top   = char_stats.char_collider.bounds.max.y;
        float character_left  = char_stats.char_collider.bounds.min.x;
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
            // if the platform is too narrow, need to check 1 player width beyond the player's rightmost bound.
            x2 = Mathf.Max( hit.collider.bounds.max.x, x1 + player_size.x * 2.0f );
        }
        else //if ( char_stats.IsFacingLeft() )
        {
            x1 = character_right;
            // If the platform is too narrow, need to check 1 player width beyond the player's leftmost bound.
            x2 = Mathf.Min( hit.collider.bounds.min.x, x1 - player_size.x * 2.0f );
        }
        if ( x2 < x1 ) { float temp = x1; x1 = x2; x2 = temp; } // swap so x2 is always larger

        const float MINIMUM_CLEARANCE = 32.0f - 0.5f; // pixels. Reduced slightly to stop false positives for EXACTLY 1 tile gaps.
        if ( Utils.AreaContainsBlockingGeometry( x1, platform_top + MINIMUM_CLEARANCE, x2 - x1, MINIMUM_CLEARANCE - 0.5f ) ) { return; }
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
        if ( x2 < x1 ) { float temp = x1; x1 = x2; x2 = temp; } // swap so x2 is always larger.

        const float MINIMUM_STANDING_CLEARANCE = 64.0f - 0.5f; // pixels. Reduced slightly to stop false positive for EXACTLY 2 tile gaps.
        if ( Utils.AreaContainsBlockingGeometry( x1, platform_top + MINIMUM_STANDING_CLEARANCE, x2 - x1, MINIMUM_STANDING_CLEARANCE - 0.5f ) ) { return; }
        if ( MINIMUM_STANDING_CLEARANCE >= char_stats.STANDING_COLLIDER_SIZE.y ) { can_stand_up_from_hang = true; } // Can stand up.
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

        // NOTE: this isn't very high fidelity. TODO: improve
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
        // left/right edge has no meaning here. Block behaviour that depends on it.
        is_at_left = false;
        is_at_right = false;
        // defaults, overridable.
        is_at_top = false;
        is_at_bottom = false;

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
        if ( wall_rect.y < top )                          { return; } // wall top is below player top.
        if ( wall_rect.y - wall_rect.size.y > bottom )    { return; } // wall bottom is above player bottom.
        GrabWall( wall_rect );
    }

    /// <summary>
    /// Attaches to a wall.
    /// </summary>
    /// <param name="rectangle">
    /// A rect representing the aggregate of the collider(s) (there can be multiple) of the wall.
    /// x = left, y = top, width and height behave as expected.
    /// The value of x and width will not be well defined if using GetClimbableWallRect to generate the rect.
    /// </param>
    private void GrabWall( Rect rectangle )
    {
        char_stats.ResetJump();
        current_climb_state = ClimbState.WallClimb;
        char_stats.current_master_state = CharEnums.MasterState.ClimbState;

        char_stats.velocity.x = 0.0f;
        char_stats.velocity.y = 0.0f;

        // prevent weird turning when grabbing onto a wall
        // if the wall is to our left (rough test)
        // TODO: improve fidelity of this test, make getrect functions care more about "ignored" axis.
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
        char_anims.ResetJumpDescend(); // not particularly clean as a 2 parter
    }

    /// <summary>
    /// if the player jumps into a ceiling, grab onto it (if they meet requirements)
    /// </summary>
    /// <returns>True if the ceiling was grabbed, false otherwise.</returns>
    public bool InitiateCeilingGrab()
    {
        // top/bottom edge has no meaning here. Block behaviour that depends on it.
        is_at_top = false;
        is_at_bottom = false;
        // defaults, overridable
        is_at_left = false;
        is_at_right = false;

        // Validation
        if ( ! player_stats.acquired_ceiling_grip )         { return false; } // need the upgrade to do this

        if ( current_climb_state != ClimbState.NotClimb )   { return false; } // don't start climbing if you already are
        if ( ! char_stats.IsInMidair )                      { return false; } // need to be in midair
        if ( wall_grab_delay_timer < WALL_GRAB_DELAY )      { return false; } // wait for it

        // TODO: enforce change of collider on exit / interrupt. (interrupt: only possible if enough space)
        // Use the ceiling climb collider for this check (without actually changing to it). Box centered on player position.
        float left   = transform.position.x + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x - char_stats.CEILING_CLIMB_COLLIDER_SIZE.x / 2.0f;
        float right  = transform.position.x + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x + char_stats.CEILING_CLIMB_COLLIDER_SIZE.x / 2.0f;
        float top    = transform.position.y + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y + char_stats.CEILING_CLIMB_COLLIDER_SIZE.y / 2.0f;
        float bottom = transform.position.y + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y + char_stats.CEILING_CLIMB_COLLIDER_SIZE.y / 2.0f;

        if ( Utils.AreaContainsBlockingGeometry( left, top, right - left, top - bottom ) ) { return false; } // If changing the player's hitbox will put a wall inside of them: ABORT!

        Collider2D[] colliders = Physics2D.OverlapAreaAll( new Vector2( left - 1.0f, top + 1.0f ), new Vector2( right + 1.0f, bottom - 1.0f ), CollisionMasks.static_mask );
        Rect ceiling_rect = GetClimbableCeilingRect( colliders );
        if ( ceiling_rect.size.x == 0.0f )                  { return false; } // There is no climbable ceiling to grab.
        if ( ceiling_rect.size.x < right - left )           { return false; } // The ceiling is not wide enough.
        // Only grab the ceiling if the player fits within it.
        if ( ceiling_rect.x > left )                        { return false; } // ceiling left edge is right of player's left edge.
        if ( ceiling_rect.x + ceiling_rect.size.x < right ) { return false; } // ceiling right edge is left of player's right edge.

        GrabCeiling();
        return true;
    }

    /// <summary>
    /// Attaches to the ceiling
    /// </summary>
    private void GrabCeiling()
    {
        char_stats.CeilingClimbHitBox();
        current_climb_state = ClimbState.CeilingClimb;
        char_stats.current_master_state = CharEnums.MasterState.ClimbState;

        char_stats.ResetJump();
        char_stats.velocity.x = 0.0f;
        char_stats.velocity.y = 0.0f;

        // Animation
        char_anims.CeilingGrabTrigger();
        char_anims.ResetJumpDescend(); // not particularly clean as a 2 parter
    }

    /// <summary>
    /// Handles the logic for climbing a wall.
    /// </summary>
    private void ClimbWall()
    {
        // get all colliders overlapped by player hitbox +1 buffer in direction of motion?
        // wall:    care about up / down.

        // if there is a wall climb collider above/below, and nothing interfering (solid), can move. + presently attached.

        if ( char_stats.current_master_state != CharEnums.MasterState.ClimbState ) { return; }
        if ( current_climb_state != ClimbState.WallClimb ) { return; }

        //Vector2 motion_direction = Vector2.zero;
        //if      ( input_manager.VerticalAxis > 0.0f ) { motion_direction = new Vector2( 0.0f,  1.0f ); }
        //else if ( input_manager.VerticalAxis < 0.0f ) { motion_direction = new Vector2( 0.0f, -1.0f ); }
        float input_scale_factor = 1.0f;
        if ( input_manager.VerticalAxis > 0.0f ) { input_scale_factor = input_manager.VerticalAxis; }

        // get the space above / below the player collider (+ buffer space) to check.
        float above = WALL_CLIMB_SPEED * Time.deltaTime * Time.timeScale * input_scale_factor;
        float below = WALL_SLIDE_SPEED * Time.deltaTime * Time.timeScale;
        // shift origin up/down based on above / below weighting, so it will be centered.
        Vector2 origin = new Vector2( char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.center.y + ( above - below ) / 2.0f ); 
        Vector2 box_size = new Vector2( char_stats.char_collider.bounds.size.x + 2.0f, char_stats.char_collider.bounds.size.y + 2.0f + above + below );

        Collider2D[] colliders = Physics2D.OverlapAreaAll( origin - box_size / 2.0f, origin + box_size / 2.0f, CollisionMasks.static_mask );
        if ( colliders.Length == 0 )
        {
            // No wall, fall?
            char_anims.DropFromWallTrigger();
            StopClimbing();
            return;
        }

        Rect climbable_rect = GetClimbableWallRect( colliders );

        // now, check if you can move up, or move down, based on climbable zone restrictions.
        bool can_move_up = false;
        bool can_move_down = false;

        if ( climbable_rect.size.y == 0 ) // no climbable surface
        {
            char_anims.DropFromWallTrigger();
            StopClimbing();
            return;
        }

        // use the relevant climbable subzone's restrictions to determine player mobility.
        if ( climbable_rect.y > char_stats.char_collider.bounds.max.y )
        {
            // can move up.
            can_move_up = true;
            is_at_top = false;
        }
        else
        {
            is_at_top = true;
        }
        if ( climbable_rect.y - climbable_rect.size.y < char_stats.char_collider.bounds.min.y )
        {
            // can move down.
            can_move_down = true;
            is_at_bottom = false;
        }
        else
        {
            is_at_bottom = true;
        }

        // move?
        if ( can_move_up && input_manager.VerticalAxis > 0.0f )
        {
            if ( ! is_looking_away )
            {
                //ESSENTIALLY: char_stats.velocity.y = WALL_CLIMB_SPEED * input_manager.VerticalAxis;, respecting collisions
                float player_top = char_stats.char_collider.bounds.max.y;
                if ( climbable_rect.y > player_top + above ) // If there is room to move full distance, move
                {
                    transform.Translate( 0.0f, above, 0.0f );
                }
                else // Not enough room, arrive at the top of the climbable area.
                {
                    transform.Translate( 0.0f, climbable_rect.y - player_top, 0.0f );
                }
            }
        }
        else if ( can_move_down && input_manager.VerticalAxis < 0.0f )
        {
            //ESSENTIALLY: char_stats.velocity.y = -WALL_SLIDE_SPEED;, respecting collisions
            float player_bottom = char_stats.char_collider.bounds.min.y;
            if ( climbable_rect.y - climbable_rect.size.y < player_bottom - below ) // If there is room to move full distance, move
            {
                transform.Translate( 0.0f, -below, 0.0f );
            }
            else // Not enough room, arrive at the bottom of the climbable area.
            {
                transform.Translate( 0.0f, ( climbable_rect.y - climbable_rect.size.y ) - player_bottom, 0.0f );

                // Check if there is a platform below the player's feet. If so, get off the wall.
                RaycastHit2D hit = Physics2D.BoxCast( new Vector2( char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.center.y - char_stats.char_collider.size.y / 2.0f + 1.0f ), new Vector2( char_stats.char_collider.size.x, 1.0f ), 0.0f, new Vector2( 0.0f, -1.0f ), 1.0f, CollisionMasks.static_mask );
                if ( hit.collider != null )
                {
                    // If you climb down and touch the ground, stop climbing.
                    char_anims.WallSlideTouchGround();
                    StopClimbing();
                }
            }
        }

        // Pull up: press towards wall
        if ( is_at_top && ( ( char_stats.IsFacingLeft() && input_manager.HorizontalAxis < -0.5f ) || ( char_stats.IsFacingRight() && input_manager.HorizontalAxis > 0.5f ) ) )
        {
            PullYourselfUpFromLedge();
        }

        // Corner
        else if ( is_at_top && input_manager.VerticalAxis > 0.0f )
        {
            //if ( ! PullYourselfUpFromLedge() )
            //{
            GoFromWallToCeilingAbove();
            //}
        }
        else if ( is_at_bottom && input_manager.VerticalAxis < 0.0f )
        {
            GoFromWallToCeilingBelow();
        }
    }

    /// <summary>
    /// If possible, causes the player to pull themselves up onto a ledge from climbing the side of it.
    /// </summary>
    /// <returns>True if the player pulls themselves up onto the ledge.</returns>
    private bool PullYourselfUpFromLedge()
    {
        // TODO: consider refactoring.
        // This is VERY similar to ledge grab, but instead of using persistent state vars, we just run a check.
        float x1,x2;
        Vector2 player_size = char_stats.char_collider.size;
        float character_left = char_stats.char_collider.bounds.min.x;
        float character_right = char_stats.char_collider.bounds.max.x;
        float character_top = char_stats.char_collider.bounds.max.y;
        float direction;

        if ( char_stats.IsFacingRight() )
        {
            x1 = character_left;
            x2 = x1 + player_size.x * 2.0f;
            direction = 1.0f;
        }
        else //if ( char_stats.IsFacingLeft() )
        {
            x2 = character_right; // x2 >= x1 must be true
            x1 = x2 - player_size.x * 2.0f;
            direction = -1.0f;
        }

        const float MINIMUM_STANDING_CLEARANCE = 64.0f - 0.5f; // pixels
        if ( Utils.AreaContainsBlockingGeometry( x1, character_top + MINIMUM_STANDING_CLEARANCE, x2 - x1, MINIMUM_STANDING_CLEARANCE - 0.5f ) ) { return false; }
        if ( MINIMUM_STANDING_CLEARANCE < char_stats.STANDING_COLLIDER_SIZE.y ) { return false; }

        // TODO: crouch fallback

        // TODO: animation
        transform.Translate( direction * player_size.x, char_stats.STANDING_COLLIDER_SIZE.y + 1.0f, 0.0f );
        return true;
    }

    /// <summary>
    /// Handles the logic of climbing the ceiling.
    /// </summary>
    private void ClimbCeiling()
    {
        // get all colliders overlapped by player hitbox +1 buffer in direction of motion?
        // ceiling:    care about left / right.
        // if there is a ceiling climb collider left/right, and nothing interfering (solid), can move.

        if ( char_stats.current_master_state != CharEnums.MasterState.ClimbState ) { return; }
        if ( current_climb_state != ClimbState.CeilingClimb ) { return; }

        float input_scale_factor = 1.0f;
        if ( Mathf.Abs( input_manager.HorizontalAxis ) > 0.0f ) { input_scale_factor = Mathf.Abs( input_manager.HorizontalAxis ); }

        // get the space to the left / right of the player collider (+ buffer space) to check.
        // 3 zones: [space to the left we could move into] [Player collider @ current position (+ 1 px buffer)] [space to the right we could move into]
        float max_distance = CEILING_CLIMB_SPEED * Time.deltaTime * Time.timeScale * input_scale_factor; // furthest left or right we can move this frame.
        // since amount of distance we can traverse left or right is balanced, keep ordinary centerpoint.
        Vector2 origin = new Vector2( char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.center.y );
        // expand the area to check beyond the player's collider by a 1 pixel buffer, plus the distance we could move, both left and right.
        Vector2 box_size = new Vector2( char_stats.char_collider.bounds.size.x + 2.0f + max_distance * 2.0f, char_stats.char_collider.bounds.size.y + 2.0f );
        // get all geometry worth checking in that designated area.
        Collider2D[] colliders = Physics2D.OverlapAreaAll( origin - box_size / 2.0f, origin + box_size / 2.0f, CollisionMasks.static_mask );
        if ( colliders.Length == 0 )
        {
            // No ceiling, fall?
            StopClimbingCeiling();
            return;
        }

        Rect climbable_rect = GetClimbableCeilingRect( colliders );

        // now, check if you can move left, or can move right, based on climbable zone restrictions.
        bool can_move_left = false;
        bool can_move_right = false;

        if ( climbable_rect.size.x == 0.0f ) // no climbable surface
        {
            StopClimbingCeiling();
            return;
        }

        // use the relevant climbable subzone's restrictions to determine player mobility
        if ( climbable_rect.x < char_stats.char_collider.bounds.min.x )
        {
            // can move left
            can_move_left = true;
            is_at_left = false;
        }
        else
        {
            is_at_left = true;
        }
        if ( climbable_rect.x + climbable_rect.size.x > char_stats.char_collider.bounds.max.x )
        {
            // can move right
            can_move_right = true;
            is_at_right = false;
        }
        else
        {
            is_at_right = true;
        }

        // move?
        if ( can_move_right && input_manager.HorizontalAxis > 0.0f )
        {
            //ESSENTIALLY: char_stats.velocity.x = CEILING_CLIMB_SPEED * input_manager.HorizontalAxis;, respecting collisions
            float player_right = char_stats.char_collider.bounds.max.x;
            if ( climbable_rect.x + climbable_rect.size.x > player_right + max_distance ) // if there is room to move full distance, move.
            {
                transform.Translate( max_distance, 0.0f, 0.0f );
            }
            else // otherwise, not enough room, arrive at the right edge of the climbable area.
            {
                transform.Translate( (climbable_rect.x + climbable_rect.size.x) - player_right, 0.0f, 0.0f );
            }
        }
        else if ( can_move_left && input_manager.HorizontalAxis < 0.0f )
        {
            //ESSENTIALLY: char_stats.velocity.x = -CEILING_CLIMB_SPEED * -input_manager.HorizontalAxis;, respecting collisions
            float player_left = char_stats.char_collider.bounds.min.x;
            if ( climbable_rect.x < player_left - max_distance ) // if there is room to move full distance, move.
            {
                transform.Translate( -max_distance, 0.0f, 0.0f );
            }
            else // otherwise, not enough room, arrive at the left edge of the climbable area.
            {
                transform.Translate( climbable_rect.x - player_left, 0.0f, 0.0f );
            }
        }

        // TODO: if fallthrough && input_manager.VerticalAxis > 0.5f
        // Corner
        if ( is_at_left || is_at_right && Mathf.Abs( input_manager.HorizontalAxis ) > 0.0f )
        {
            if ( ! GoFromCeilingToWallAbove() ) 
            {
                GoFromCeilingToWallBelow();
            }
        }
    }

    /// <summary>
    /// Stops climbing the ceiling and moves to standing or crouching, if possible.
    /// </summary>
    private void StopClimbingCeiling()
    {
        // Check if changing the player's hitbox to standing would put them inside a wall.
        float left, right, top, bottom;
        left   = transform.position.x + char_stats.STANDING_COLLIDER_OFFSET.x - char_stats.STANDING_COLLIDER_SIZE.x / 2.0f;
        right  = transform.position.x + char_stats.STANDING_COLLIDER_OFFSET.x + char_stats.STANDING_COLLIDER_SIZE.x / 2.0f; // left + size.x
        top    = transform.position.y + char_stats.STANDING_COLLIDER_OFFSET.y + char_stats.STANDING_COLLIDER_SIZE.y / 2.0f;
        bottom = transform.position.y + char_stats.STANDING_COLLIDER_OFFSET.y - char_stats.STANDING_COLLIDER_SIZE.y / 2.0f; // top - size.y
        if ( Utils.AreaContainsBlockingGeometry( left, top, right - left, top - bottom ) ) // Can't stand. Check if can crouch.
        {
            // Check if changing the player's hitbox to crouching (+ moving them up) would put them inside a wall.
            //   The ceiling climbing collider is short, and offset so the top of it lines up with the top of the "standard" collider for the player. 
            //   It is also extra wide, but centered, so that's not important.
            //   The crouching collider is short, and offset so the bottom of it lines up with the bottom of the "standard" collider for the player.
            //   This makes transitioning from the standard collider to these simple, 
            //   but here I want to transition between the ceiling and crouching colliders due to not having enough space to stand up, so I need to move the player up.
            //   Since crouching collider height <= ceiling climb height, align the bottom of the crouch collider with the bottom of the ceiling climbing collider.
            #if UNITY_EDITOR
            Debug.Assert( char_stats.CROUCHING_COLLIDER_SIZE.y < char_stats.CEILING_CLIMB_COLLIDER_SIZE.y,
                "Assumptions about the relative sizes of the crouching and ceiling climbing colliders were violated. This will probably cause unexpected behaviour (phasing through walls). FIX THIS." );
            #endif
            // bottom = bottom
            float offset = char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y - char_stats.CROUCHING_COLLIDER_OFFSET.y - (char_stats.CEILING_CLIMB_COLLIDER_SIZE.y - char_stats.CROUCHING_COLLIDER_SIZE.y ) / 2.0f;

            left   = transform.position.x + char_stats.CROUCHING_COLLIDER_OFFSET.x - char_stats.CROUCHING_COLLIDER_SIZE.x / 2.0f;
            right  = transform.position.x + char_stats.CROUCHING_COLLIDER_OFFSET.x + char_stats.CROUCHING_COLLIDER_SIZE.x / 2.0f; // left + size.x
            top    = transform.position.y + char_stats.CROUCHING_COLLIDER_OFFSET.y + char_stats.CROUCHING_COLLIDER_SIZE.y / 2.0f + offset;
            bottom = transform.position.y + char_stats.CROUCHING_COLLIDER_OFFSET.y - char_stats.CROUCHING_COLLIDER_SIZE.y / 2.0f + offset; // top - size.y
            if ( Utils.AreaContainsBlockingGeometry( left, top, right - left, top - bottom ) ) // Can't crouch. Cancel the stop.
            {
                return;
            }
            // TODO: crouch state
            transform.Translate( 0.0f, offset, 0.0f );
            char_stats.CrouchingHitBox();
            StopClimbing();
            return;
        }

        // Stand.
        char_stats.StandingHitBox();
        StopClimbing();
    }

    #region geometry testing
    /// <summary>
    /// Gets a rect representing which passed in colliders are contiguous climbable geometry from the player's current position and hitbox dimensions.
    /// </summary>
    /// <param name="colliders">An array of colliders, some or all of which may be climbable geometry.</param>
    /// <returns>A rect representing the contiguous climbable zone. Note: the behaviour of the rect's x and width are undefined.</returns>
    private Rect GetClimbableWallRect( Collider2D[] colliders )
    {
        float left   = char_stats.char_collider.bounds.min.x;
        float right  = char_stats.char_collider.bounds.max.x;
        float top    = char_stats.char_collider.bounds.max.y;
        float bottom = char_stats.char_collider.bounds.min.y;
        CharEnums.FacingDirection facing = char_stats.facing_direction;
        return GetClimbableWallRect( colliders, left, right, top, bottom, facing );
    }

    /// <summary>
    /// Gets a rect representing which passed in colliders are contiguous climbable geometry from an arbitrary position and hitbox dimensions.
    /// </summary>
    /// <param name="colliders">An array of colliders, some or all of which may be climbable geometry.</param>
    /// <param name="player_left">X coordinate of the left edge of the player's hitbox.</param>
    /// <param name="player_right">X coordinate of the right edge of the player's hitbox.</param>
    /// <param name="player_top">Y coordinate of the top edge of the player's hitbox.</param>
    /// <param name="player_bottom">Y coordinate of the bottom edge of the player's hitbox.</param>
    /// <param name="facing">Which direction the player is facing. (They must be facing toward a wall to climb it).</param>
    /// <returns>A rect representing the contiguous climbable zone. Note: the behaviour of the rect's x and width are undefined.</returns>
    private Rect GetClimbableWallRect( Collider2D[] colliders, float player_left, float player_right, float player_top, float player_bottom, CharEnums.FacingDirection facing )
    {
        // need to figure out what's up with the colliders.
        // if on correct side / coords & player collider y within bounds of all ys, ok.
        // if on wrong side / coords, respect as new min/max (blocking), unless going up and fallthrough / non-blocking.
        List<Rect> climbable_zone = new List<Rect>();

        // Build the climbable bounds.
        foreach ( Collider2D collider in colliders )
        {
            CustomTileData tile_data = Utils.GetCustomTileData( collider );
            if ( tile_data == null ) { continue; }
            CollisionType collision_type = tile_data.collision_type;

            // Climbable?
            if ( ! collision_type.IsWallClimbable ) { continue; }

            // Is it ACTUALLY climbable?
            if ( ! ( facing == CharEnums.FacingDirection.Left  && collider.bounds.max.x < player_left ||
                     facing == CharEnums.FacingDirection.Right && collider.bounds.min.x > player_right ) )
            { continue; }
            // TODO: refine checks to check if within allowable range as well?

            // If it is really climbable, we set up the "climbable zone" to include it.
            // x,y is top left. x+size x is right, y-size y is down.
            AddAndConsolidateEdge( ref climbable_zone, new Rect( collider.bounds.min.x, collider.bounds.max.y, collider.bounds.size.x, collider.bounds.size.y ), false );
        }

        // Reduce the climbable bounds.
        foreach ( Collider2D collider in colliders )
        {
            CustomTileData tile_data = Utils.GetCustomTileData( collider );
            if ( tile_data == null ) { continue; }
            CollisionType collision_type = tile_data.collision_type;

            if ( collision_type.IsWallClimbable &&
                ( facing == CharEnums.FacingDirection.Left  && collider.bounds.max.x < player_left ||
                  facing == CharEnums.FacingDirection.Right && collider.bounds.min.x > player_right ) )
            {
                //TODO: refine checks to check if within allowable range as well?
                // This collider is a climbable wall you (which you are probably climbing). Do not remove it from the climbable zone, ignore it.
                // There can be a block of climbable wall jutting out from another climbable wall, so that's why we need the extra positional checks before it's safe to ignore.
                // X
                // XX
                // X
                continue;
            }
            else if ( collision_type.CanFallthrough )
            {
                // Moving down a wall onto a fallthrough platform should make you get off the wall and stand on the platform.
                // If platform's top is below player's bottom, then treat this collider as blocking, subtract it from the climbable zone.
                if ( collider.bounds.max.y <= player_bottom )
                {
                    RemoveEdge( ref climbable_zone, new Rect( collider.bounds.min.x, collider.bounds.max.y, collider.bounds.size.x, collider.bounds.size.y ), false );
                }
                // Not blocking: ignore. (You may climb a wall up through a fallthrough platform).
                continue;
            }
            else
            {
                // This collider is a blocking collider. Subtract it from the climbable zone.
                // TODO: differentiate enemies, etc?
                RemoveEdge( ref climbable_zone, new Rect( collider.bounds.min.x, collider.bounds.max.y, collider.bounds.size.x, collider.bounds.size.y ), false );
            }
        }

        if ( climbable_zone.Count == 0 ) { return new Rect( 0.0f, 0.0f, 0.0f, 0.0f ); } // no climbable surface

        // How do we parse this climbable zone, which may contain numerous climbable subzones and intervening gaps? Arbitrarily:
        // Find the contiguous region that overlaps player head-level. It must also at least go down to the player's feet.
        // If one doesn't exist, stop climbing.
        Rect climbable_rect = new Rect( 0.0f, 0.0f, 0.0f, 0.0f );
        foreach ( Rect rect in climbable_zone )
        {
            if ( rect.y >= player_top && rect.y - rect.size.y <= player_top ) // overlaps head
            {
                if ( /*rect.y >= player_bottom &&*/ rect.y - rect.size.y <= player_bottom ) // extends to or beyond feet.
                {
                    climbable_rect = rect;
                }
            }
        }
        if ( climbable_rect.size.y == 0 ) { return new Rect( 0.0f, 0.0f, 0.0f, 0.0f ); } // no climbable surface

        return climbable_rect;
    }

    /// <summary>
    /// Gets a rect representing which passed in colliders are contiguous climbable geometry from the player's current position and hitbox dimensions.
    /// </summary>
    /// <param name="colliders">An array of colliders, some or all of which may be climbable geometry.</param>
    /// <returns>A rect representing the contiguous climbable zone. Note: the behaviour of the rect's y and height are undefined.</returns>
    private Rect GetClimbableCeilingRect( Collider2D[] colliders )
    {
        float left   = char_stats.char_collider.bounds.min.x;
        float right  = char_stats.char_collider.bounds.max.x;
        float top    = char_stats.char_collider.bounds.max.y;
        float bottom = char_stats.char_collider.bounds.min.y;
        return GetClimbableCeilingRect( colliders, left, right, top, bottom );
    }

    /// <summary>
    /// Gets a rect representing which passed in colliders are contiguous climbable geometry from an arbitrary position and hitbox dimensions.
    /// </summary>
    /// <param name="colliders">An array of colliders, some or all of which may be climbable geometry</param>
    /// <param name="player_left">X coordinate of the left edge of the player's hitbox.</param>
    /// <param name="player_right">X coordinate of the right edge of the player's hitbox.</param>
    /// <param name="player_top">Y coordinate of the top edge of the player's hitbox.</param>
    /// <param name="player_bottom">Y coordinate of the bottom edge of the player's hitbox.</param>
    /// <returns>A rect representing the contiguous climbable zone. Note: the behaviour of the rect's y and height are undefined.</returns>
    private Rect GetClimbableCeilingRect( Collider2D[] colliders, float player_left, float player_right, float player_top, float player_bottom )
    {
        // need to figure out what's up with the colliders.
        // if on correct side / coords & player collider x within bounds of all xs, ok.
        // if on wrong side / coords, respect as new min/max (blocking), unless non-blocking.
        List<Rect> climbable_zone = new List<Rect>();

        // Build the climbable bounds.
        foreach ( Collider2D collider in colliders )
        {
            CustomTileData tile_data = Utils.GetCustomTileData( collider );
            if ( tile_data == null ) { continue; }
            CollisionType collision_type = tile_data.collision_type;

            // Climbable?
            if ( ! collision_type.IsCeilingClimbable ) { continue; }

            // Is it ACTUALLY climbable? ( ceiling bottom must be above player's top, and no further than 1 px away. )
            //if ( ! ( collider.bounds.min.y > player_top ) ) { continue; }
            float bottom_to_top = collider.bounds.min.y - player_top;
            if ( bottom_to_top < 0.0f || bottom_to_top > 1.0f ) { continue; }

            // If it is really climbable, we set up the "climbable zone" to include it.
            // x,y is top left. x+size x is right, y-size y is down.
            AddAndConsolidateEdge( ref climbable_zone, new Rect( collider.bounds.min.x, collider.bounds.max.y, collider.bounds.size.x, collider.bounds.size.y ), true );
        }

        // Reduce the climbable bounds.
        foreach ( Collider2D collider in colliders )
        {
            CustomTileData tile_data = Utils.GetCustomTileData( collider );
            if ( tile_data == null ) { continue; }
            CollisionType collision_type = tile_data.collision_type;

            float bottom_to_top = collider.bounds.min.y - player_top;
            if ( collision_type.IsCeilingClimbable && bottom_to_top >= 0.0f && bottom_to_top <= 1.0f )
            {
                // This collider is a climbable ceiling (which you are almost certainly climbing). Do not remove it from the climbable zone, ignore it.
                continue;
            }
            else
            {
                // This collider is a blocking collider. Subtract it from the climbable zone.
                // TODO: differentiate enemies, etc?
                RemoveEdge( ref climbable_zone, new Rect( collider.bounds.min.x, collider.bounds.max.y, collider.bounds.size.x, collider.bounds.size.y ), true );
            }
        }

        if ( climbable_zone.Count == 0 ) { return new Rect( 0.0f, 0.0f, 0.0f, 0.0f ); } // no climbable surface

        // How do we parse this climbable zone, which may contain numerous climbable subzones and intervening gaps? Arbitrarily:
        // Find the contiguous region that contains the player collider.
        // If one doesn't exist, stop climbing.
        Rect climbable_rect = new Rect( 0.0f, 0.0f, 0.0f, 0.0f );
        foreach ( Rect rect in climbable_zone )
        {
            if ( rect.x <= player_left && rect.x + rect.size.x >= player_left ) // contains left
            {
                if ( /*rect.x <= player_right &&*/ rect.x + rect.size.x >= player_right ) // contains right
                {
                    climbable_rect = rect;
                }
            }
        }
        if ( climbable_rect.size.x == 0 ) { return new Rect( 0.0f, 0.0f, 0.0f, 0.0f ); }

        return climbable_rect;
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

    #region climbing corners
    #region checks
    /// <summary>
    /// Checks if the player can move from climbing under the corner of the ceiling to climbing the wall above the ceiling which makes up another face of the corner.
    /// </summary>
    /// <returns>True if the player can move onto the wall.</returns>
    private bool CanGoFromCeilingToWallAbove()
    {
        if ( ! player_stats.acquired_mag_grip ) { return false; }

        // check if you are at the corner of a wall and a ceiling.
        // [?][X][ ]   [ ][X][?]
        // [X][X][ ]   [ ][X][X]
        // [P][P][ ]   [ ][P][P]

        // Are you at the edge of the ceiling?
        if ( ! is_at_left && ! is_at_right ) { return false; }

        float x, y, width, height;
        float new_left, new_right, new_top, new_bottom;
        CharEnums.FacingDirection facing;
        Collider2D[] colliders;

        if ( is_at_left )
        {
            // Check that there is enough contiguous climbable wall above you.
            float left = transform.position.x - char_stats.CEILING_CLIMB_COLLIDER_SIZE.x / 2.0f + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x;
            float top  = transform.position.y + char_stats.CEILING_CLIMB_COLLIDER_SIZE.y / 2.0f + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y;
            width = 1.0f;
            height = char_stats.STANDING_COLLIDER_SIZE.y;
            x = left;
            y = top + height;
            colliders = Physics2D.OverlapAreaAll( new Vector2( x, y ), new Vector2( x + width, y - height ), CollisionMasks.static_mask );

            // Check that there is enough empty space left of the wall to fit the new hitbox.
            width = char_stats.STANDING_COLLIDER_SIZE.x;
            x = left - width - 0.5f;
            height = char_stats.STANDING_COLLIDER_SIZE.y + char_stats.CEILING_CLIMB_COLLIDER_SIZE.y; // extended so you can't phase through diagonally touching walls

            // Calculate the future hitbox location + facing, for climbable zone calcs.
            new_left = x;
            new_right = x + width; // Margin required.
            new_top = y + 1.0f;
            new_bottom = new_top - char_stats.STANDING_COLLIDER_SIZE.y;
            facing = CharEnums.FacingDirection.Right;
        }
        else //if ( is_at_right )
        {
            // Check that there is enough contiguous climbable wall above you.
            float right = transform.position.x + char_stats.CEILING_CLIMB_COLLIDER_SIZE.x / 2.0f + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x;
            float top   = transform.position.y + char_stats.CEILING_CLIMB_COLLIDER_SIZE.y / 2.0f + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y;
            width = 1.0f;
            height = char_stats.STANDING_COLLIDER_SIZE.y;
            x = right - width;
            y = top + height;
            colliders = Physics2D.OverlapAreaAll( new Vector2( x, y ), new Vector2( x + width, y - height ), CollisionMasks.static_mask );

            // Check that there is enough empty space right of the wall to fit the new hitbox.
            width = char_stats.STANDING_COLLIDER_SIZE.x;
            x = right + 0.5f;
            height = char_stats.STANDING_COLLIDER_SIZE.y + char_stats.CEILING_CLIMB_COLLIDER_SIZE.y; // extended so you can't phase through diagonally touching walls

            // Calculate the future hitbox location + facing, for climbable zone calcs.
            new_left = x; // Margin required.
            new_right = x + width;
            new_top = y + 1.0f;
            new_bottom = new_top - char_stats.STANDING_COLLIDER_SIZE.y;
            facing = CharEnums.FacingDirection.Left;
        }

        // Enough wall?
        Rect climbable_area = GetClimbableWallRect( colliders, new_left, new_right, new_top, new_bottom, facing );
        if ( climbable_area.height < char_stats.STANDING_COLLIDER_SIZE.y ) { return false; }
        // Enough empty space?
        if ( Utils.AreaContainsBlockingGeometry( x, y, width, height ) ) { return false; }

        return true;
    }

    /// <summary>
    /// Checks if the player can go from climbing under the corner of the ceiling to climbing the wall below which makes up another face of the corner.
    /// </summary>
    /// <returns>True if the player can move onto the wall.</returns>
    private bool CanGoFromCeilingToWallBelow()
    {
        if ( ! player_stats.acquired_mag_grip ) { return false; }

        // check if you are at the corner of a wall and a ceiling.
        // [X][X][X]   [X][X][X]
        // [X][P][P]   [P][P][X]
        // [X][ ][?]   [?][ ][X]

        // Are you at the edge of the ceiling?
        if ( ! is_at_left && ! is_at_right ) { return false; }

        float x, y, width, height;
        float new_left, new_right, new_top, new_bottom;
        CharEnums.FacingDirection facing;
        Collider2D[] colliders;

        if ( is_at_left )
        {
            // Check that wall (and enough of it) exists below you.
            float left = transform.position.x - char_stats.CEILING_CLIMB_COLLIDER_SIZE.x / 2.0f + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x;
            x = left - 1.0f;
            y = transform.position.y + char_stats.CEILING_CLIMB_COLLIDER_SIZE.y / 2.0f + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y;
            width  = 1.0f;
            height = char_stats.STANDING_COLLIDER_SIZE.y;
            // left-1 to left, top to top-standing height
            colliders = Physics2D.OverlapAreaAll( new Vector2( x, y ), new Vector2( x + width, y - height ), CollisionMasks.static_mask );

            // Check that there is enough empty space right of the wall to fit the new hitbox.
            x = left + 0.5f;
            width = char_stats.STANDING_COLLIDER_SIZE.x;

            new_left   = left + 0.5f; // (0,1) px margin required for GetClimbableWallRect.
            new_right  = new_left + char_stats.STANDING_COLLIDER_SIZE.x;
            new_top    = y;
            new_bottom = y - char_stats.STANDING_COLLIDER_SIZE.y;
            facing = CharEnums.FacingDirection.Left;
        }
        else //if (is_at_right)
        {
            // Check that wall (and enough of it) exists below you.
            float right = transform.position.x + char_stats.CEILING_CLIMB_COLLIDER_SIZE.x / 2.0f + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x;
            x = right;
            y = transform.position.y + char_stats.CEILING_CLIMB_COLLIDER_SIZE.y / 2.0f + char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y;
            width  = 1.0f;
            height = char_stats.STANDING_COLLIDER_SIZE.y;
            // right to right+1, top to top-standing height
            colliders = Physics2D.OverlapAreaAll( new Vector2( x, y ), new Vector2( x + width, y - height ), CollisionMasks.static_mask );

            // Check that there is enough empty space left of the wall to fit the new hitbox.
            x = right - char_stats.STANDING_COLLIDER_SIZE.x - 0.5f;
            width = char_stats.STANDING_COLLIDER_SIZE.x;

            new_right  = right - 0.5f; // (0,1) px margin required for GetClimbableWallRect.
            new_left   = new_right - char_stats.STANDING_COLLIDER_SIZE.x;
            new_top    = y;
            new_bottom = y - char_stats.STANDING_COLLIDER_SIZE.y;
            facing = CharEnums.FacingDirection.Right;
        }

        // Enough wall?
        Rect climbable_area = GetClimbableWallRect( colliders, new_left, new_right, new_top, new_bottom, facing );
        if ( climbable_area.height < char_stats.STANDING_COLLIDER_SIZE.y ) { return false; }
        // Enough empty space?
        if ( Utils.AreaContainsBlockingGeometry( x, y, width, height ) ) { return false; }

        return true;
    }

    /// <summary>
    /// Checks if the player can move from climbing on the left side of a wall to the ceiling directly above them, and expand a little to the left.
    /// </summary>
    /// <returns>True if the player can move onto the ceiling.</returns>
    private bool CanGoFromWallToCeilingAboveLeft()
    {
        // check if you are at the corner of a wall and a ceiling.
        // [X][X][X]
        // [ ][P][X]
        // [?][P][X]

        if ( ! player_stats.acquired_ceiling_grip ) { return false; }
        // Are you at the top edge of the wall?
        if ( ! is_at_top ) { return false; }

        // Check that ceiling (and enough of it) exists above you.
        float x, y, width, height;
        float new_left, new_right, new_top, new_bottom;

        // Align the new hitbox with the old hitbox's right and top.
        float top   = transform.position.y + char_stats.STANDING_COLLIDER_SIZE.y / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.y;
        float right = transform.position.x + char_stats.STANDING_COLLIDER_SIZE.x / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.x;
        x = right - char_stats.CEILING_CLIMB_COLLIDER_SIZE.x - char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x;
        y = top + 1.0f; // 1 pixel above top
        width  = char_stats.CEILING_CLIMB_COLLIDER_SIZE.x;
        height = 1.0f;

        new_right  = right;
        new_top    = top - 0.5f; // margin required
        new_left   = new_right - char_stats.CEILING_CLIMB_COLLIDER_SIZE.x;
        new_bottom = new_top   - char_stats.CEILING_CLIMB_COLLIDER_SIZE.y;

        Collider2D[] colliders = Physics2D.OverlapAreaAll( new Vector2( x, y ), new Vector2( x + width, y - height ), CollisionMasks.static_mask );
        Rect climbable_area = GetClimbableCeilingRect( colliders, new_left, new_right, new_top, new_bottom );
        if ( climbable_area.width < char_stats.CEILING_CLIMB_COLLIDER_SIZE.x ) { return false; }

        // Check that there is enough space below the ceiling to fit the new hitbox.
        y = top - 0.5f; // margin required
        height = char_stats.CEILING_CLIMB_COLLIDER_SIZE.y;
        if ( Utils.AreaContainsBlockingGeometry( x, y, width, height ) ) { return false; }

        return true;
    }

    /// <summary>
    /// Checks if the player can move from climbing on the right side of a wall to the ceiling directly above them, and expand a little to the right.
    /// </summary>
    /// <returns>True if the player can move onto the ceiling.</returns>
    private bool CanGoFromWallToCeilingAboveRight()
    {
        if ( ! player_stats.acquired_ceiling_grip ) { return false; }

        // check if you are at the corner of a wall and a ceiling.
        // [X][X][X]
        // [X][P][ ]
        // [X][P][?]

        // Are you at the top edge of the wall?
        if ( ! is_at_top ) { return false; }

        // Check that ceiling (and enough of it) exists above you.
        float x, y, width, height;
        float new_left, new_right, new_top, new_bottom;

        // Align the new hitbox with the old hitbox's top and left.
        float top  = transform.position.y + char_stats.STANDING_COLLIDER_SIZE.y / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.y;
        float left = transform.position.x - char_stats.STANDING_COLLIDER_SIZE.x / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.x;
        x = left;
        y = top + 1.0f; // 1 pixel above top.
        width  = char_stats.CEILING_CLIMB_COLLIDER_SIZE.x;
        height = 1.0f;

        new_left   = left;
        new_top    = top - 0.5f; // margin required
        new_right  = new_left + char_stats.CEILING_CLIMB_COLLIDER_SIZE.x;
        new_bottom = new_top  - char_stats.CEILING_CLIMB_COLLIDER_SIZE.y;

        Collider2D[] colliders = Physics2D.OverlapAreaAll( new Vector2( x, y ), new Vector2( x + width, y - height ), CollisionMasks.static_mask );
        Rect climbable_area = GetClimbableCeilingRect( colliders, new_left, new_right, new_top, new_bottom );
        if ( climbable_area.width < char_stats.CEILING_CLIMB_COLLIDER_SIZE.x ) { return false; }

        // Check that there is enough space below the ceiling to fit the new hitbox.
        y = top - 0.5f; // margin required
        height = char_stats.CEILING_CLIMB_COLLIDER_SIZE.y;
        if ( Utils.AreaContainsBlockingGeometry( x, y, width, height ) ) { return false; }

        return true;
    }

    /// <summary>
    /// Checks if the player can move from climbing on the right side of a wall to the underside of the wall.
    /// </summary>
    /// <returns>True if the player can move onto the ceiling.</returns>
    private bool CanGoFromWallToCeilingBelowLeft()
    {
        if ( ! player_stats.acquired_ceiling_grip ) { return false; }

        // check if you are at the corner of a wall and a ceiling.
        // [?][X][P]
        // [X][X][P]
        // [ ][ ][ ]

        if ( ! is_at_bottom ) { return false; }

        // Check that ceiling (and enough of it) exists below you.
        float x, y, width, height;
        float new_left, new_right, new_top, new_bottom;

        float left   = transform.position.x - char_stats.STANDING_COLLIDER_SIZE.x / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.x;
        float bottom = transform.position.y - char_stats.STANDING_COLLIDER_SIZE.y / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.y;
        x = left - char_stats.CEILING_CLIMB_COLLIDER_SIZE.x - 1.0f; // left
        y = bottom + 1.0f; // 1 pixel above bottom
        width  = char_stats.CEILING_CLIMB_COLLIDER_SIZE.x;
        height = 1.0f;

        new_left   = x;
        new_top    = bottom - 0.5f;
        new_right  = new_left + char_stats.CEILING_CLIMB_COLLIDER_SIZE.x;
        new_bottom = new_top  - char_stats.CEILING_CLIMB_COLLIDER_SIZE.y;

        Collider2D[] colliders = Physics2D.OverlapAreaAll( new Vector2( x, y ), new Vector2( x + width, y - height ), CollisionMasks.static_mask );
        Rect climbable_area = GetClimbableCeilingRect( colliders, new_left, new_right, new_top, new_bottom );
        if ( climbable_area.width < char_stats.CEILING_CLIMB_COLLIDER_SIZE.x ) { return false; }

        // Check that there is enough empty space below the ceiling (+ below you) to fit the new hitbox
        y = bottom - 0.5f;
        height = char_stats.CEILING_CLIMB_COLLIDER_SIZE.y;
        width  = char_stats.CEILING_CLIMB_COLLIDER_SIZE.x + char_stats.STANDING_COLLIDER_SIZE.x; // extra width so you can't phase through diagonally touching walls
        if ( Utils.AreaContainsBlockingGeometry( x, y, width, height ) ) { return false; }

        return true;
    }

    /// <summary>
    /// Checks if the player can move from climbing on the left side of a wall to the underside of the wall.
    /// </summary>
    /// <returns>True if the player can move onto the ceiling.</returns>
    private bool CanGoFromWallToCeilingBelowRight()
    {
        if ( ! player_stats.acquired_ceiling_grip ) { return false; }

        // check if you are at the corner of a wall and a ceiling.
        // [P][X][?]
        // [P][X][X]
        // [ ][ ][ ]

        if ( ! is_at_bottom ) { return false; }

        // Check that ceiling (and enough of it) exists below you.
        float x, y, width, height;
        float new_left, new_right, new_top, new_bottom;

        float right = transform.position.x + char_stats.STANDING_COLLIDER_SIZE.x / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.x;
        float bottom = transform.position.y - char_stats.STANDING_COLLIDER_SIZE.y / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.y;
        x = right + 1.0f;
        y = bottom + 1.0f; // 1 pixel above bottom
        width  = char_stats.CEILING_CLIMB_COLLIDER_SIZE.x;
        height = 1.0f;

        new_left   = right + 1.0f;
        new_top    = bottom - 0.5f;
        new_right  = new_left + char_stats.CEILING_CLIMB_COLLIDER_SIZE.x;
        new_bottom = new_top  - char_stats.CEILING_CLIMB_COLLIDER_SIZE.y;

        Collider2D[] colliders = Physics2D.OverlapAreaAll( new Vector2( x, y ), new Vector2( x + width, y - height ), CollisionMasks.static_mask );
        Rect climbable_area = GetClimbableCeilingRect( colliders, new_left, new_right, new_top, new_bottom );
        if ( climbable_area.width < char_stats.CEILING_CLIMB_COLLIDER_SIZE.x ) { return false; }

        // Check that there is enough empty space below the ceiling (+ below you) to fit the new hitbox
        x = transform.position.x - char_stats.STANDING_COLLIDER_SIZE.x / 2.0f + char_stats.STANDING_COLLIDER_OFFSET.x; // left
        y = bottom - 0.5f;
        height = char_stats.CEILING_CLIMB_COLLIDER_SIZE.y;
        width  = char_stats.CEILING_CLIMB_COLLIDER_SIZE.x + char_stats.STANDING_COLLIDER_SIZE.x + 1.0f; // extra width so you can't phase through diagonally touching walls
        if ( Utils.AreaContainsBlockingGeometry( x, y, width, height ) ) { return false; }

        return true;
    }
    #endregion

    #region transitions
     /// <summary>
    /// If possible, move from climbing under the corner of a ceiling to climbing on the wall around the corner.
    /// </summary>
    /// <returns>True if the player was moved.</returns>
    private bool GoFromCeilingToWallAbove()
    {
        if ( ! CanGoFromCeilingToWallAbove() ) { return false; }
        
        float delta_x = 0.0f;
        float delta_y = 0.0f;
        CharEnums.FacingDirection facing = char_stats.facing_direction;

        if ( is_at_left )
        {
            // Align the new hitbox with the old hitbox's left and top.
            float offset_x = ( char_stats.CEILING_CLIMB_COLLIDER_SIZE.x - char_stats.STANDING_COLLIDER_SIZE.x ) / -2.0f + ( char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x - char_stats.STANDING_COLLIDER_OFFSET.x );
            float offset_y = ( char_stats.CEILING_CLIMB_COLLIDER_SIZE.y - char_stats.STANDING_COLLIDER_SIZE.y ) / 2.0f + ( char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y - char_stats.STANDING_COLLIDER_OFFSET.y );
            // Once aligned, move the player out one player width, and up one player height, plus padding margin.
            delta_x = -1.0f * char_stats.STANDING_COLLIDER_SIZE.x + offset_x - 0.5f;
            delta_y = char_stats.STANDING_COLLIDER_SIZE.y + offset_y + 1.0f;
            facing = CharEnums.FacingDirection.Right;
        }
        else if ( is_at_right )
        {
            // Align the new hitbox with the old hitbox's right and top.
            float offset_x = ( char_stats.CEILING_CLIMB_COLLIDER_SIZE.x - char_stats.STANDING_COLLIDER_SIZE.x ) / 2.0f + ( char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x - char_stats.STANDING_COLLIDER_OFFSET.x );
            float offset_y = ( char_stats.CEILING_CLIMB_COLLIDER_SIZE.y - char_stats.STANDING_COLLIDER_SIZE.y ) / 2.0f + ( char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y - char_stats.STANDING_COLLIDER_OFFSET.y );
            // Once aligned, move the player out one player width, and up one player height, plus padding margin.
            delta_x = char_stats.STANDING_COLLIDER_SIZE.x + offset_x + 0.5f;
            delta_y = char_stats.STANDING_COLLIDER_SIZE.y + offset_y + 1.0f;
            facing = CharEnums.FacingDirection.Left;
        }
        ChangeClimbingState( new Vector2( delta_x, delta_y ), ClimbState.WallClimb, facing );
        return true;
    }

    /// <summary>
    /// If possible, move from climbing the ceiling to climbing a wall under that ceiling.
    /// </summary>
    private void GoFromCeilingToWallBelow()
    {
        if ( ! CanGoFromCeilingToWallBelow() ) { return; }

        float delta_x = 0.0f;
        float delta_y = 0.0f;
        CharEnums.FacingDirection facing = char_stats.facing_direction;

        if ( is_at_left )
        {
            // Align the new hitbox with the old hitbox's left and top.
            delta_x = (char_stats.CEILING_CLIMB_COLLIDER_SIZE.x - char_stats.STANDING_COLLIDER_SIZE.x) / -2.0f + (char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x - char_stats.STANDING_COLLIDER_OFFSET.x) + 0.5f;
            delta_y = (char_stats.CEILING_CLIMB_COLLIDER_SIZE.y - char_stats.STANDING_COLLIDER_SIZE.y) / 2.0f + (char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y - char_stats.STANDING_COLLIDER_OFFSET.y);
            facing = CharEnums.FacingDirection.Left;
        }
        else if ( is_at_right )
        {
            // Align the new hitbox with the old hitbox's right and top.
            delta_x = (char_stats.CEILING_CLIMB_COLLIDER_SIZE.x - char_stats.STANDING_COLLIDER_SIZE.x) / 2.0f + (char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x - char_stats.STANDING_COLLIDER_OFFSET.x) - 0.5f;
            delta_y = (char_stats.CEILING_CLIMB_COLLIDER_SIZE.y - char_stats.STANDING_COLLIDER_SIZE.y) / 2.0f + (char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y - char_stats.STANDING_COLLIDER_OFFSET.y);
            facing = CharEnums.FacingDirection.Right;
        }
        ChangeClimbingState( new Vector2( delta_x, delta_y ), ClimbState.WallClimb, facing );
    }

    /// <summary>
    /// If possible, move from climbing a wall to climbing the ceiling directly above you.
    /// </summary>
    private void GoFromWallToCeilingAbove()
    {
        bool can_go_left  = CanGoFromWallToCeilingAboveLeft();
        bool can_go_right = CanGoFromWallToCeilingAboveRight();

        if ( ! can_go_left && ! can_go_right ) { return; }

        float delta_x = 0.0f;
        float delta_y = 0.0f;
        CharEnums.FacingDirection facing = char_stats.facing_direction;

        if ( can_go_right )
        {
            // Align the new hitbox with the old hitbox's left and top.
            delta_x = ( char_stats.STANDING_COLLIDER_SIZE.x - char_stats.CEILING_CLIMB_COLLIDER_SIZE.x ) / -2.0f + ( char_stats.STANDING_COLLIDER_OFFSET.x - char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x );
            delta_y = ( char_stats.STANDING_COLLIDER_SIZE.y - char_stats.CEILING_CLIMB_COLLIDER_SIZE.y ) / 2.0f + ( char_stats.STANDING_COLLIDER_OFFSET.y - char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y ) - 0.5f;
        }
        else if ( can_go_left )
        {
            // Align the new hitbox with the old hitbox's right and top.
            delta_x = ( char_stats.STANDING_COLLIDER_SIZE.x - char_stats.CEILING_CLIMB_COLLIDER_SIZE.x ) / 2.0f + ( char_stats.STANDING_COLLIDER_OFFSET.x - char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x );
            delta_y = ( char_stats.STANDING_COLLIDER_SIZE.y - char_stats.CEILING_CLIMB_COLLIDER_SIZE.y ) / 2.0f + ( char_stats.STANDING_COLLIDER_OFFSET.y - char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y ) - 0.5f;
        }
        ChangeClimbingState( new Vector2( delta_x, delta_y ), ClimbState.CeilingClimb, facing );
    }

    /// <summary>
    /// If possible, move from climbing a wall to climbing the underside of the wall.
    /// </summary>
    private void GoFromWallToCeilingBelow()
    { 
        bool can_go_left  = CanGoFromWallToCeilingBelowLeft();
        bool can_go_right = CanGoFromWallToCeilingBelowRight();

        if ( ! can_go_left && ! can_go_right ) { return; }

        float delta_x = 0.0f;
        float delta_y = 0.0f;
        CharEnums.FacingDirection facing = char_stats.facing_direction;

        if ( can_go_left )
        {
            // Align the new hitbox with the old hitbox's right and top.
            float offset_x = (char_stats.STANDING_COLLIDER_SIZE.x - char_stats.CEILING_CLIMB_COLLIDER_SIZE.x) / 2.0f + ( char_stats.STANDING_COLLIDER_OFFSET.x - char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x );
            float offset_y = (char_stats.STANDING_COLLIDER_SIZE.y - char_stats.CEILING_CLIMB_COLLIDER_SIZE.y) / 2.0f + ( char_stats.STANDING_COLLIDER_OFFSET.y - char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y );
            // Once aligned, push player's left left one standing width and top down one standing height, plus padding margin.
            delta_x = -1.0f * char_stats.STANDING_COLLIDER_SIZE.x + offset_x - 1.0f; // offset slightly increased (from 0.5) as jumping into a wall puts you a tiny bit away
            delta_y = -1.0f * char_stats.STANDING_COLLIDER_SIZE.y + offset_y - 0.5f;
            // TODO: extended offset requires compound movement: direction then up/down. fix that?
        }
        else if ( can_go_right )
        {
            // Align the new hitbox with the old hitbox's left and top.
            float offset_x = (char_stats.STANDING_COLLIDER_SIZE.x - char_stats.CEILING_CLIMB_COLLIDER_SIZE.x) / -2.0f + ( char_stats.STANDING_COLLIDER_OFFSET.x - char_stats.CEILING_CLIMB_COLLIDER_OFFSET.x );
            float offset_y = (char_stats.STANDING_COLLIDER_SIZE.y - char_stats.CEILING_CLIMB_COLLIDER_SIZE.y) / 2.0f + ( char_stats.STANDING_COLLIDER_OFFSET.y - char_stats.CEILING_CLIMB_COLLIDER_OFFSET.y );
            // Once aligned, push player's right right one standing width and top down one standing height, plus padding margin.
            delta_x = char_stats.STANDING_COLLIDER_SIZE.x + offset_x + 1.0f; // offset slightly increased as jumping into a wall puts you a tiny bit away
            delta_y = -1.0f * char_stats.STANDING_COLLIDER_SIZE.y + offset_y - 0.5f;
        }
        ChangeClimbingState( new Vector2( delta_x, delta_y ), ClimbState.CeilingClimb, facing );
    }

    /// <summary>
    /// Moves the player, changes their state and facing, and resizes their hitbox when changing climbing state.
    /// </summary>
    /// <param name="delta">Change to apply to player's position.</param>
    /// <param name="state">The new climb state to set the player to.</param>
    /// <param name="facing">The new facing to set the player to.</param>
    private void ChangeClimbingState( Vector2 delta, ClimbState state, CharEnums.FacingDirection facing )
    {
        transform.Translate( delta.x, delta.y, 0.0f );
        char_stats.facing_direction = facing;
        current_climb_state = state;

        if ( state == ClimbState.WallClimb )
        {
            char_stats.StandingHitBox();
        }
        else if ( state == ClimbState.CeilingClimb )
        {
            char_stats.CeilingClimbHitBox();
        }

        // TODO: add animation stuff
        //char_anims.;
    }
    #endregion
    #endregion

    #region marked for deletion or reimplementation
    /*
    /// <summary>
    /// Gets the player off of the wall.
    /// </summary>
    void WallToGroundStart() // TODO: rename, too vague. don't trust animation with positional changes
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
    public void WallToGroundStop() // TODO: rename, too vague
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
    }

    /// <summary>
    /// 
    /// </summary>
    void GroundToWallStart() // TODO: rename, too vague.
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
    public void GroundToWallStop() // TODO: rename, too vague
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
    public void WallClimbFromLedge() // TODO: reimplement
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
    
     if ( player_stats.acquired_mag_grip )
            {
                // if we want to grab down onto the wall from the ledge
                if ( abuts_facing_sticky_ledge ) // && we're standing on a grabbable surface?
                {
                    if ( ! char_stats.IsInMidair && input_manager.JumpInputInst )
                    {
                        // do we want to climb down?
                        //mag_grip.WallClimbFromLedge();
                    }
                }
            }
     */
    #endregion
}
