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

    #region Evade
    private bool EvadeEnqueued = false;
    public float EvadeCost = 20.0f;

    private bool IsEvading = false;

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

    // progress values
    public bool AquiredMagGrip;

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

    /// <summary>
    /// Evades
    /// </summary>
    public void Evade()
    {
        if ( IsEvading )
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

        IsEvading = true;
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
        EvadeEnqueued = false;
        Invincible = false;
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
                IsEvading = false;

                if ( EvadeEnqueued )
                {
                    EvadeEnqueued = false;
                    Evade();
                }
            }
        }
        #endregion
        #endregion

        // Cheat codes
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (AquiredMagGrip)
                AquiredMagGrip = false;
            else
                AquiredMagGrip = true;
        }
    }
	
}
