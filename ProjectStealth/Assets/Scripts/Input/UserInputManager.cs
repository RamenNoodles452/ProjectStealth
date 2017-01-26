using UnityEngine;
using System.Collections;

public class UserInputManager : IInputManager
{

    void Start()
    {
        InputOverride = false;
    }

	// Update is called once per frame
	void Update () {

        if (InputOverride == false)
        {
            HorizontalAxis = Input.GetAxisRaw("Horizontal");
            VerticalAxis = Input.GetAxisRaw("Vertical");
            RunAxis = Input.GetAxisRaw("Run");


            if (HorizontalAxis != 0)
            {
                if (HorizontalAxisInstance)
                {
                    HorizontalAxisInstance = false;
                }
                if (horizontalInstanceCheck)
                {
                    HorizontalAxisInstance = true;
                    horizontalInstanceCheck = false;
                }
            }
            else if (HorizontalAxis == 0) //|| instantAxisInversion
            {
                HorizontalAxisInstance = false;
                horizontalInstanceCheck = true;
            }

            /*
            if (Input.GetButton("Horizontal"))
            {
                HorizontalInput = true;
            }
            else
            {
                HorizontalInput = false;
            }
            */
            if (Input.GetButton("Jump"))
            {
                //Debug.Log("Jump");
                //this.Jump.Invoke();
                JumpInput = true;
            }
            else
            {
                JumpInput = false;
            }
            if (Input.GetButtonDown("Jump"))
                JumpInputInst = true;
            else
                JumpInputInst = false;

            if (RunAxis > 0)
                RunInput = true;
            else
                RunInput = false;
            if (Input.GetButtonDown("Run"))
                RunInputDownInst = true;
            else
                RunInputDownInst = false;
            if (Input.GetButtonUp("Run"))
                RunInputUpInst = true;
            else
                RunInputUpInst = false;

            /*
            if (Input.GetButton("Attack"))
            {
                //Debug.Log("LightAttack");
                this.Attack.Invoke();
            }

            if (Input.GetButton("Evade"))
            {
                //Debug.Log("Evade");
                this.Evade.Invoke();
            }
            */
            if (Input.GetButton("Interact"))
                InteractInput = true;
            else
                InteractInput = false;

            if (Input.GetButtonDown("Interact"))
                InteractInputInst = true;
            else
                InteractInputInst = false;

        }
    }
}
