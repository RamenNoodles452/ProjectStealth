using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    #region vars
    public float Health;
    public float HealthMax = 1.0f;

    #region Shield
    private float Shield;
    private float ShieldMax = 100.0f;

    private bool WasHitThisFrame;
    private bool IsRegenerating;
    private float ShieldRegenerationDelay = 2.0f;
    private float ShieldDelayCounter = 0.0f;
    private float ShieldRegenerationTime = 4.0f; // 25% per second
    #endregion

    #region Energy
    private bool IsEnergyRegenerating;
    private float Energy = 100.0f;
    private float EnergyMax = 100.0f;
    //private float EnergyRegenerationDelay = 0.0f;
    //private float EnergyDelayCounter = 0.0f;
    private float EnergyRegenerationTime = 4.0f; // 25% per second
    #endregion

    #region Evade
    private bool EvadeEnqueued = false;
    public float EvadeCost = 20.0f;

    private bool isEvading = false;

    private bool isEvadeWindingUp = false;
    private float EvadeWindupTime = 0.10f;
    private float EvadeWindupCounter = 0.0f;

    private bool Invincible = false;
    private float InvincibilityTime = 0.65f;
    private float InvincibilityCounter = 0.0f;

    private bool isEvadeRecovering = false;
    private float EvadeRecoveryTime = 0.10f;
    private float EvadeRecoveryCounter = 0.15f;

	//TODO: evade speed in direction, backstep from neutral (2 states here) chord with direction
	// aerial 4 directional dodge movement (no iframes)
	// sliders for total time, windup time, i frames (not for aerial), recovery time / skip for enqueued windup, speed in pixels per second.
	// disable normal movement when dodging
	// don't collide with enemies while dodging
    #endregion

    #region Cloak
    private bool IsCloaked = false;
    private const float CloakCost = 35.0f;
    private const float CloakDrainPerSecond = 6.5f; //10s
    //TODO: may need to put a CD on cloak to prevent spamming if regen makes even huge cost spammable.
    // Just change input mapping to require a couple seconds of sneaking in place.
    #endregion

    #region silencer
    // Not sure if this should be consolidated with stealth / UI needs simplification here....
    // Potentially concerning
    private float silencer = 0.0f;
    private float silencerMax = 3.0f;
    private float silencerRegen = 1.0f;
    #endregion

    // progress values
    public bool AquiredMagGrip;

    // checkpointing
    public Vector2 checkpoint;

    // Noise Prefab: Set in Editor
    public GameObject NoisePrefab;

	private CharacterStats charStats;
	private float walkAnimationTimer;
    #endregion

    #region stat accessors
    // I don't like having accessors phony encapsulation, but we'll keep things together

    /// <returns>The player's current amount of shields</returns>
    public float GetShields()
    {
        return Shield;
    }

    /// <returns>The player's maximum amount of shields</returns>
    public float GetShieldsMax()
    {
        return ShieldMax;
    }

    /// <returns>The player's current amount of energy</returns>
    public float GetEnergy()
    {
        return Energy;
    }

    /// <returns>The player's maximum amount of energy</returns>
    public float GetEnergyMax()
    {
        return EnergyMax;
    }
    #endregion

	/// <summary>
	/// Sets the animation timer for noise generation when the player starts walking
	/// </summary>
	public void StartWalking()
	{
		walkAnimationTimer = 0.15f;
	}

    /// <summary>
    /// Sets location to respawn at
    /// </summary>
    /// <param name="coordinates">coordinates</param>
    public void SetCheckpoint( Vector2 coordinates )
    {
        checkpoint = coordinates;
        Debug.Log( "Checkpoint!: " + coordinates );
    }

    /// <summary>
    /// Does damage to the player's health / shields
    /// </summary>
    /// <param name="damage">the amount of damage</param>
    public void Hit( float damage )
    {
        if ( Invincible )
        {
            return;
        }

        // Interrupt shield recharge
        WasHitThisFrame = true;

        // Deal damage
        if ( Shield > 0.0f )
        {
            Shield = Mathf.Max( Shield - damage, 0.0f );
            if ( Shield <= 0.0f )
            {
                // shield break
            }
        }
        else
        {
            Health = Mathf.Max( Health - damage, 0.0f );
            if ( Health <= 0.0f )
            {
                // kill
                Respawn(); //TODO: put an ani and delay on this
            }
        }
    }

    /// <summary>
    /// Respawns the player at the last checkpoint. Also, resets the level.
    /// </summary>
    public void Respawn()
    {
        // TODO: reach into gamestate and force reset enemies.

        // Teleport to checkpoint
        // value type
        /*if ( checkpoint == null )
        {
            // find and set a default checkpoint?
            Debug.LogError( "Player died without a checkpoint set!" );
        }
        else*/
        //{
        this.gameObject.transform.position = new Vector3( checkpoint.x, checkpoint.y, this.gameObject.transform.position.z );
        //}

        // Reset stats
        ResetState();
    }

    /// <summary>
    /// Reset state to prevent bugs when changing levels / respawning (may need to split into 2)
    /// Also refill resources
    /// </summary>
    private void ResetState()
    {
        EvadeEnqueued = false;
        Invincible = false;
        IsCloaked = false;
        WasHitThisFrame = false;

        Health = HealthMax;
        Shield = ShieldMax;

        // TODO: interrupt everything, stop animations, reset all that

        // reset movement
        charStats = this.gameObject.GetComponent<CharacterStats>();
        charStats.Velocity = new Vector2( 0.0f, 0.0f );
    }

    /// <summary>
    /// Fire weapon
    /// </summary>
    public void Shoot()
    {
        if ( isEvading ) { return; } // no shooting mid-evade
        //animation lock checks?

        //make noise?
        //use silencer meter / shooting when cloaked makes no noise
        if ( silencer >= 1.0f )
        {
            // Suppressed shot
            silencer -= 1.0f;
        }
        else
        { 
            // Go loud
            GameObject noiseObj = GameObject.Instantiate( NoisePrefab, this.gameObject.transform.position, Quaternion.identity );
            Noise noise = noiseObj.GetComponent<Noise>();
            noise.lifetime = 0.25f; // seconds
            noise.radius = 200.0f;
        }

        if ( IsCloaked ) { IsCloaked = false; } // attacking breaks stealth (even silenced?)

        // Actually fire bullets
        // Start with closest tagged enemies (if any)?
    }

    /// <summary>
    /// Light, fast attack combo
    /// </summary>
    public void Attack()
    {
        if ( isEvading ) { return; } // no attacking mid-evade
        //animation lock checks?

        if ( IsCloaked ) { IsCloaked = false; } // attacking breaks stealth

        //enqueueing? + comboing
        //tag enemies for auto aim
    }

    /// <summary>
    /// Heavy attack / insta kill
    /// </summary>
    public void Assassinate()
    {
        if ( isEvading ) { return; } // no assassinating mid-evade
        //animation lock checks?

        //need to be positioned
        //look at enemy positions
        if ( IsCloaked ) { IsCloaked = false; } // attacking breaks stealth

        //enqueueing?
    }

    /// <summary>
    /// Evades
    /// </summary>
    public void Evade()
    {
        if ( isEvading )
        {
            // enqueue if within grace period
            if ( isEvadeRecovering )
            {
                EvadeEnqueued = true;
            }
            else if ( Invincible )
            {
                if ( InvincibilityTime - InvincibilityCounter <= 0.1f ) { EvadeEnqueued = true; }
            }

            return;
        }

        if ( Energy <= EvadeCost ) { return; } // insufficient resources. play sound?

        // check if stuck in a non-cancellable animation

        isEvading = true;
        isEvadeWindingUp = true;
        EvadeWindupCounter = 0.0f;
        Invincible = false;
        InvincibilityCounter = 0.0f;
        isEvadeRecovering = false;
        EvadeRecoveryCounter = 0.0f;

        Energy -= EvadeCost;

        // differences for aerial evasion / ground evasion?
        // animate
        // movement? collision mask changes?

        //return true/false? based on abort / already evading / stuck in recovery / resource insuffiency / success
    }

    /// <returns>Whether the player is evading or not</returns>
    public bool IsEvading()
    {
        return isEvading;
    }

    #region Cloak
    /// <summary>
    /// Turn on cloaking
    /// </summary>
    public void Cloak()
    {
        if ( IsCloaked ) { return; } // Decloak?

        // check if unlocked
        // animation lock checks?

        if ( Energy >= CloakCost )
        {
            Energy = Energy - CloakCost;
        }
        else
        {
            return;
        }

        IsCloaked = true;
        //animate
    }

    /// <summary>
    /// Turn off cloaking
    /// </summary>
    public void Decloak()
    {
        IsCloaked = false;
        //animate
    }

    /// <returns>Whether the player is currently cloaked or not</returns>
    public bool IsCloaking() { return IsCloaked; }
    #endregion

    /// <summary>
    /// Begins dodging invincibility frames
    /// </summary>
    private void StartIFrames()
    {
        Invincible = true;
        InvincibilityCounter = 0.0f;
    }

    /// <summary>
    /// Initialization upon level entry
    /// </summary>
    void Start()
    {
        ResetState();
    }

    /// <summary>
    /// Timer stuff that gets checked every frame
    /// </summary>
    void Update()
    {
        // Death
        if ( Health <= 0.0f )
        {
            Respawn(); //TODO: if this takes >1 frame, need state tracking.
        }

        #region timers
        #region shield
        // Shield regeneration
        if ( WasHitThisFrame )
        {
            WasHitThisFrame = false;
            IsRegenerating = false;
            ShieldDelayCounter = 0.0f;
        }

        if ( IsRegenerating )
        {
            // Regenerate to full
            Shield = Mathf.Min( Shield + ( ShieldMax / ShieldRegenerationTime ) * Time.deltaTime * TimeScale.timeScale, ShieldMax );
            if ( Shield == ShieldMax )
            {
                IsRegenerating = false;
            }
        }
        else if ( Shield < ShieldMax )
        {
            // Delay before regen begins
            ShieldDelayCounter += Time.deltaTime * TimeScale.timeScale;
            if ( ShieldDelayCounter >= ShieldRegenerationDelay )
            {
                IsRegenerating = true;
                // play recharge sound?
            }
        }
        #endregion

        #region Evade
        if ( isEvadeWindingUp )
        {
            //Debug.Log( "Evade windup" );
            EvadeWindupCounter += Time.deltaTime * TimeScale.timeScale;
            if ( EvadeWindupCounter >= EvadeWindupTime )
            {
                isEvadeWindingUp = false;
                StartIFrames();
            }
        }

        // I frames
        if ( Invincible )
        {
            //Debug.Log("Evade");
            InvincibilityCounter += Time.deltaTime * TimeScale.timeScale;
            if ( InvincibilityCounter >= InvincibilityTime )
            {
                Invincible = false;
                isEvadeRecovering = true;
            }
        }

        if ( isEvadeRecovering )
        {
            //Debug.Log("Evade recovery");
            EvadeRecoveryCounter += Time.deltaTime * TimeScale.timeScale;
            if ( EvadeRecoveryCounter >= EvadeRecoveryTime )
            {
                isEvadeRecovering = false;
                isEvading = false;

                if ( EvadeEnqueued )
                {
                    EvadeEnqueued = false;
                    Evade();
                }
            }
        }
        #endregion

        #region Cloaking
        if ( IsCloaked )
        {
            Energy = Mathf.Max( Energy - CloakDrainPerSecond * Time.deltaTime * TimeScale.timeScale, 0.0f );
            if ( Energy <= 0.0f )
            {
                IsCloaked = false;
            }
        }
        #endregion

        #region Silencer
        silencer = Mathf.Min( silencer + silencerRegen * Time.deltaTime * TimeScale.timeScale, silencerMax );
        #endregion

        #region Energy
        IsEnergyRegenerating = true;

        if ( IsCloaked || isEvading ) { IsEnergyRegenerating = false; }
        //if ( IsShooting || IsAttacking || IsAssassinating ) { IsEnergyRegenerating = false; }

        if ( IsEnergyRegenerating )
        {
            Energy = Mathf.Min( Energy + ( EnergyMax / EnergyRegenerationTime ) * Time.deltaTime * TimeScale.timeScale, EnergyMax );
        }
        #endregion

		#region Walking
		if ( charStats.CurrentMoveState == CharEnums.MoveState.isWalking || charStats.CurrentMoveState == CharEnums.MoveState.isRunning )
		{
			walkAnimationTimer += Time.deltaTime; // t_scale SHOULD be respected?
			if ( walkAnimationTimer >= 0.35f )
			{
				walkAnimationTimer -= 0.35f;
				// make noise
				GameObject noiseObj = GameObject.Instantiate( NoisePrefab, this.gameObject.transform.position + new Vector3( 0.0f, -20.0f, 0.0f ), Quaternion.identity );
				Noise noise = noiseObj.GetComponent<Noise>();
				noise.lifetime = 0.2f; // seconds
				if ( charStats.CurrentMoveState == CharEnums.MoveState.isWalking )
				{
				  noise.radius = 25.0f;
				}
				else if ( charStats.CurrentMoveState == CharEnums.MoveState.isRunning )
				{
					noise.radius = 50.0f;
				}
			}
		}
		#endregion
        #endregion

        // Cheat codes
        // TODO: remove
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (AquiredMagGrip)
            {
                AquiredMagGrip = false;
            }
            else
            { 
                AquiredMagGrip = true;
            }
        }
    }
	
}
