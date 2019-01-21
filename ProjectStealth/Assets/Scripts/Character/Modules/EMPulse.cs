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

    private const float ENERGY_COST = 75.0f;
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

        if ( input_manager.GadgetInputInst && player_stats.gadget == GadgetEnum.ElectroMagneticPulse )
        {
            // Energy cost
            if ( ! player_stats.IsAdrenalRushing )
            {
                if ( player_stats.GetEnergy() < ENERGY_COST ) { return; }
                player_stats.SpendEnergy( ENERGY_COST );
            }

            // Instantiate EMP prefab.
            GameObject EMP_object = GameObject.Instantiate( EMP_prefab, transform.position, Quaternion.identity );
            EMP EMP_script = EMP_object.GetComponent<EMP>();
            EMP_script.radius = 96.0f;

            // TODO: animation
        }
    }
}
