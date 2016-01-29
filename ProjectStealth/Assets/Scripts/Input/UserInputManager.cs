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

            if (Input.GetButton("Run"))
                RunInput = true;
            else
                RunInput = false;
            if (Input.GetButtonDown("Run"))
                RunInputInst = true;
            else
                RunInputInst = false;
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

            if (Input.GetButton("Interact"))
            {
                //Debug.Log("Interact");
                this.Interact.Invoke();
            }
            */
        }
    }
}
