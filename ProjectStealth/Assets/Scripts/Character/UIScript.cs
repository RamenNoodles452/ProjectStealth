using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// - GabeV
// Player data tracking UI script
public class UIScript : MonoBehaviour
{
    #region vars
    private Player player;
    private PlayerStats player_stats;

    [SerializeField]
    private Text shield_value;
    [SerializeField]
    private Image shield_bar;
    [SerializeField]
    private Image shield_outline;
    //[SerializeField]
    //private Image[] shield_overlays;
    [SerializeField]
    private Image shield_overlay_cap;
    [SerializeField]
    private Image shield_loss;
    [SerializeField]
    private Image shield_loss_background;
    [SerializeField]
    private Image[] shield_underlays;
    [SerializeField]
    private Image shield_underlay_mask;
    private Color shield_bar_default_color          = new Color( 1.0f,  1.0f,  1.0f, 0.75f );
    private Color shield_bar_pulse_color            = new Color( 0.35f, 0.75f, 1.0f, 0.90f );
    private Color shield_bar_regen_pulse_color      = new Color( 0.25f, 0.5f,  1.0f, 0.95f );
    private Color shield_outline_default_color      = new Color( 0.0f,  0.5f,  1.0f, 0.125f );
    private Color shield_text_outline_default_color = new Color( 0.0f,  0.5f,  1.0f, 0.25f );

    [SerializeField]
    private Text energy_value;
    [SerializeField]
    private Image energy_bar;
    [SerializeField]
    private Image energy_bar_background;
    [SerializeField]
    private Image energy_outline;
    [SerializeField]
    private Image[] energy_overlays;
    [SerializeField]
    private Image energy_overlay_mask;
    [SerializeField]
    private Image energy_overlay_cap;
    [SerializeField]
    private Image energy_loss;
    [SerializeField]
    private Image energy_loss_background;
    private Color energy_bar_default_color = new Color( 0.0f, 0.70f, 0.35f, 0.5f );
    private Color energy_outline_default_color = new Color( 0.0f, 1.0f, 0.5f, 0.125f );
    private Color energy_value_outline_default_color = new Color( 0.0f, 1.0f, 0.5f, 0.25f );

    [SerializeField]
    private Image adrenal_rush_cooldown;
    [SerializeField]
    private Image adrenaline_slosh;
    [SerializeField]
    private Image adrenaline_outline;

    [SerializeField]
    private GameObject gadget_object;
    [SerializeField]
    private Image gadget_image;

    // Probable Debug UI (TODO: remove?)
    [SerializeField]
    private Text gadget_name;
    [SerializeField]
    private GameObject cloak_icon;
    [SerializeField]
    private GameObject evade_icon;
    [SerializeField]
    private GameObject shadow_icon;

    public bool hide_all;
    private const float BAR_LENGTH = 90.0f; // pixels

    private float energy_overlay_timer;
    private float shield_overlay_timer;
    private float shield_underlay_timer;
    private bool  is_energy_blinking;
    private bool is_adrenaline_blinking;
    private bool  is_shield_blinking;
    private bool  is_shield_text_blinking;
    private float energy_blink_timer;
    private float shield_blink_timer;
    private float adrenaline_blink_timer;
    private const float BLINK_DURATION = 0.5f; // seconds
    private float shield_pulse_timer;
    private float adrenaline_timer;

    private float energy_prev_frame;
    private float shield_prev_frame;
    private float energy_falling_value; // for loss
    private float shield_falling_value; // for loss
    private float energy_remembered_value;  // loss
    private float shield_remembered_value;  // loss
    #endregion

    // TODO: should probably be non-destroyable, instantiated on load

    private void Awake()
    {
    }

    // Use this for initialization
    void Start()
    {
        player = Referencer.instance.player; //GameObject.Find("PlayerCharacter").GetComponent<Player>(); //bad
        player_stats = Referencer.instance.player.GetComponent<PlayerStats>();
        energy_prev_frame = player.GetEnergyMax();
        energy_falling_value = energy_prev_frame;
        shield_prev_frame = player.GetShieldsMax();
        shield_falling_value = shield_prev_frame;
    }

