using UnityEngine;
using System.Collections;

public class Player : SimpleCharacterCore
{
    [HideInInspector]
    public int Health;

    public CharacterStatus Status;
    //enum states { idle, running };
    //states CurrentState = states.idle;
    //bool isRunning = false;

    

    void Awake()
    {
        this.Status = CharacterStatus.CreateInstance<CharacterStatus>();
    }

    public override void Start()
    {
        base.Start();
        if (GameState.Instance.PlayerState != null)
        {
            this.Status.Clone(GameState.Instance.PlayerState);
            Debug.Log("Loading from the GameState instance");
        }

        //walk and run vars
        WALK_SPEED = 1.0f; //used for cutscenes with Alice
        SNEAK_SPEED = 1.5f; //Alice's default speed
        RUN_SPEED = 4.0f;
        currentMoveState = moveState.isSneaking;
}

    void OnDestroy()
    {
        if (GameState.Instance.PlayerState == null)
        {
            GameState.Instance.PlayerState = CharacterStatus.CreateInstance<CharacterStatus>();
            Debug.Log("Creating a new PlayerState instance in GameState");
        }
        GameState.Instance.PlayerState.Clone(this.Status);
        Debug.Log("Storing the current player instance in GameState");
    }

    private void LoadInstance()
    {

    }

    private void StoreInstance()
    {

    }

    public override void Update()
    {
        if (InputManager.RunInput)
        {
            if (InputManager.RunInputInst)
            {
                tempMoveState = currentMoveState;
                currentMoveState = moveState.isRunning;
            }
        }
        else
        {
            if (InputManager.RunInputUpInst)
            {
                currentMoveState = tempMoveState;
            }
        }

        // set Sprite flip
        if (previousFacingDirection != FacingDirection)
        {
            SetFacing();
        }
        previousFacingDirection = FacingDirection;

        base.Update();

        
        prevMoveState = currentMoveState;
        /*
        //handle running
        if (InputManager.HorizontalInput == true)
        {
            CurrentState = states.running;
            Anim.SetBool("Running", true);
        }
        else if (InputManager.HorizontalInput == false)
        {
            CurrentState = states.idle;
            Anim.SetBool("Running", false);
        }
        */

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    void SetFacing()
    {
        Vector3 theScale = transform.localScale;
        theScale.x = FacingDirection;
        transform.localScale = theScale;
    }

    public void Run(float horizontal, float Vertical)
    {
        
        if (FacingDirection == 1 && horizontal > 0)
        {
            FacingDirection = -1;
            Anim.SetBool("TurnAround", true);
        }
        else if (FacingDirection == -1 && horizontal < 0)
        {
            FacingDirection = 1;
            Anim.SetBool("TurnAround", true);
        }
        
        /*
        if (horizontal < 0)
        {
            FacingDirection = 1;
        }
        else if (horizontal > 0)
        {
            FacingDirection = -1;
        */
        //Debug.Log("Move: " + horizontal + ", " + Vertical);
    }

    public void SetStopAnim(string stop)
    {
        if(stop == "false")
            Anim.SetBool("Stop", false);
        else if(stop == "true")
            Anim.SetBool("Stop", true);
    }

    public void SetTurningAnim()
    {
        Anim.SetBool("TurnAround", false);
    }

    /*
    public void Jump()
    {
        IsJumping = true;
    }
    */
}
