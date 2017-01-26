using UnityEngine;
using System.Collections;
using System;

public class Player : SimpleCharacterCore
{
    // Player Modules
	private PlayerStats playerStats;
    private MagGripUpgrade magGrip;

    void Awake ()
	{
	}

	public override void Start ()
	{
		base.Start ();

		//walk and run vars
		WALK_SPEED = 1.0f; //used for cutscenes with Alice
        SNEAK_SPEED = 2.0f; //PC's default speed
        RUN_SPEED = 4.5f;
        charStats.CurrentMoveState = CharEnums.MoveState.isSneaking;

		playerStats = GetComponent<PlayerStats>();
        magGrip = GetComponent<MagGripUpgrade>();
	}

	void OnDestroy ()
	{
		
	}

	private void LoadInstance ()
	{

	}

	private void StoreInstance ()
	{

	}

	public override void Update ()
	{
        if (charStats.CurrentMasterState == CharEnums.MasterState.defaultState)
        {
            base.Update();

            // base mag grip checks
            if (playerStats.AquiredMagGrip)
            {
                // if we want to grab down onto the wall from the ledge
                if (lookingOverLedge) // && we're standing on a grabbable surface?
                {
                    if (charStats.OnTheGround && InputManager.JumpInputInst)
                    {
                        // do we want to climb down?
                        magGrip.WallClimbFromLedge();
                    }
                }
            }
        }
	}

    public override void LateUpdate()
    {
        //this happens regardless of state
        base.LateUpdate();
    }

	public override void FixedUpdate()
	{
        if (charStats.CurrentMasterState == CharEnums.MasterState.defaultState)
        {
            base.FixedUpdate();
        }    
	}

    /// <summary>
    /// at the player level, we have to take into looking over the ledge from wall climbs as well
    /// </summary>
    public override void LookingOverLedge()
    {
        base.LookingOverLedge();
    }

    public override void TouchedWall(GameObject collisionObject)
    {
        magGrip.InitiateWallGrab(collisionObject.GetComponent<Collider2D>());
    }

    public override void TouchedCeiling(GameObject collisionObject)
    {
        magGrip.InitiateCeilingGrab(collisionObject.GetComponent<Collider2D>());
    }
}