    // Update is called once per frame
    void Update()
    {
        ShowText();
        ShowBasicBars();
        AnimateOverlays();
        BlinkOutlines();
        DropLosses();

        ShowGadget();
    }

    /// <summary>
    /// Handles displaying text (for counters, etc.)
    /// </summary>
    private void ShowText()
    {
        shield_value.text = ( (int) player.GetShields() ).ToString();
        if ( player_stats.IsAdrenalRushing ) { energy_value.text = "\u221E"; } // if adrenal rush is active, show infinite energy
        else { energy_value.text = ( (int) player.GetEnergy() ).ToString(); }

        // Debug
        #region cloak
        // Cloak
        if ( player.IsCloaking() )
        {
            cloak_icon.GetComponent<Text>().enabled = true;
        }
        else
        {
            cloak_icon.GetComponent<Text>().enabled = false;
        }
        #endregion

        #region evade
        // Evade
        if ( player.IsEvading() )
        {
            evade_icon.GetComponent<Text>().enabled = true;
        }
        else
        {
            evade_icon.GetComponent<Text>().enabled = false;
        }
        #endregion

        #region shadow
        // Shadow / Light
        if ( player.IsInShadow )
        {
            shadow_icon.GetComponent<Text>().enabled = true;
        }
        else
        {
            shadow_icon.GetComponent<Text>().enabled = false;
        }
        #endregion
    }

    /// <summary>
    /// Handle simple energy/shield bar display logic
    /// </summary>
    private void ShowBasicBars()
    {
        float shield_percent = player.GetShields() / player.GetShieldsMax();
        float energy_percent = player.GetEnergy() / player.GetEnergyMax();

        SetWidth( ref shield_bar, shield_percent * BAR_LENGTH );
        SetWidth( ref energy_bar, energy_percent * BAR_LENGTH );
        SetX( ref shield_overlay_cap, shield_bar.rectTransform.position.x + shield_percent * BAR_LENGTH - 16.0f );
        SetX( ref energy_overlay_cap, energy_bar.rectTransform.position.x + energy_percent * BAR_LENGTH - 16.0f );

        adrenal_rush_cooldown.fillAmount = player_stats.PercentAdrenalineCharge;
        adrenaline_slosh.rectTransform.position = new Vector3( adrenal_rush_cooldown.rectTransform.position.x,
            adrenal_rush_cooldown.rectTransform.position.y + player_stats.PercentAdrenalineCharge * adrenal_rush_cooldown.rectTransform.sizeDelta.y,
            0.0f );
    }

    /// <summary>
    /// Animate the overlay for the energy bar
    /// </summary>
    private void AnimateOverlays()
    {
        #region Energy
        // Energy
        float duration = 2.25f; // seconds for a full bar length (90px) animation cycle
        float speed = Time.deltaTime * Time.timeScale;
        if ( player_stats.IsAdrenalRushing ) { speed = speed * 16.0f; } // up animation speed while adrenal rush is active.

        AnimateOverlay( duration, speed, player.GetEnergy() / player.GetEnergyMax(), energy_bar.rectTransform.position.x, ref energy_overlay_timer,
            ref energy_overlays[ 0 ], ref energy_overlays[ 1 ], ref energy_overlay_mask );
        #endregion

        #region Health
        // Health
        duration = 4.5f; // varies from this to 1/4 this
        speed = Time.deltaTime * Time.timeScale * ( 1.0f + 3.0f * ( 1.0f - player.GetShields() / player.GetShieldsMax() ) );
        AnimateOverlay( duration, speed, player.GetShields() / player.GetShieldsMax(), shield_bar.rectTransform.position.x, ref shield_underlay_timer,
            ref shield_underlays[ 0 ], ref shield_underlays[ 1 ], ref shield_underlay_mask );
        SetWidth( ref shield_underlay_mask, BAR_LENGTH ); // never mask this.
        #endregion

        PulseShield();
    }

