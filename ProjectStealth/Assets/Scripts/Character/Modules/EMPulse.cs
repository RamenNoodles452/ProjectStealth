using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates an EMP shockwave centered on the player.
/// </summary>
public class EMPulse : MonoBehaviour
{
    #region vars
    [SerializeField]
    private GameObject EMP_prefab;
    private PlayerStats player_stats;
    private IInputManager input_manager;
    private float timer;
    private bool is_active;

    private const float ENERGY_COST = 50.0f;
    #endregion

    /// <summary>
    /// Use this for early initialization (references)
    /// </summary>
    private void Awake()
    {
        input_manager = this.gameObject.GetComponent<IInputManager>();
        player_stats  = this.gameObject.GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if ( ! player_stats.acquired_emp ) { return; }

        UpdateTimers(); // Put first so the timer is accurate.
    }

    /// <summary>
    /// Counts down the wind-up animation.
    /// </summary>
    private void UpdateTimers()
    {
        timer = Mathf.Max( 0.0f, timer - Time.deltaTime * Time.timeScale );
        if ( timer <= 0.0f && is_active )
        {
            // Instantiate EMP prefab.
            GameObject EMP_object = GameObject.Instantiate( EMP_prefab, transform.position, Quaternion.identity );
            EMP EMP_script = EMP_object.GetComponent<EMP>();
            EMP_script.radius = PulseRadius();
            is_active = false;
        }

        // TODO: interrupts (deactivate) + energy refund? (req. adrenaline/not at activation tracking)
    }

    /// <summary>
    /// Called remotely to activate this gadget.
    /// </summary>
    public void Trigger()
    {
        if ( ! player_stats.acquired_emp ) { return; }
        if ( is_active ) { return; } // already active.

        // Energy cost
        if ( ! player_stats.IsAdrenalRushing )
        {
            if ( player_stats.GetEnergy() < ENERGY_COST )
            {
                Referencer.instance.hud_ui.InsuffienctStamina();
                // TODO: play sound?
                return;
            }
            player_stats.SpendEnergy( ENERGY_COST );
        }

        is_active = true;
        timer = WindUpDelay();

        // TODO: animation
    }

    /// <returns>The radius of the EMP, in pixels.</returns>
    private float PulseRadius()
    {
        // Provided as a function in case we want to check upgrades.
        return 128.0f;
    }

    /// <returns>The duration of the wind-up, in seconds.</returns>
    private float WindUpDelay()
    {
        // Provided as a function in case we want to check upgrades.
        return 0.5f;
    }
}
