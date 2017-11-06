using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    // random values
    public float Health;
    public float HealthMax = 1.0f;

    public float Shield;
    public float ShieldMax = 10.0f;

    private bool WasHitThisFrame;
    private bool IsRegenerating;
    private float ShieldRegenerationDelay = 2.0f;
    private float ShieldDelayCounter = 0.0f;
    private float ShieldRegenerationTime = 4.0f; // 25% per second

    // progress values
    public bool AquiredMagGrip;

    public void Hit( float damage )
    {
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

    void Start()
    {
        WasHitThisFrame = false;
        Health = HealthMax;
        Shield = ShieldMax;
    }

    void Update()
    {
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
