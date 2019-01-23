using UnityEngine;
using System.Collections;

/// <summary>
/// Represents the state of the input controls.
/// </summary>
public class UserInputManager : IInputManager
{
    // Use this for initialization
    void Start()
    {
        IgnoreInput = false;
    }

    // Update is called once per frame
    void Update()
    {
        if ( IgnoreInput ) // Ignore all actual input state, use defaults.
        {
            Reset();
            return;
        }
        GetInputState();
    }

    /// <summary>
    /// Sets properties to represent the input state.
    /// </summary>
    private void GetInputState()
    {
        AimingInput();

        PreviousHorizontalAxis = HorizontalAxis;
        PreviousVerticalAxis = VerticalAxis;

        HorizontalAxis = Input.GetAxisRaw( "Horizontal" );
        VerticalAxis = Input.GetAxisRaw( "Vertical" );
        RunAxis = Input.GetAxisRaw( "Run" );

        UnCrouchInputInst = ( VerticalAxis > 0.1f && PreviousVerticalAxis < 0.1f );
        CrouchInputInst = ( VerticalAxis < -0.1f && PreviousVerticalAxis > -0.1f );

        JumpInput = Input.GetButton( "Jump" );
        JumpInputInst = Input.GetButtonDown( "Jump" );
        RunInput = ( RunAxis > 0 );
        RunInputDownInst = Input.GetButtonDown( "Run" );
        //RunInputUpInst = Input.GetButtonUp( "Run" ); // unused
        AttackInput = Input.GetButton( "Attack" );
        AttackInputInst = Input.GetButtonDown( "Attack" );
        AssassinateInput = Input.GetButton( "Assassinate" );
        AssassinateInputInst = Input.GetButtonDown( "Assassinate" );
        ShootInput = Input.GetButton( "Shoot" );
        ShootInputInst = Input.GetButtonDown( "Shoot" );
        ShootInputReleaseInst = Input.GetButtonUp( "Shoot" );
        //CloakInput = Input.GetButton( "Cloak" );
        //CloakInputInst = Input.GetButtonDown( "Cloak" );
        EvadeInput = Input.GetButton( "Evade" );
        EvadeInputInst = Input.GetButtonDown( "Evade" );
        AdrenalineInputInst = Input.GetKeyDown( KeyCode.Semicolon );
        InteractInput = Input.GetButton( "Interact" );
        InteractInputInst = Input.GetButtonDown( "Interact" );
        GadgetInputInst = Input.GetButtonDown( "Gadget" );
        GadgetInput = Input.GetButton( "Gadget" );
        GadgetInputReleaseInst = Input.GetButtonUp( "Gadget" );
    }

    /// <summary>
    /// Resets all the input variables.
    /// Used when locking out player input.
    /// </summary>
    private void Reset()
    {
        Vector3 viewport_point = Camera.main.WorldToViewportPoint( Referencer.instance.player.transform.position );
        AimPosition = new Vector2( viewport_point.x * Screen.width, viewport_point.y * Screen.height );

        AimToggleInputInst = false;
        HorizontalAimAxis = 0.0f;
        VerticalAimAxis = 0.0f;
        HorizontalAxis = 0.0f;
        VerticalAxis = 0.0f;
        PreviousHorizontalAxis = 0.0f;
        PreviousVerticalAxis = 0.0f;
        RunAxis = 0.0f;
        JumpInput = false;
        JumpInputInst = false;
        RunInput = false;
        RunInputDownInst = false;
        RunInputUpInst = false;
        AttackInput = false;
        AttackInputInst = false;
        AssassinateInput = false;
        AssassinateInputInst = false;
        ShootInput = false;
        ShootInputInst = false;
        ShootInputReleaseInst = false;
        //CloakInput = false;
        //CloakInputInst = false;
        EvadeInput = false;
        EvadeInputInst = false;
        CrouchInputInst = false;
        UnCrouchInputInst = false;
        AdrenalineInputInst = false;
        InteractInput = false;
        InteractInputInst = false;
        GadgetInputInst = false;
        GadgetInput = false;
        GadgetInputReleaseInst = false;
    }

    /// <summary>
    /// Parses aiming input
    /// </summary>
    private void AimingInput()
    {
        #region aim
        AimToggleInputInst = false;
        bool is_stick = false;
        //AimMode = ManualAimMode.position;

        #region Auto vs Manual Toggle
        // Manual / Auto toggling
        if ( is_stick )
        {
            //HorizontalAimAxis = Input.GetAxis();
            //VerticalAimAxis = Input.GetAxis();
            if ( AimMode == ManualAimMode.angle )
            {
                // non-neutral -> neutral
                if ( IsManualAimOn && HorizontalAimAxis == 0.0f && VerticalAimAxis == 0.0f )
                {
                    IsManualAimOn = false;
                    AimToggleInputInst = true;
                }
                else if ( ! IsManualAimOn && ( HorizontalAimAxis != 0.0f || VerticalAimAxis != 0.0f ) ) // neutral -> non-neutral
                {
                    IsManualAimOn = true;
                    AimToggleInputInst = true;
                }
            }
            else if ( AimMode == ManualAimMode.position )
            {
                // Aim axis in: L3
                AimToggleInputInst = Input.GetButtonDown( "AimToggle" );
            }
        }
        else
        {
            AimToggleInputInst = Input.GetButtonDown( "AimToggle" );
            if ( AimToggleInputInst ) { IsManualAimOn = ! IsManualAimOn; }
        }
        #endregion

        // Re-zero.
        if ( AimToggleInputInst && IsManualAimOn && AimMode == ManualAimMode.position )
        {
            Vector3 viewport_point = Camera.main.WorldToViewportPoint( Referencer.instance.player.transform.position );
            AimPosition = new Vector2( viewport_point.x * Screen.width, viewport_point.y * Screen.height );
        }

        #region Manual Aim Logic
        // Aim.
        if ( IsManualAimOn )
        {
            // If aim mode is angle, mouse or stick
            if ( AimMode == ManualAimMode.angle )
            {
                if ( is_stick )
                {
                    // do nothing, just keep axis values
                    AimPosition = Vector2.zero;
                }
                else
                {
                    AimPosition = new Vector2( Input.mousePosition.x, Input.mousePosition.y );
                    Vector3 screen_position = Camera.main.WorldToViewportPoint( Referencer.instance.player.transform.position );
                    screen_position.x = screen_position.x * Screen.width;
                    screen_position.y = screen_position.y * Screen.height;
                    float aim_angle   = Mathf.Atan2( Input.mousePosition.y - screen_position.y, Input.mousePosition.x - screen_position.x );
                    HorizontalAimAxis = Mathf.Cos( aim_angle );
                    VerticalAimAxis = Mathf.Sin( aim_angle );
                }
            }
            else if ( AimMode == ManualAimMode.position )
            {
                if ( is_stick )
                {
                    AimPosition = AimPosition + new Vector2( HorizontalAimAxis, VerticalAimAxis ) * 320.0f * Time.deltaTime * Time.timeScale;
                    // TODO: factor out sensitivity into a setting.
                    // TODO: clamp.
                }
                else
                {
                    AimPosition = new Vector2( Input.mousePosition.x, Input.mousePosition.y );
                    HorizontalAimAxis = 0.0f;
                    VerticalAimAxis = 0.0f;
                }
            }
        }
        #endregion
        #endregion
    }
}
