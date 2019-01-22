﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows the player to hack certain interactable objects.
/// </summary>
public class Hacker : MonoBehaviour
{
    #region vars
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
        if ( ! player_stats.acquired_hack ) { return; }

        if ( input_manager.GadgetInputInst && player_stats.gadget == GadgetEnum.Hacker )
        {

        }
    }
}