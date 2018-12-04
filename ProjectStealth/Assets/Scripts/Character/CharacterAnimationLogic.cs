using UnityEngine;
using System.Collections;

// Reads in character state data from char_stats, and sets/executes animator triggers accordingly.
public class CharacterAnimationLogic : MonoBehaviour
{
    #region vars
    public Animator animator;
    protected CharacterStats char_stats;
    protected UserInputManager input_manager;
    #endregion

    // Use this for pre-initialization (reference setup)
    private void Awake()
    {
        animator = transform.Find( "Sprites" ).GetComponent<Animator>();
        char_stats = GetComponent<CharacterStats>();
        input_manager = GetComponent<UserInputManager>();
    }

    // Use this for initialization
    void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {
        if ( char_stats.current_master_state == CharEnums.MasterState.DefaultState )
        {
            animator.SetBool( "midair", char_stats.IsInMidair );
        }
        else if ( char_stats.current_master_state == CharEnums.MasterState.ClimbState )
        {
            animator.SetBool( "midair", false );
        }

        CoverLogic();
        RunningLogic();
        SneakingLogic();
        CrouchLogic();
        WallClimb();
        WallSlide();
        HorizontalAxisInput();
    }

    private void HorizontalAxisInput()
    {
        if (input_manager.HorizontalAxis != 0)
        {
            animator.SetBool( "horizontal_axis_input", true );
        }
        else
        {
            animator.SetBool( "horizontal_axis_input", false );
        }
    }

    // This is temporary & may not solve all problems. Once the animator has run states, adjust this accordingly.
    private void RunningLogic()
    {
        if ( char_stats.IsInMidair ) { return; }
        if ( char_stats.current_move_state != CharEnums.MoveState.IsRunning ) { return; }
        if ( char_stats.velocity.x != 0.0f )
        {
            animator.SetBool( "sneaking", true ); // TODO: replace
        }
        else
        {
            animator.SetBool( "sneaking", false ); // TODO: replace
        }
    }

    private void SneakingLogic()
    {
        if ( char_stats.IsInMidair ) { return; }
        if ( char_stats.current_move_state != CharEnums.MoveState.IsSneaking ) { return; }
        if ( char_stats.velocity.x != 0.0f )
        {
            animator.SetBool( "sneaking", true );
        }
        else
        {
            animator.SetBool( "sneaking", false );
        }
    }

    private void CoverLogic()
    {
        if ( char_stats.is_taking_cover == false )
        {
            animator.SetBool( "taking_cover", false );
        }
        else
        {
            animator.SetBool( "taking_cover", true );
        }
    }

    private void CrouchLogic()
    {
        if ( char_stats.is_crouching == false )
        {
            animator.SetBool( "crouching", false );
        }
        else
        {
            animator.SetBool( "crouching", true );
        }
    }

    public void SetCrouch()
    {
        animator.SetBool( "crouching", true );
    }

    // Triggers are called within the character scripts
    public void JumpTrigger()
    {
        animator.SetTrigger( "jump_ascend" );
    }

    // Triggers are called within the character scripts
    public void FallTrigger()
    {
        animator.SetTrigger( "jump_descend" );
    }

    public void ResetJumpDescend()
    {
        animator.ResetTrigger( "jump_descend" );
    }

    public void FallthoughTrigger()
    {
        animator.SetTrigger( "fallthrough" );
    }

    // Triggers are called within the character scripts
    public void WallGrabTrigger()
    {
        animator.SetTrigger( "wall_grab_trigger" );
        animator.SetBool( "sneaking", false );
    }

    public void WallClimb()
    {
        if ( char_stats.current_master_state == CharEnums.MasterState.ClimbState && char_stats.velocity.y > 0 )
        {
            animator.SetBool( "wall_climb", true );
        }
        else
        {
            animator.SetBool( "wall_climb", false );
        }
    }

    public void WallSlide()
    {
        if ( char_stats.current_master_state == CharEnums.MasterState.ClimbState && char_stats.velocity.y < 0 )
        {
            animator.SetBool( "wall_slide", true );
        }
        else
        {
            animator.SetBool( "wall_slide", false );
        }
    }

    // triggers when a character dropps from a wall by moving down to the end of the wall and pressing down + jump
    public void DropFromWallTrigger()
    {
        animator.SetTrigger( "drop_from_wall" );
    }

    // triggers when a character climbs up from a wall
    public void WallToGroundTrigger()
    {
        animator.SetTrigger( "wall_to_ground" );
    }

    // triggers when a character climbs to the wall from the ground
    public void GroundToWallTrigger()
    {
        animator.SetTrigger( "ground_to_wall" );
    }

    // triggers when a character slides down the wall and touches the ground
    public void WallSlideTouchGround()
    {
        animator.SetTrigger( "wall_slide_touch_ground" );
    }

    // triggers when a character grabs the ceiling
    public void CeilingGrabTrigger()
    {
        animator.SetTrigger( "ceiling_grab_trigger" );
    }

    // triggers when a character dodge rolls on the ground
    public void DodgeRollTrigger()
    {
        animator.SetTrigger( "dodge_roll_trigger" );
    }

    // triggers when a character dodge rolls in midair
    public void DodgeRollAerialTrigger()
    {
        animator.SetTrigger( "dodge_roll_aerial_trigger" );
    }

    // bool when you're looking away from the wall when climbing
    public void WallLookAway( bool is_looking_away )
    {
        animator.SetBool( "wall_look_away", is_looking_away );
    }

    /// <summary>
    /// Resets wall-climb related animations state parameters.
    /// </summary>
    public void ResetWallClimb()
    {
        //animator.ResetTrigger( "wall_grab_trigger" );
        //animator.SetBool( "wall_climb", false );
        //animator.SetBool( "wall_slide", false );
        //animator.ResetTrigger( "drop_from_wall" );
        //animator.ResetTrigger( "wall_to_ground" );
        //animator.ResetTrigger( "ground_to_wall" );
        //animator.ResetTrigger( "wall_slide_touch_ground" );
    }

    /// <summary>
    /// Resets all animation state parameters.
    /// </summary>
    public void Reset()
    {
        animator.SetBool( "sneaking", false );
        animator.SetBool( "taking_cover", false );
        animator.SetFloat( "jumping_vel", 0.0f );
        animator.ResetTrigger( "jump_ascend" );
        animator.ResetTrigger( "jump_descend" );
        animator.SetBool( "midair", false );
        animator.SetBool( "crouching", false );
        animator.ResetTrigger( "wall_grab_trigger" );
        animator.SetBool( "wall_climb", false );
        animator.SetBool( "wall_slide", false );
        animator.ResetTrigger( "drop_from_wall" );
        animator.ResetTrigger( "wall_to_ground" );
        animator.ResetTrigger( "ground_to_wall" );
        animator.ResetTrigger( "wall_slide_touch_ground" );
        animator.ResetTrigger( "ceiling_grab_trigger" );
        animator.ResetTrigger( "fallthrough" );
        animator.SetBool( "horizontal_axis_input", false );
        animator.SetBool( "wall_look_away", false );
    }
}