    /// <summary>
    /// Applies a scrolling animated overlay to a bar's fill.
    /// </summary>
    /// <param name="duration">The duration of a full animation cycle.</param>
    /// <param name="speed">The speed at which the timer fills. Typically, should be delta time.</param>
    /// <param name="fill_percent">How full the bar is, as a percentage (0.0f : 1.0f)</param>
    /// <param name="left">The x coordinate of the left side of the bar</param>
    /// <param name="timer">The timer to update</param>
    /// <param name="overlay_right">The image object that will overlay from the right</param>
    /// <param name="overlay_left"> The image object that will overlay from the left</param>
    /// <param name="mask">The image object of the mask, which hides overlay animation that exceeds the bar's fill</param>
    private void AnimateOverlay( float duration, float speed, float fill_percent, float left, ref float timer, ref Image overlay_right, ref Image overlay_left, ref Image mask )
    {
        timer += speed;
        while ( timer >= duration ) { timer -= duration; } // if->while Just in case something odd happens
        float x = timer * BAR_LENGTH / duration; // {0,BAR_LENGTH} x coordinate where the two pieces join
        float max_x = fill_percent * BAR_LENGTH; // do not allow either piece to go beyond this coordinate.

        // position is (-90:0), fills from (0:position+90) from the right
        SetX( ref overlay_right, left + (int) x - BAR_LENGTH );
        overlay_right.fillAmount = ( (int) x ) / BAR_LENGTH;

        // position is (0:90), fills from (position:90) from the left
        SetX( ref overlay_left, left + (int) x );
        overlay_left.fillAmount = ( BAR_LENGTH - (int) x ) / BAR_LENGTH;

        // Use a mask to prevent the overlay from extending into the empty portion of the bar.
        SetWidth( ref mask, max_x );
    }

    /// <summary>
    /// Makes the shield bar pulse, and pulse more obviously while it regenerates
    /// </summary>
    private void PulseShield()
    {
        float duration = 2.0f; // pulse cycle length, in seconds
        shield_pulse_timer += Time.deltaTime * Time.timeScale; //TODO: cache this common time operation, save dozens of mults
        while ( shield_pulse_timer >= duration ) { shield_pulse_timer -= duration; }

        if ( player_stats.IsRegenerating )
        {
            float t = Mathf.PingPong( shield_pulse_timer * 16.0f / duration, 1.0f ); // 8x as fast, and brighter
            shield_bar.color = Blend( shield_bar_default_color, shield_bar_regen_pulse_color, t );
        }
        else // default
        {
            float t = Mathf.PingPong( shield_pulse_timer * 2.0f / duration, 1.0f );
            shield_bar.color = Blend( shield_bar_default_color, shield_bar_pulse_color, t );
            //shield_bar.color = shield_bar_default_color;
        }
    }

