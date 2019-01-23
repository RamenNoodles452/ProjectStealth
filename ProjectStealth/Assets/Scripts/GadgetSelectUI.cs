using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI to select the equipped gadget.
/// </summary>
public class GadgetSelectUI : MonoBehaviour
{
    #region vars
    private bool is_open = false;
    private float prev_time_scale = 1.0f;
    private IInputManager input_manager;
    private PlayerStats player_stats;

    private Dictionary<GadgetEnum,int> gadget_to_index_map;
    private GadgetEnum[] gadgets;
    private bool[] is_locked;
    private int selection_index = 0;
    #endregion

    /// <summary>
    /// Use this for early initialization.
    /// </summary>
    private void Awake()
    {
        gadget_to_index_map = new Dictionary<GadgetEnum, int>();
        gadgets = new GadgetEnum[ 4 ];
        is_locked = new bool[ 4 ];

        gadget_to_index_map.Add( GadgetEnum.Bomb, 0 );
        gadget_to_index_map.Add( GadgetEnum.ElectroMagneticPulse, 1 );
        gadget_to_index_map.Add( GadgetEnum.MagnetLink, 2 );
        gadget_to_index_map.Add( GadgetEnum.Cloak, 3 );

        gadgets[ gadget_to_index_map[ GadgetEnum.Bomb ] ] = GadgetEnum.Bomb;
        gadgets[ gadget_to_index_map[ GadgetEnum.ElectroMagneticPulse ] ] = GadgetEnum.ElectroMagneticPulse;
        gadgets[ gadget_to_index_map[ GadgetEnum.MagnetLink ] ] = GadgetEnum.MagnetLink;
        gadgets[ gadget_to_index_map[ GadgetEnum.Cloak ] ] = GadgetEnum.Cloak;
    }

    // Start is called before the first frame update
    void Start()
    {
        input_manager = Referencer.instance.player.GetComponent<IInputManager>();
        player_stats  = Referencer.instance.player.GetComponent<PlayerStats>();
        this.gameObject.SetActive( false );
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    /// <summary>
    /// Updates the locked or unlocked status of each gadget
    /// </summary>
    private void UpdateLocks()
    {
        is_locked[ gadget_to_index_map[ GadgetEnum.Bomb ] ] = player_stats.acquired_explosive;
        is_locked[ gadget_to_index_map[ GadgetEnum.ElectroMagneticPulse ] ] = player_stats.acquired_emp;
        is_locked[ gadget_to_index_map[ GadgetEnum.MagnetLink ] ] = player_stats.acquired_hookshot;
        is_locked[ gadget_to_index_map[ GadgetEnum.Cloak ] ] = false; // TODO:
    }

    /// <summary>
    /// Process user input.
    /// </summary>
    private void ProcessInput()
    {
        // Menu controls.
        if ( ! is_open ) { return; }

        if ( input_manager.GadgetInputReleaseInst )
        {
            // TODO: set gadget to currently selected gadget, if unlocked.
            Close();
            return;
        }

        if ( Mathf.Abs( input_manager.VerticalAxis ) < 0.1f && Mathf.Abs( input_manager.HorizontalAxis ) < 0.1f )
        {
            // Neutral input: Select the currently equipped gadget.
            selection_index = gadget_to_index_map[ player_stats.CurrentlyEquippedGadget ];
        }
        else
        {

            // make the selected gadget bigger and brighter, or something.
            selection_index = 0;
        }
    }

    /// <summary>
    /// Opens the selection UI.
    /// </summary>
    public void Open()
    {
        if ( is_open ) { return; }
        if ( NoGadgets() ) { return; } // COULD lock out if 1- gadgets.

        is_open = true;
        prev_time_scale = Time.timeScale;
        Time.timeScale = 0.0f;

        UpdateLocks();
        this.gameObject.SetActive( true );
    }

    /// <summary>
    /// Closes the selection UI.
    /// </summary>
    private void Close()
    {
        is_open = false;
        Time.timeScale = prev_time_scale;
        this.gameObject.SetActive( false );
    }

    /// <summary>
    /// Checks if the player only has no gadgets
    /// </summary>
    /// <returns>True if the player has no gadgets, False if they have more.</returns>
    private bool NoGadgets()
    {
        int count = 0;
        if ( player_stats.acquired_explosive ) { count++; }
        if ( player_stats.acquired_emp ) { count++; }
        if ( player_stats.acquired_hookshot ) { count++; }
        //if ( player_stats.acquired_cloak ) { count++; }

        return count > 0;
    }
}
