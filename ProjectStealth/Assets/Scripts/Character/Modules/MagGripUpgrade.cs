using UnityEngine;
using System.Collections;
using UnityEditor;

// Handles wall climbing
public class MagGripUpgrade : MonoBehaviour
{
    #region vars
    private SpriteRenderer sprite_renderer;
    private IInputManager input_manager;
    private CharacterAnimationLogic char_anims;

    //this allows us to reference player stuff like their movement state
    Player player_script; //AVOID CIRCULAR REFERENCING
    PlayerStats player_stats;
    CharacterStats char_stats;

    private const float WALL_GRAB_DELAY = 0.15f;
    private float wall_grab_delay_timer = 0.15f;

    //Mag Grip variables
    public Collider2D grab_collider; // = null; //private
    public enum ClimbState { NotClimb, WallClimb, CeilingClimb, Transition };
    public ClimbState current_climb_state = ClimbState.NotClimb;

    private const float WALL_CLIMB_SPEED = 90.0f; // pixels / second
    private const float WALL_SLIDE_SPEED = 180.0f; // pixels / second

    //TODO: do something about duplicate code
    // ledge logic
    private bool is_overlooking_ledge;
    private bool is_against_ledge;

    //consts
    protected const float JUMP_ACCELERATION = 240.0f; // base accel for jump off the wall with no input (pixels / second / second)
    #endregion

    // Use this for initialization
    void Start()
    {
        player_script   = GetComponent<Player>();
        sprite_renderer = GetComponentInChildren<SpriteRenderer>();
        player_stats    = GetComponent<PlayerStats>();
        char_stats      = GetComponent<CharacterStats>();
        input_manager   = GetComponent<IInputManager>();
        char_anims      = GetComponent<CharacterAnimationLogic>();

    }

    // Update is called once per frame
    void Update()
    {
        //wall grab delay timer
        if ( wall_grab_delay_timer < WALL_GRAB_DELAY )
        {
            wall_grab_delay_timer = wall_grab_delay_timer + Time.deltaTime * Time.timeScale;
        }
        else
        {
            wall_grab_delay_timer = WALL_GRAB_DELAY;
        }


        if ( char_stats.current_master_state == CharEnums.MasterState.ClimbState )
        {
            if ( current_climb_state == ClimbState.WallClimb )
            {
                ClimbMovementInput();
            }
            else if ( current_climb_state == ClimbState.CeilingClimb )
            {
            }
        }

        if ( current_climb_state == ClimbState.WallClimb )
        {
            ClimbHorizontalVelocity();
            ClimbVerticalVelocity();
            player_script.Collisions();
            ClimbVerticalEdgeDetect();

            //move the character after all calculations have been done
            transform.Translate( char_stats.velocity * Time.deltaTime * Time.timeScale );

            //if you climb down and touch the ground, stop climbing
            if ( char_stats.IsGrounded )
            {
                char_anims.WallSlideTouchGround();
                StopClimbing();
            }
        }
        else if ( current_climb_state == ClimbState.CeilingClimb )
        {

        }
    }

    void ClimbHorizontalVelocity()
    {
        if ( Mathf.Approximately( char_stats.velocity.x - 1000000, -1000000 ) ) //TODO:
        {
            char_stats.velocity.x = 0;
        }
    }

    void ClimbVerticalVelocity()
    {
        if ( input_manager.VerticalAxis > 0.0f )
        {
            char_stats.velocity.y = WALL_CLIMB_SPEED * input_manager.VerticalAxis;
        }
        else if ( input_manager.VerticalAxis < 0.0f )
        {
            char_stats.velocity.y = -WALL_SLIDE_SPEED;
        }
        else
        {
            char_stats.velocity.y = 0.0f;
        }
    }

    /// <summary>
    /// This function detects if the player is touching the ledge when climbing vertically against a wall
    /// </summary>
    void ClimbVerticalEdgeDetect()
    {
        // raycast to find the climbing object
        RaycastHit2D hit = Physics2D.Raycast(char_stats.char_collider.bounds.center, new Vector2(char_stats.GetFacingXComponent(), 0), char_stats.char_collider.bounds.size.x, CollisionMasks.wall_grab_mask);
        if ( hit.collider != null )
        {
            float colliderTop = hit.collider.bounds.max.y;
            float colliderBottom = hit.collider.bounds.min.y;
            float characterTop = char_stats.char_collider.bounds.max.y + 0.5f;
            float characterBottom = char_stats.char_collider.bounds.min.y - 0.5f;

            // stop at the edges of the surface
            float ledgeDistanceTop = colliderTop - characterTop;
            if ( char_stats.velocity.y > 0.0f && ledgeDistanceTop <= Mathf.Abs( char_stats.velocity.y * Time.deltaTime * Time.timeScale ) )
            {
                char_stats.velocity.y = ledgeDistanceTop / ( Time.deltaTime * Time.timeScale );
            }

            float ledgeDistanceBottom = characterBottom - colliderBottom;
            if ( char_stats.velocity.y < 0.0f && ledgeDistanceBottom <= Mathf.Abs( char_stats.velocity.y * Time.deltaTime * Time.timeScale ) )
            {
                char_stats.velocity.y = -ledgeDistanceBottom / ( Time.deltaTime * Time.timeScale );
            }

            // set if you're against the ledge
            if ( ledgeDistanceTop == 0.0f || ledgeDistanceBottom == 0.0f )
            {
                is_against_ledge = true;
            }
            else
            {
                is_against_ledge = false;
            }
        }
        else
        {
            Debug.LogError( "Can't find collider when climbing against a wall. This shouldn't happen" );
        }
    }

