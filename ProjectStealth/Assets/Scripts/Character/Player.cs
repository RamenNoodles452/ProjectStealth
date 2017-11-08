using UnityEngine;
using System.Collections;
using System;

public class Player : SimpleCharacterCore
{
    #region vars
    // Player Modules
    private PlayerStats playerStats;
    private MagGripUpgrade magGrip;
    #endregion

    void Awake ()
	{
        if ( Referencer.Instance == null )
        {
            DontDestroyOnLoad(this.gameObject); // Persist across scene changes
        }
        else if (Referencer.Instance.player != this && Referencer.Instance.player != null)
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

            //Evade
            if ( InputManager.EvadeInputInst )
            {
                playerStats.Evade();
            }

            //Shoot
            if ( InputManager.ShootInputInst )
            {
                playerStats.Shoot();
            }

            //Attack
            if ( InputManager.AttackInputInst )
            {
                playerStats.Attack();
            }

            //Assassinate
            if ( InputManager.AssassinateInputInst )
            {
                playerStats.Assassinate();
            }

            //Cloak
            if ( InputManager.CloakInputInst )
            {
                playerStats.Cloak();
            }

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

     /// <returns>The coordinates of the center point of the player (in pixels)</returns>
    public Vector2 CenterPoint()
    {
        BoxCollider2D collider = this.gameObject.GetComponent<BoxCollider2D>();
        return new Vector2( this.gameObject.transform.position.x + collider.size.x / 2.0f, this.gameObject.transform.position.y + collider.size.y / 2.0f );
    }

    /// <returns>Whether the player is cloaked or not</returns>
    public bool IsCloaking() { return playerStats.IsCloaking(); }

    /// <returns>Whether the player is evading or not</returns>
    public bool IsEvading() { return playerStats.IsEvading(); }

    /// <returns>The player's current amount of shields</returns>
    public float GetShields() {  return playerStats.GetShields(); }

    /// <returns>The player's maximum amount of shields</returns>
    public float GetShieldsMax() {  return playerStats.GetShieldsMax(); }

    /// <returns>The player's current amount of energy</returns>
    public float GetEnergy() { return playerStats.GetEnergy(); }

    /// <returns>The player's maximum amount of energy</returns>
    public float GetEnergyMax() { return playerStats.GetEnergyMax(); }

    /// <summary>
    /// Saves checkpoint location to respawn at.
    /// </summary>
    /// <param name="coordinates">Coordinates</param>
    public void SetCheckpoint( Vector2 coordinates )
    {
        playerStats.SetCheckpoint( coordinates );
    }

    /// <summary>
    /// Hits the player
    /// </summary>
    /// <param name="damage">The amount of damage</param>
    public void Hit( float damage )
    {
        playerStats.Hit( damage );
    }


    /// <summary>
    /// Kills the player
    /// </summary>
    public void Kill()
    {
        //this is for things like dropping off a cliff, not damage-based death (that's handled in PlayerStats)
        //TODO: put an ani and delay on this
        playerStats.Respawn();
    }
}
