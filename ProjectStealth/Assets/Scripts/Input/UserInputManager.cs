using UnityEngine;
using System.Collections;

//TODO: wrap everything, rename the private vars.
public class UserInputManager : IInputManager
{

    void Start()
    {
        InputOverride = false;
    }

    // Update is called once per frame
    void Update()
    {

        HorizontalAxis = Input.GetAxisRaw( "Horizontal" );
        VerticalAxis = Input.GetAxisRaw( "Vertical" );
        if ( InputOverride == false )
        {

            RunAxis = Input.GetAxisRaw( "Run" );


            if ( HorizontalAxis != 0 )
            {
                if ( HorizontalAxisInstance )
                {
                    HorizontalAxisInstance = false;
                }
                if ( horizontalInstanceCheck )
                {
                    HorizontalAxisInstance = true;
                    horizontalInstanceCheck = false;
                }
            }
            else if ( HorizontalAxis == 0 ) //|| instantAxisInversion
            {
                HorizontalAxisInstance = false;
                horizontalInstanceCheck = true;
            }

            if ( Input.GetButton( "Jump" ) ) { JumpInput = true; }
            else { JumpInput = false; }

            if ( Input.GetButtonDown( "Jump" ) ) { JumpInputInst = true; }
            else { JumpInputInst = false; }

            if ( RunAxis > 0 ) { RunInput = true; }
            else { RunInput = false; }
            if ( Input.GetButtonDown( "Run" ) ) { RunInputDownInst = true; }
            else { RunInputDownInst = false; }
            if ( Input.GetButtonUp( "Run" ) ) { RunInputUpInst = true; }
            else { RunInputUpInst = false; }

            if ( Input.GetButton( "Attack" ) ) { AttackInput = true; }
            else { AttackInput = false; }
            if ( Input.GetButtonDown( "Attack" ) ) { AttackInputInst = true; }
            else { AttackInputInst = false; }

            if ( Input.GetButton( "Assassinate" ) ) { AssassinateInput = true; }
            else { AssassinateInput = false; }
            if ( Input.GetButtonDown( "Assassinate" ) ) { AssassinateInputInst = true; }
            else { AssassinateInputInst = false; }

            if ( Input.GetButton( "Shoot" ) ) { ShootInput = true; }
            else { ShootInput = false; }
            if ( Input.GetButtonDown( "Shoot" ) ) { ShootInputInst = true; }
            else { ShootInputInst = false; }

            if ( Input.GetButton( "Cloak" ) ) { CloakInput = true; }
            else { CloakInput = false; }
            if ( Input.GetButtonDown( "Cloak" ) ) { CloakInputInst = true; }
            else { CloakInputInst = false; }

            if ( Input.GetButton( "Evade" ) ) { EvadeInput = true; }
            else { EvadeInput = false; }

            if ( Input.GetButtonDown( "Evade" ) ) { EvadeInputInst = true; }
            else { EvadeInputInst = false; }

            if ( Input.GetKeyDown( KeyCode.Semicolon ) ) { AdrenalineInputInst = true; }
            else { AdrenalineInputInst = false; }

            if ( Input.GetButton( "Interact" ) ) { InteractInput = true; }
            else { InteractInput = false; }

            if ( Input.GetButtonDown( "Interact" ) ) { InteractInputInst = true; }
            else { InteractInputInst = false; }

        }
    }
}
