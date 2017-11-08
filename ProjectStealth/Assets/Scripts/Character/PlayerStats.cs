using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    // random values
    public float Health;
    public float HealthMax = 1.0f;

    #region shield
    public float Shield;
    public float ShieldMax = 10.0f;

    private bool WasHitThisFrame;
    private bool IsRegenerating;
    private float ShieldRegenerationDelay = 2.0f;
    private float ShieldDelayCounter = 0.0f;
    private float ShieldRegenerationTime = 4.0f; // 25% per second
    #endregion

    public float Energy = 100.0f;
    public float EnergyMax = 100.0f;

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
    #endregion

    #region cloak
    private bool IsCloaked = false;
    private const float CloakCost = 5.0f;
    private const float CloakDrainPerSecond = 10.0f; //9.5s
    #endregion

    // progress values
    public bool AquiredMagGrip;

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
            }
        }
    }

    public void Shoot()
    {
        if ( isEvading ) { return; } // no shooting mid-evade
        //animation lock checks?

        //make noise?
        //use silencer meter / shooting when cloaked makes no noise

        if ( IsCloaked ) { IsCloaked = false; } // attacking breaks stealth
    }

    public void Attack()
    {
        if ( isEvading ) { return; } // no attacking mid-evade
        //animation lock checks?

        if ( IsCloaked ) { IsCloaked = false; } // attacking breaks stealth

        //enqueueing? + comboing
        //tag enemies for auto aim
    }

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
        if ( IsCloaked ) { return; }

        // check if unlocked
        // animation lock checks?

        if ( Energy >= CloakCost )
        {
            Energy = Energy - CloakCost;
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

    void Start()
    {
        // Reset state to prevent bugs when changing levels
        EvadeEnqueued = false;
        Invincible = false;
        IsCloaked = false;
        WasHitThisFrame = false;

        Health = HealthMax;
        Shield = ShieldMax;
    }

    void Update()
    {
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
        #endregion

        // Cheat codes
        // TODO: remove
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (AquiredMagGrip)
                AquiredMagGrip = false;
            else
                AquiredMagGrip = true;
        }
    }
	
}