    #region Blink
    /// <summary>
    /// Handles making outlines of bars + text blink
    /// </summary>
    private void BlinkOutlines()
    {
        #region Energy
        // Energy bar
        if ( is_energy_blinking )
        {
            energy_blink_timer += Time.deltaTime * Time.timeScale;
            float duration = BLINK_DURATION; // seconds
            BlinkRed( energy_blink_timer, duration, 3.0f, energy_outline_default_color , ref energy_outline );

            // state
            if ( energy_blink_timer >= duration )
            {
                is_energy_blinking = false;
            }
        }
        else if ( player_stats.IsAdrenalRushing )
        {
            // super mode effects
            AdrenalineMode();
        }
        else // default
        {
            energy_bar.color = energy_bar_default_color;
            energy_outline.color = energy_outline_default_color;
            energy_value.fontSize = 22;
            energy_value.GetComponent<Outline>().effectColor = energy_value_outline_default_color;
        }
        #endregion

        #region Shield
        // Shield bar
        if ( is_shield_blinking )
        {
            // blink red
            shield_blink_timer += Time.deltaTime * Time.timeScale;
            float duration = BLINK_DURATION; // seconds
            BlinkRed( shield_blink_timer, duration, 3.0f, shield_outline_default_color, ref shield_outline );
            if ( is_shield_text_blinking ) // text, too?
            {
                Outline outline = shield_value.GetComponent<Outline>();
                BlinkRed( shield_blink_timer, duration, 3.0f, shield_text_outline_default_color, ref outline );
            }

            // state
            if ( shield_blink_timer >= duration )
            {
                is_shield_blinking = false;
                is_shield_text_blinking = false;
            }
        }
        else // default
        {
            shield_outline.color = shield_outline_default_color;
        }
        #endregion

        #region Adrenaline
        // Adrenaline
        if ( player_stats.acquired_adrenal_rush )
        {
            adrenal_rush_cooldown.transform.parent.gameObject.SetActive( true ); // excessive?

            adrenaline_timer += Time.deltaTime * Time.timeScale;
            while ( adrenaline_timer > 1.0f ) { adrenaline_timer -= 1.0f; }

            if ( player_stats.PercentAdrenalineCharge >= 1.0f || player_stats.IsAdrenalRushing )
            {
                // pulse and flicker when ready or active
                adrenaline_outline.color = new Color( 0.0f, 1.0f, 0.35f, Random.Range( 0.25f, 0.5f ) * ( 0.8f * player_stats.PercentAdrenalineCharge + 0.2f ) );
                adrenal_rush_cooldown.color = new Color( 0.0f, 1.0f, 0.5f, 0.65f + 0.34f * Mathf.Sin( adrenaline_timer * Mathf.PI * 2.0f ) );
            }
            else if ( is_adrenaline_blinking )
            {
                // blink red
                adrenaline_blink_timer += Time.deltaTime * Time.timeScale;
                float duration = BLINK_DURATION; // seconds

                BlinkRed( adrenaline_blink_timer, duration, 3.0f, new Color( 0.0f, 1.0f, 0.5f, 0.1f ), ref adrenaline_outline );

                // state
                if ( adrenaline_blink_timer >= duration )
                {
                    is_adrenaline_blinking = false;
                }
            }
            else
            {
                // default
                adrenaline_outline.color = new Color( 0.0f, 1.0f, 0.5f, 0.0f );
                adrenal_rush_cooldown.color = new Color( 0.0f, 1.0f, 0.5f, 0.5f );
            }
        }
        else // don't show adrenal rush UI if you haven't unlocked it.
        {
            adrenal_rush_cooldown.transform.parent.gameObject.SetActive( false );
        }
        #endregion
    }

    /// <summary>
    /// Makes a UI element blink red
    /// </summary>
    /// <param name="timer">The timer's value {0:duration}</param>
    /// <param name="duration">The duration of the blink effect, in seconds</param>
    /// <param name="count">The number of blink cycles to perform in the duration</param>
    /// <param name="base_color">The (not red) base color of the blinking element</param>
    /// <param name="outline">The UI element to blink</param>
    private void BlinkRed( float timer, float duration, float count, Color base_color, ref Image outline )
    {
        outline.color = BlinkColor( timer, duration,count, base_color );
    }

    /// <summary>
    /// Makes a UI element blink red.
    /// </summary>
    /// <param name="timer">The timer's value {0:duration}</param>
    /// <param name="duration">The duration of the blink effect, in seconds</param>
    /// <param name="count">The number of blink cycles to perform in the duration</param>
    /// <param name="base_color">The (not red) base color of the blinking element</param>
    /// <param name="outline">The UI element to blink</param>
    private void BlinkRed( float timer, float duration, float count, Color base_color, ref Outline outline )
    {
        outline.effectColor = BlinkColor( timer, duration, count, base_color );
    }

    /// <summary>
    /// Gets the color for a blinking red UI element
    /// </summary>
    /// <param name="timer">The timer's value {0:duration}</param>
    /// <param name="duration">The duration of the blink effect, in seconds</param>
    /// <param name="count">The number of blink cycles to perform in the duration</param>
    /// <param name="base_color">The (not red) base color of the blinking element</param>
    /// <returns>The appropriate blended color between base and red</returns>
    private Color BlinkColor( float timer, float duration, float count, Color base_color )
    {
        float weight = Mathf.PingPong( timer * count * 2.0f / duration, 1.0f );
        Color red = new Color( 1.0f, 0.0f, 0.0f, 0.80f);
        return Blend( base_color, red, weight );
    }
    #endregion

