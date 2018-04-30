﻿using UnityEngine;
using System.Collections;
using System;

public class Player : SimpleCharacterCore
{
    #region vars
    // Player Modules
    private PlayerStats player_stats;
    private MagGripUpgrade mag_grip;
    #endregion

    void Awake ()
	{
        player_stats = GetComponent<PlayerStats>();

        if ( Referencer.instance == null )
        {
            DontDestroyOnLoad(this.gameObject); // Persist across scene changes
        }
        else if (Referencer.instance.player != this && Referencer.instance.player != null)
        {
            Destroy( this.gameObject ); // NO CLONES!
        }
        else
        { 
            DontDestroyOnLoad(this.gameObject); // Persist across scene changes
        }
	}

	public override void Start ()
	{
		base.Start ();

		//walk and run vars
		WALK_SPEED = 1.0f; //used for cutscenes with Alice
        SNEAK_SPEED = 2.0f; //PC's default speed
        RUN_SPEED = 4.5f;
        char_stats.current_move_state = CharEnums.MoveState.IsSneaking;

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
        if (char_stats.current_master_state == CharEnums.MasterState.DefaultState)
        {
            base.Update();

            //Evade
            if ( input_manager.EvadeInputInst )
            {
                player_stats.Evade();
            }

            //Shoot
            if ( input_manager.ShootInputInst )
            {
                player_stats.Shoot();
            }

            //Attack
            if ( input_manager.AttackInputInst )
            {
                player_stats.Attack();
            }

            //Assassinate
            if ( input_manager.AssassinateInputInst )
            {
                player_stats.Assassinate();
            }

            //Cloak
            if ( input_manager.CloakInputInst )
            {
                player_stats.Cloak();
            }

            // base mag grip checks
            if (player_stats.acquired_mag_grip)
            {
                // if we want to grab down onto the wall from the ledge
				if (is_overlooking_ledge) // && we're standing on a grabbable surface?
                {
					if (!char_stats.IsInMidair && input_manager.JumpInputInst)
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
        if (char_stats.current_master_state == CharEnums.MasterState.DefaultState)
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

    public override void TouchedCeiling(GameObject collisionObject)
    {
        mag_grip.InitiateCeilingGrab(collisionObject.GetComponent<Collider2D>());
    }

     /// <returns>The coordinates of the center point of the player (in pixels)</returns>
    public Vector2 CenterPoint()
    {
        BoxCollider2D collider = this.gameObject.GetComponent<BoxCollider2D>();
        return new Vector2( this.gameObject.transform.position.x + collider.size.x / 2.0f, this.gameObject.transform.position.y + collider.size.y / 2.0f );
    }

    /// <returns>Whether the player is cloaked or not</returns>
    public bool IsCloaking() { return player_stats.IsCloaking(); }

    /// <returns>Whether the player is evading or not</returns>
    public bool IsEvading() { return player_stats.IsEvading(); }

    /// <returns>The player's current amount of shields</returns>
    public float GetShields() {  return player_stats.GetShields(); }

    /// <returns>The player's maximum amount of shields</returns>
    public float GetShieldsMax() {  return player_stats.GetShieldsMax(); }

    /// <returns>The player's current amount of energy</returns>
    public float GetEnergy() { return player_stats.GetEnergy(); }

    /// <returns>The player's maximum amount of energy</returns>
    public float GetEnergyMax() { return player_stats.GetEnergyMax(); }

    /// <summary>
    /// Saves checkpoint location to respawn at.
    /// </summary>
    /// <param name="coordinates">Coordinates</param>
    public void SetCheckpoint( Vector2 coordinates )
    {
        if ( player_stats == null )
        {
            Debug.Log( "Player stats accessed before available." );
            return;
        }
        player_stats.SetCheckpoint( coordinates );
    }

    /// <summary>
    /// Hits the player
    /// </summary>
    /// <param name="damage">The amount of damage</param>
    public void Hit( float damage )
    {
        player_stats.Hit( damage );
    }


    /// <summary>
    /// Kills the player
    /// </summary>
    public void Kill()
    {
        //this is for things like dropping off a cliff, not damage-based death (that's handled in PlayerStats)
        //TODO: put an ani and delay on this
        player_stats.Respawn();
    }
}
