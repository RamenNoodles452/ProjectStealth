using UnityEngine;
using System.Collections;
using System;

public class Player : SimpleCharacterCore
{
    // Player Modules
	private PlayerStats player_stats;
    private MagGripUpgrade mag_grip;

    void Awake ()
	{
	}

	public override void Start ()
	{
		base.Start ();

		//walk and run vars
		WALK_SPEED = 1.0f; //used for cutscenes with Alice
		SNEAK_SPEED = 2.0f; //Alice's default speed
		RUN_SPEED = 4.5f;
		currentMoveState = moveState.isSneaking;

		player_stats = GetComponent<PlayerStats>();
        mag_grip = GetComponent<MagGripUpgrade>();
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
        if (char_stats.CurrentMasterState == CharacterStats.MasterState.defaultState)
        {
            base.Update();

            // base mag grip checks
            if (player_stats.AquiredMagGrip)
            {
                // if we want to grab down onto the wall from the ledge
                if (lookingOverLedge) // && we're standing on a grabbable surface?
                {
                    if (char_stats.OnTheGround && InputManager.JumpInputInst)
                    {
                        // do we want to climb down?
                        mag_grip.WallClimbFromLedge();
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
        if (char_stats.CurrentMasterState == CharacterStats.MasterState.defaultState)
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
        mag_grip.InitiateWallGrab(collisionObject.GetComponent<Collider2D>());
    }
}
