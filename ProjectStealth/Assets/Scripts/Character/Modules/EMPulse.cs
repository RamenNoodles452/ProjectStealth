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
    #endregion

    /// <summary>
    /// Use this for early initialization (references)
    /// </summary>
    private void Awake()
    {
        input_manager = this.gameObject.GetComponent<IInputManager>();
        player_stats  = this.gameObject.GetComponent<PlayerStats>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ( ! player_stats.acquired_emp ) { return; }

        if ( input_manager.GadgetInputInst && player_stats.gadget == GadgetEnum.ElectroMagneticPulse )
        {

        }
    }
}