    void ClimbMovementInput()
    {
        float characterTop = char_stats.char_collider.bounds.max.y + 0.5f;
        float characterBottom = char_stats.char_collider.bounds.min.y - 0.5f;

        if ( grab_collider )
        {
            LedgeLook();

            // Jump logic.
            if ( !is_overlooking_ledge && input_manager.JumpInputInst )
            {
                StopClimbing();
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
            //ledge climb logic
            else if ( is_overlooking_ledge && input_manager.JumpInputInst )
            {
                // if we're looking below, drop
                if ( grab_collider.bounds.min.y == characterBottom )
                {
                    StopClimbing();
                    char_anims.DropFromWallTrigger();
                }
                // if we're looking above, climb up
                else if ( grab_collider.bounds.max.y == characterTop )
                {
                    WallToGroundStart();
                }
                else
                {
                    Debug.LogError( "The grab_collider object is most likely null. grab_collider.bounds.min.y: " + grab_collider.bounds.min.y );
                }
            }
        }
    }

    void LedgeLook()
    {
        if ( current_climb_state == ClimbState.WallClimb )
        {
            if ( is_against_ledge && ( Mathf.Abs( input_manager.VerticalAxis ) > 0.0f ||
                ( input_manager.HorizontalAxis > 0.0f && char_stats.char_collider.bounds.center.x < grab_collider.bounds.center.x ) ||
                ( input_manager.HorizontalAxis < 0.0f && char_stats.char_collider.bounds.center.x > grab_collider.bounds.center.x ) ) )
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
    /// sets everything that needs to be done when player stops climbing
    /// </summary>
    public void StopClimbing()
    {
        current_climb_state = ClimbState.NotClimb;
        //no grab, no collider
        grab_collider = null;
        //reset the delay before we can wall grab again
        wall_grab_delay_timer = 0.0f;
        //set the master state to the default state. It'll transition into any other state from there. 
        char_stats.current_master_state = CharEnums.MasterState.DefaultState;
    }

    void WallToGroundStart()
    {
        input_manager.JumpInputInst = false;
        input_manager.JumpInput = false;
        input_manager.InputOverride = true;
        // variable sterilazation
        char_stats.is_jumping = false;
        char_stats.is_on_ground = true;


        char_anims.WallToGroundTrigger();
        current_climb_state = ClimbState.Transition;

        char_stats.is_crouching = false;
        char_stats.CrouchingHitBox();
        char_anims.SetCrouch();
    }

    public void WallToGroundStop()
    {
        current_climb_state = ClimbState.NotClimb;
        input_manager.InputOverride = false;
        is_overlooking_ledge = false;
        is_against_ledge = false;


        if ( input_manager.VerticalAxis >= 0.0f )
        {
            char_stats.StandingHitBox();
        }

        char_stats.current_master_state = CharEnums.MasterState.DefaultState;
        char_stats.current_move_state = CharEnums.MoveState.IsSneaking;
        grab_collider = null;
    }

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
        input_manager.InputOverride = true;
        // variable sterilazation
        char_stats.is_jumping = false;
        char_anims.GroundToWallTrigger();
        current_climb_state = ClimbState.Transition;

        char_stats.is_crouching = false;
        char_stats.is_on_ground = false;
        grab_collider = char_stats.on_ground_collider;
    }

    public void GroundToWallStop()
    {
        current_climb_state = ClimbState.WallClimb;
        input_manager.InputOverride = false;
        is_overlooking_ledge = false;
        is_against_ledge = false;

        char_stats.current_master_state = CharEnums.MasterState.ClimbState;
        char_stats.current_move_state = CharEnums.MoveState.IsSneaking;
        char_stats.velocity.x = 0.0f;

        char_stats.ResetJump();
        char_stats.StandingHitBox();
    }

    /// <summary>
    /// Function that initiates a wallclimb from a crouching on the ground against a ledge
    /// </summary>
    public void WallClimbFromLedge()
    {
        // This is kinda inefficient as it is redundant code from the collision detection...
        Vector2 verticalBoxSize = new Vector2(char_stats.char_collider.bounds.size.x - 0.1f, 0.1f);
        Vector2 downHitOrigin = new Vector2(char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.center.y - char_stats.char_collider.bounds.extents.y + 0.1f);
        RaycastHit2D downHit = Physics2D.BoxCast(downHitOrigin, verticalBoxSize, 0.0f, Vector2.down, 25.0f, CollisionMasks.all_collision_mask);
        if ( downHit.collider != null )
        {
            //check the ledge to see if it's tall enough to grab onto
            RaycastHit2D grabCheck;
            if ( char_stats.IsFacingRight() )
            {
                Vector2 leftPoint = new Vector2(char_stats.char_collider.bounds.center.x + char_stats.char_collider.bounds.size.x, char_stats.char_collider.bounds.min.y - char_stats.char_collider.bounds.size.y);
                grabCheck = Physics2D.Raycast( leftPoint, Vector2.left, char_stats.char_collider.bounds.size.x );
            }
            else
            {
                Vector2 rightPoint = new Vector2(char_stats.char_collider.bounds.center.x - char_stats.char_collider.bounds.size.x, char_stats.char_collider.bounds.min.y - char_stats.char_collider.bounds.size.y);
                grabCheck = Physics2D.Raycast( rightPoint, Vector2.right, char_stats.char_collider.bounds.size.x );
            }

            if ( grabCheck.collider == downHit.collider && downHit.collider.gameObject.GetComponent<CollisionType>().WallClimb )
            {
                char_stats.current_master_state = CharEnums.MasterState.ClimbState;
                GroundToWallStart();
            }
            else
            {
                char_stats.is_jumping = false;
                //TODO: head shake animation to notify that the ledge is too short

            }
        }
    }

    /// <summary>
    /// if the player jumps into a wall and meet requirements, grab onto it if they meet requirements
    /// </summary>
    /// <param name="collisionObject"></param>
    public void InitiateWallGrab( Collider2D collisionObject )
    {
        if ( player_stats.acquired_mag_grip && collisionObject.gameObject.GetComponent<CollisionType>().WallClimb )
        {
            if ( current_climb_state == ClimbState.NotClimb )
            {
                if ( char_stats.IsInMidair && current_climb_state == ClimbState.NotClimb && wall_grab_delay_timer == WALL_GRAB_DELAY )
                {
                    // only grab the wall if we aren't popping out under it or over it
                    if ( collisionObject.bounds.min.y <= char_stats.char_collider.bounds.min.y )
                    {
                        // if character is a bit too above the ledge, bump them down till they're directly under it
                        // if this block is commented out, then the character will not snap directly to the ledge if slightly above it and will slide down till they grab on
                        /*
                        if (collisionObject.bounds.max.y < char_stats.char_collider.bounds.max.y)
                        {
                            // check to see if the wall we're gonna be offsetting against is too short.
                            RaycastHit2D predictionCast;
                            float offsetDistance = char_stats.char_collider.bounds.max.y - collisionObject.bounds.max.y;
                            Vector2 predictionCastOrigin = new Vector2(char_stats.char_collider.bounds.center.x, char_stats.char_collider.bounds.min.y - offsetDistance);
                            if (collisionObject.bounds.center.x < char_stats.char_collider.bounds.center.x)
                                predictionCast = Physics2D.Raycast(predictionCastOrigin, Vector2.left, char_stats.char_collider.bounds.size.x, CollisionMasks.AllCollisionMask);
                            else
                                predictionCast = Physics2D.Raycast(predictionCastOrigin, Vector2.right, char_stats.char_collider.bounds.size.x, CollisionMasks.AllCollisionMask);

                            if (predictionCast.collider == collisionObject)
                                transform.Translate(0.0f, -(char_stats.char_collider.bounds.max.y - collisionObject.bounds.max.y), 0.0f);
                        }
                        */
                        // if we're good to grab, get everything in order
                        if ( collisionObject.bounds.max.y >= char_stats.char_collider.bounds.max.y )
                        {
                            char_stats.ResetJump();
                            current_climb_state = ClimbState.WallClimb;
                            char_stats.current_master_state = CharEnums.MasterState.ClimbState;

                            // variable sets to prevent weird turning when grabbing onto a wall
                            // if the wall is to our left
                            if ( collisionObject.bounds.center.x < char_stats.char_collider.bounds.center.x )
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
                            char_stats.velocity.x = 0.0f;
                            // assign the grab_collider now that the grab is actually happening
                            grab_collider = collisionObject.GetComponent<Collider2D>();
                            //trigger the signal to start the wall climb animation
                            char_anims.WallGrabTrigger();
                            char_anims.ResetJumpDescend();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// if the player jumps into a ceiling and meet requirements, grab onto it if they meet requirements
    /// </summary>
    /// <param name="collisionObject"></param>
    public void InitiateCeilingGrab( Collider2D collisionObject )
    {
        if ( player_stats.acquired_mag_grip && collisionObject.gameObject.GetComponent<CollisionType>().CeilingClimb )
        {
            if ( current_climb_state == ClimbState.NotClimb )
            {
                if ( char_stats.IsInMidair && current_climb_state == ClimbState.NotClimb && wall_grab_delay_timer == WALL_GRAB_DELAY )
                {
                    // TODO: Ceiling grab
                    // only grab the ceiling if we aren't popping out over the side
                    //if (collisionObject.bounds.min.y <= char_stats.char_collider.bounds.min.y)

                }
            }
        }
    }
}