    /// <summary>
    /// Handles special display logic for adrenaline rush mode
    /// </summary>
    private void AdrenalineMode()
    {
        // Flicker
        energy_bar.color = new Color( 0.0f, 1.0f, 0.5f, Random.Range( 0.5f, 0.6f ) );
        energy_outline.color = new Color( 0.0f, 1.0f, 0.35f, Random.Range( 0.25f, 0.5f ) );
        energy_value.GetComponent<Outline>().effectColor = new Color( 0.0f, 1.0f, 0.5f, Random.Range( 0.25f, 0.5f ) );
        energy_value.fontSize = 38; // embiggen
    }

    /// <summary>
    /// Reduce the size of the "dropped recently" bar
    /// </summary>
    private void DropLosses()
    {
        #region Energy
        // Energy
        if ( energy_prev_frame >= player.GetEnergy() && energy_prev_frame >= energy_falling_value )
        {
            energy_falling_value = energy_prev_frame;
            energy_remembered_value = energy_prev_frame;
        }

        float x = energy_bar.rectTransform.position.x;
        DropLoss( energy_falling_value, energy_remembered_value, player_stats.GetEnergy(), player_stats.GetEnergyMax(), x, ref energy_loss, ref energy_loss_background );
        #endregion

        #region Shield
        // Shield
        if ( shield_prev_frame >= player.GetShields() && shield_prev_frame >= shield_falling_value )
        {
            shield_falling_value = shield_prev_frame;
            shield_remembered_value = shield_prev_frame;
        }

        x = shield_bar.rectTransform.position.x;
        DropLoss( shield_falling_value, shield_remembered_value, player_stats.GetShields(), player_stats.GetShieldsMax(), x, ref shield_loss, ref shield_loss_background );
        #endregion

        UpdateLossValues();
    }

    /// <summary>
    /// When a bar's value is reduced, this displays an indicator of the amount of the hit, and
    /// a secondary indicator that falls to the current value the bar is tracking.
    /// </summary>
    /// <param name="falling_value">The value of the falling indicator</param>
    /// <param name="remembered_value">The value the loss is falling from</param>
    /// <param name="current_value">The actual value</param>
    /// <param name="maximum_value">The maximum value</param>
    /// <param name="x">The x coordinate of the beginning of the corresponding resource bar</param>
    /// <param name="loss_bar">The image object for the falling indicator</param>
    /// <param name="loss_background_bar">The image object for the old high value that has since been lost</param>
    private void DropLoss( float falling_value, float remembered_value, float current_value, float maximum_value, float x, ref Image loss_bar, ref Image loss_background_bar )
    {
        if ( falling_value >= current_value )
        {
            float percent = current_value / maximum_value;
            float start_x = x + percent * BAR_LENGTH;
            SetX( ref loss_bar, start_x );
            SetX( ref loss_background_bar, start_x );
            SetWidth( ref loss_bar, ( falling_value - current_value ) / maximum_value * BAR_LENGTH );
            SetWidth( ref loss_background_bar, ( remembered_value - current_value ) / maximum_value * BAR_LENGTH );
        }
        else // hide
        {
            SetWidth( ref loss_bar, 0.0f );
            SetWidth( ref loss_background_bar, 0.0f );
        }
    }

    /// <summary>
    /// Causes the red loss zone in UI bars to deplete.
    /// </summary>
    private void UpdateLossValues()
    {
        float drop_speed = 180.0f; // pixels per second
        energy_falling_value -= drop_speed * Time.deltaTime * Time.timeScale;
        shield_falling_value -= drop_speed * Time.deltaTime * Time.timeScale;
        energy_prev_frame = player.GetEnergy();
        shield_prev_frame = player.GetShields();
    }

