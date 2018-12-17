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
        HorizontalAxis = Input.GetAxisRaw( "Horizontal" );
        VerticalAxis = Input.GetAxisRaw( "Vertical" );
        RunAxis = Input.GetAxisRaw( "Run" );
        JumpInput = Input.GetButton( "Jump" );
        JumpInputInst = Input.GetButtonDown( "Jump" );
        RunInput = ( RunAxis > 0 );
        RunInputDownInst = Input.GetButtonDown( "Run" );
        RunInputUpInst = Input.GetButtonUp( "Run" );
        AttackInput = Input.GetButton( "Attack" );
        AttackInputInst = Input.GetButtonDown( "Attack" );
        AssassinateInput = Input.GetButton( "Assassinate" );
        AssassinateInputInst = Input.GetButtonDown( "Assassinate" );
        ShootInput = Input.GetButton( "Shoot" );
        ShootInputInst = Input.GetButtonDown( "Shoot" );
        ShootInputReleaseInst = Input.GetButtonUp( "Shoot" );
        CloakInput = Input.GetButton( "Cloak" );
        CloakInputInst = Input.GetButtonDown( "Cloak" );
        EvadeInput = Input.GetButton( "Evade" );
        EvadeInputInst = Input.GetButtonDown( "Evade" );
        AdrenalineInputInst = Input.GetKeyDown( KeyCode.Semicolon );
        InteractInput = Input.GetButton( "Interact" );
        InteractInputInst = Input.GetButtonDown( "Interact" );
    }

    /// <summary>
    /// Resets all the input variables.
    /// Used when locking out player input.
    /// </summary>
    private void Reset()
    {
        HorizontalAxis = 0.0f;
        VerticalAxis = 0.0f;
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
        CloakInput = false;
        CloakInputInst = false;
        EvadeInput = false;
        EvadeInputInst = false;
        AdrenalineInputInst = false;
        InteractInput = false;
        InteractInputInst = false;
    }
}
