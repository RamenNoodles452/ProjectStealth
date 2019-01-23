using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI to select the equipped gadget.
/// </summary>
public class GadgetSelectUI : MonoBehaviour
{
    #region vars
    [SerializeField]
    private Sprite locked_sprite;
    [SerializeField]
    private Sprite bomb_sprite;
    [SerializeField]
    private Sprite emp_sprite;
    [SerializeField]
    private Sprite mag_link_sprite;
    [SerializeField]
    private Sprite cloak_sprite;
    [SerializeField]
    private Sprite hollow_hex;
    [SerializeField]
    private Sprite filled_hex;

    [SerializeField]
    private Image bomb_image;
    [SerializeField]
    private Image emp_image;
    [SerializeField]
    private Image mag_link_image;
    [SerializeField]
    private Image cloak_image;

    [SerializeField]
    private Image bomb_hex;
    [SerializeField]
    private Image emp_hex;
    [SerializeField]
    private Image mag_link_hex;
    [SerializeField]
    private Image cloak_hex;

    [SerializeField]
    private Text gadget_name;
    [SerializeField]
    private Text gadget_description;

    private bool is_open = false;
    private float prev_time_scale = 1.0f;
    private IInputManager input_manager;
    private PlayerStats player_stats;

    private Dictionary<GadgetEnum,int> gadget_to_index_map;
    private GadgetEnum[] gadgets; // reverse lookup
    private bool[] is_locked;
    private int selection_index = 0;
    private int previous_selection_index = 0;
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
        previous_selection_index = selection_index;
    }

    /// <summary>
    /// Updates the locked or unlocked status of each gadget
    /// </summary>
    private void UpdateLocks()
    {
        is_locked[ gadget_to_index_map[ GadgetEnum.Bomb ] ] = ! player_stats.acquired_explosive;
        is_locked[ gadget_to_index_map[ GadgetEnum.ElectroMagneticPulse ] ] = ! player_stats.acquired_emp;
        is_locked[ gadget_to_index_map[ GadgetEnum.MagnetLink ] ] = ! player_stats.acquired_hookshot;
        is_locked[ gadget_to_index_map[ GadgetEnum.Cloak ] ] = ! player_stats.acquired_cloak;
    }

    /// <summary>
    /// Sets the icons for gadgets based on their locked / unlocked status.
    /// </summary>
    private void UpdateIcons()
    {
        if ( ! is_locked[ gadget_to_index_map[ GadgetEnum.Bomb ] ] )
        {
            bomb_image.sprite = bomb_sprite;
        }
        else
        {
            bomb_image.sprite = locked_sprite;
        }

        if ( ! is_locked[ gadget_to_index_map[ GadgetEnum.MagnetLink ] ] )
        {
            mag_link_image.sprite = mag_link_sprite;
        }
        else
        {
            mag_link_image.sprite = locked_sprite;
        }

        if ( ! is_locked[ gadget_to_index_map[ GadgetEnum.ElectroMagneticPulse ] ] )
        {
            emp_image.sprite = emp_sprite;
        }
        else
        {
            emp_image.sprite = locked_sprite;
        }

        if ( ! is_locked[ gadget_to_index_map[ GadgetEnum.Cloak ] ] )
        {
            cloak_image.sprite = cloak_sprite;
        }
        else
        {
            cloak_image.sprite = locked_sprite;
        }
    }

    /// <summary>
    /// Gets the icon image object associated with a gadget.
    /// </summary>
    /// <param name="gadget">The gadget to look up the icon for</param>
    /// <returns>The image object of the icon</returns>
    private Image GetGadgetIcon( GadgetEnum gadget )
    {
        if ( gadget == GadgetEnum.Bomb ) { return bomb_image; }
        if ( gadget == GadgetEnum.ElectroMagneticPulse ) { return emp_image; }
        if ( gadget == GadgetEnum.MagnetLink ) { return mag_link_image; }
        if ( gadget == GadgetEnum.Cloak ) { return cloak_image; }
        return null;
    }

    /// <summary>
    /// Gets the hex image object associated with a gadget.
    /// </summary>
    /// <param name="gadget">The gadget to look up the icon for</param>
    /// <returns>The image object of the hex</returns>
    private Image GetGadgetHex( GadgetEnum gadget )
    {
        if ( gadget == GadgetEnum.Bomb ) { return bomb_hex; }
        if ( gadget == GadgetEnum.ElectroMagneticPulse ) { return emp_hex; }
        if ( gadget == GadgetEnum.MagnetLink ) { return mag_link_hex; }
        if ( gadget == GadgetEnum.Cloak ) { return cloak_hex; }
        return null;
    }

    /// <summary>
    /// Gets the name of the gadger.
    /// </summary>
    /// <param name="gadget">The gadget to get the name for.</param>
    /// <returns>The gadget's name.</returns>
    private string GetGadgetName( GadgetEnum gadget )
    {
        if ( is_locked[ gadget_to_index_map[ gadget ] ] ) { return "???"; }

        if ( gadget == GadgetEnum.Bomb ) { return "Bomb"; }
        if ( gadget == GadgetEnum.ElectroMagneticPulse ) { return "E. M. P."; }
        if ( gadget == GadgetEnum.MagnetLink ) { return "Magnetic Link"; }
        if ( gadget == GadgetEnum.Cloak ) { return "Tactical Cloak"; }
        return "???";
    }

    /// <summary>
    /// Gets the description for the gadget.
    /// </summary>
    /// <param name="gadget">The gadget you want the description of.</param>
    /// <returns>The gadget's description.</returns>
    private string GetGadgetDescription( GadgetEnum gadget )
    {
        if ( is_locked[ gadget_to_index_map[ gadget ] ] ) { return "No information available."; }

        if ( gadget == GadgetEnum.Bomb )
        {
            return "Deploys a sticky explosive charge which can be detonated remotely.";
        }
        if ( gadget == GadgetEnum.ElectroMagneticPulse )
        {
            return "Discharges a shockwave that will take nearby electronics offline temporarily.";
        }
        if ( gadget == GadgetEnum.MagnetLink )
        {
            return "Pulls the user to a targeted magnetic surface.";
        }
        if ( gadget == GadgetEnum.Cloak )
        {
            return "Renders the wearer virtually invisible. Requires an immense amount of energy.";
        }

        return "No information available.";
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
            // Set gadget to currently selected gadget, if unlocked.
            if ( ! is_locked[ selection_index ] )
            {
                player_stats.CurrentlyEquippedGadget = gadgets[ selection_index ];
            }

            Close();
            return;
        }

        if ( Mathf.Abs( input_manager.VerticalAxis ) < 0.1f && Mathf.Abs( input_manager.HorizontalAxis ) < 0.1f )
        {
            // Neutral input: just maintain current state. Seems to work better than resetting, as it's more tolerant.
            // Neutral input: Select the currently equipped gadget.
            //selection_index = gadget_to_index_map[ player_stats.CurrentlyEquippedGadget ];
            //VisuallySelect( false );
        }
        else
        {
            // Figure out which gadget we selected.
            float selection_count = 4.0f; // number of choices.
            float angle = Mathf.Atan2( input_manager.VerticalAxis, input_manager.HorizontalAxis ) * Mathf.Rad2Deg;
            angle = angle % 360.0f;
            angle += 360.0f / ( selection_count * 2.0f );
            while ( angle < 0.0f ) { angle += 360.0f; }
            selection_index = (int) Mathf.Floor( angle / ( 360.0f / selection_count ) );
            VisuallySelect( false );
        }
    }

    /// <summary>
    /// Makes the UI show the currently selected gadget as selected.
    /// </summary>
    /// <param name="force_refresh">If true, visuals will be forced to be updated, even if no change was detected.</param>
    private void VisuallySelect( bool force_refresh )
    {
        if ( selection_index == previous_selection_index && ! force_refresh ) { return; }

        // Reset non-selected hexes.
        bomb_hex.sprite = hollow_hex;
        emp_hex.sprite = hollow_hex;
        mag_link_hex.sprite = hollow_hex;
        cloak_hex.sprite = hollow_hex;

        // Make the selected gadget brighter.
        GadgetEnum gadget = gadgets[ selection_index ];
        Image icon = GetGadgetIcon( gadget );
        Image hex  = GetGadgetHex(  gadget );
        gadget_name.text = GetGadgetName( gadget );
        gadget_description.text = GetGadgetDescription( gadget );
        hex.sprite = filled_hex;
    }

    /// <summary>
    /// Opens the selection UI.
    /// </summary>
    public void Open()
    {
        if ( is_open ) { return; }
        if ( GadgetCount() <= 0 ) { return; } // COULD lock out if 1- gadgets.

        is_open = true;
        prev_time_scale = Time.timeScale;
        Time.timeScale = 0.0f;

        UpdateLocks();
        UpdateIcons();
        selection_index = gadget_to_index_map[ player_stats.CurrentlyEquippedGadget ];
        previous_selection_index = selection_index;
        VisuallySelect( true );

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
    /// Gets the number of gadgets the player possesses.
    /// </summary>
    /// <returns>The number of gadgets the player possesses.</returns>
    private int GadgetCount()
    {
        int count = 0;
        if ( player_stats.acquired_explosive ) { count++; }
        if ( player_stats.acquired_emp ) { count++; }
        if ( player_stats.acquired_hookshot ) { count++; }
        if ( player_stats.acquired_cloak ) { count++; }

        return count;
    }
}