    /// <summary>
    /// Displays the gadget icon.
    /// </summary>
    private void ShowGadget()
    {
        GadgetSelectUI gadget_ui = Referencer.instance.gadget_select_ui;
        if ( gadget_ui != null )
        {
            Sprite sprite = gadget_ui.GetGadgetSprite( player_stats.CurrentlyEquippedGadget );
            if ( sprite != null ) // show the gadget UI and icon.
            {
                if ( ! gadget_object.activeInHierarchy )
                {
                    gadget_object.SetActive( true );
                }
                gadget_image.sprite = sprite;
            }
            else // no gadget icon, hide entire gadget portion of the UI.
            {
                if ( gadget_object.activeInHierarchy )
                {
                    gadget_object.SetActive( false );
                }
            }
        }
    }

    #region Utility
    /// <summary>
    /// Blends two colors along RGBA.
    /// (TODO: use HSV? It goes pretty fast, so maybe no need)
    /// Utility function.
    /// </summary>
    /// <param name="a">the starting color</param>
    /// <param name="b">the other color</param>
    /// <param name="t">weight factor between 0 and 1. 0 = color a, 1 = color b, 0.5f = halfway between both</param>
    /// <returns></returns>
    private Color Blend( Color a, Color b, float t )
    {
        return new Color( a.r * ( 1.0f - t ) + b.r * t,
                          a.g * ( 1.0f - t ) + b.g * t,
                          a.b * ( 1.0f - t ) + b.b * t,
                          a.a * ( 1.0f - t ) + b.a * t );
    }

    /// <summary>
    /// Sets the width of an image, while preserving its height.
    /// Utility function.
    /// </summary>
    /// <param name="image">The image to resize</param>
    /// <param name="width">The width to give the image</param>
    private void SetWidth( ref Image image, float width )
    {
        image.rectTransform.sizeDelta = new Vector2( width, image.rectTransform.sizeDelta.y );
    }

    /// <summary>
    /// Sets the x coordinate of the image, while preserving its y coordinate.
    /// Utility function.
    /// </summary>
    /// <param name="image">The image to move</param>
    /// <param name="x">The x coordinate to give the image</param>
    private void SetX( ref Image image, float x )
    {
        SetPosition( ref image, x, image.rectTransform.position.y );
    }

    /// <summary>
    /// Sets the coordinates of an image.
    /// Utility function.
    /// </summary>
    /// <param name="image">The image to move</param>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    private void SetPosition( ref Image image, float x, float y )
    {
        image.rectTransform.position = new Vector3( x, y, 0.0f );
    }
    #endregion

    #region event hooks
    /// <summary>
    /// Causes the energy bar to blink red when you don't have enough stamina.
    /// </summary>
    public void InsuffienctStamina()
    {
        is_energy_blinking = true;
        energy_blink_timer = 0.0f;
        //Referencer.instance.player.GetComponent<AudioSource>().PlayOneShot();
    }

    /// <summary>
    /// Causes the adrenaline gauge to blink red when you don't have enough adrenaline.
    /// </summary>
    public void InsufficientAdrenaline()
    {
        is_adrenaline_blinking = true;
        adrenaline_blink_timer = 0.0f;
        //Referencer.instance.player.GetComponent<AudioSource>().PlayOneShot();
    }

    /// <summary>
    /// Causes shield bar to freak out when you get hit.
    /// </summary>
    public void OnHit()
    {
        is_shield_blinking = true;
        is_shield_text_blinking = true;
        shield_blink_timer = 0.0f;
    }

    /// <summary>
    /// Causes the shield bar to super freak out and make noise because one more hit and you're dead.
    /// </summary>
    public void OnShieldBreak()
    {
        is_shield_blinking = true;
        is_shield_text_blinking = true;
        shield_blink_timer = -1.0f * player_stats.RegenerationDelay + BLINK_DURATION; // extends the blink duration
        //Referencer.instance.player.GetComponent<AudioSource>().PlayOneShot();
    }
    #endregion
}
