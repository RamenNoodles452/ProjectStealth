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
    private Image energy_outline;
    [SerializeField]
    private Image[] energy_overlays;
    [SerializeField]
    private Image energy_overlay_mask;
    [SerializeField]
    private Image energy_overlay_cap;
    [SerializeField]
    private Image energy_loss;
    private Color energy_outline_default_color = new Color( 0.0f, 1.0f, 0.5f, 0.125f );

    [SerializeField]
    private Image adrenal_rush_cooldown;

    // Probable Debug UI (TODO: remove?)
    [SerializeField]
    private Text gadget_name;
    [SerializeField]
    private GameObject cloak_icon;
    [SerializeField]
    private GameObject evade_icon;

    public bool hide_all;
    private const float BAR_LENGTH = 90.0f; // pixels

    private float energy_overlay_timer;
    private float shield_overlay_timer;
    private bool  is_energy_blinking;
    private bool  is_shield_blinking;
    private bool  is_shield_text_blinking;
    private float energy_blink_timer;
    private float shield_blink_timer;
    private const float BLINK_DURATION = 0.5f; // seconds
    private float shield_pulse_timer;

    private float energy_prev_frame;
    private float shield_prev_frame;
    private float energy_remembered_value; // for loss
    private float shield_remembered_value; // for loss
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
        energy_remembered_value = energy_prev_frame;
        shield_prev_frame = player.GetShieldsMax();
        shield_remembered_value = shield_prev_frame;
    }

    // Update is called once per frame
    void Update()
    {
        shield_value.text = ( (int) player.GetShields() ).ToString(); //+ " / " + (int)player.GetShieldsMax();
        if ( false ) { energy_value.text = "\u221E"; } // if adrenal rush is active, show infinite energy
        energy_value.text = ( (int) player.GetEnergy()  ).ToString(); //+ " / " + (int)player.GetEnergyMax();

        ShowBasicBars();
        AnimateOverlays();
        BlinkOutlines();
        DropLosses();

        // Debug
        if ( player.IsCloaking() )
        {
            cloak_icon.GetComponent<Text>().enabled = true;
        }
        else
        {
            cloak_icon.GetComponent<Text>().enabled = false;
        }

        if ( player.IsEvading())
        {
            evade_icon.GetComponent<Text>().enabled = true;
        }
        else
        {
            evade_icon.GetComponent<Text>().enabled = false;
        }
    }

    /// <summary>
    /// Handle simple energy/shield bar display logic
    /// </summary>
    private void ShowBasicBars()
    {
        float shield_percent = player.GetShields() / player.GetShieldsMax();
        float energy_percent = player.GetEnergy() / player.GetEnergyMax();

        shield_bar.rectTransform.sizeDelta = new Vector2( shield_percent * BAR_LENGTH, shield_bar.rectTransform.sizeDelta.y );
        energy_bar.rectTransform.sizeDelta = new Vector2( energy_percent * BAR_LENGTH, energy_bar.rectTransform.sizeDelta.y );
        shield_overlay_cap.rectTransform.position = new Vector3( shield_bar.rectTransform.position.x + shield_percent * BAR_LENGTH - 16.0f, shield_bar.rectTransform.position.y, 0.0f );
        energy_overlay_cap.rectTransform.position = new Vector3( energy_bar.rectTransform.position.x + energy_percent * BAR_LENGTH - 16.0f, energy_bar.rectTransform.position.y, 0.0f );
    }

    /// <summary>
    /// Animate the overlay for the energy bar
    /// </summary>
    private void AnimateOverlays()
    {
        float duration = 2.25f; // seconds for a full bar length (90px) animation cycle
        float speed = Time.deltaTime * Time.timeScale;
        if ( false ) { speed = speed * 2.0f; } // up animation speed while adrenal rush is active.

        energy_overlay_timer += speed;
        while ( energy_overlay_timer >= duration ) { energy_overlay_timer -= duration; } // if->while Just in case something odd happens
        float x = energy_overlay_timer * BAR_LENGTH / duration; // {0,BAR_LENGTH} x coordinate where the two pieces join
        float percent_energy = player.GetEnergy() / player.GetEnergyMax();
        float max_x = percent_energy * BAR_LENGTH; // do not allow either piece to go beyond this coordinate.

        // position is (-90:0), fills from (0:position+90) from the right
        energy_overlays[ 0 ].rectTransform.position = new Vector3( energy_bar.rectTransform.position.x + (int) x - BAR_LENGTH, energy_overlays[0].rectTransform.position.y, 0.0f);
        energy_overlays[ 0 ].rectTransform.sizeDelta = new Vector2( BAR_LENGTH, energy_overlays[0].rectTransform.sizeDelta.y );
        energy_overlays[ 0 ].fillAmount = ( (int) x ) / BAR_LENGTH;

        // position is (0:90), fills from (position:90) from the left
        energy_overlays[ 1 ].rectTransform.position = new Vector3( energy_bar.rectTransform.position.x + (int) x, energy_overlays[1].rectTransform.position.y, 0.0f );
        energy_overlays[ 1 ].rectTransform.sizeDelta = new Vector2( BAR_LENGTH, energy_overlays[ 1 ].rectTransform.sizeDelta.y );
        energy_overlays[ 1 ].fillAmount = ( BAR_LENGTH - (int) x ) / BAR_LENGTH;

        // Use a mask to prevent the overlay from extending into the empty portion of the bar.
        energy_overlay_mask.rectTransform.sizeDelta = new Vector2( max_x, energy_overlay_mask.rectTransform.sizeDelta.y );

        PulseShield();
    }

    // Makes the shield bar pulse while it regenerates
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
        else
        {
            float t = Mathf.PingPong( shield_pulse_timer * 2.0f / duration, 1.0f );
            shield_bar.color = Blend( shield_bar_default_color, shield_bar_pulse_color, t );
            //shield_bar.color = shield_bar_default_color;
        }
    }

    /// <summary>
    /// Handles making outlines of bars + text blink
    /// </summary>
    private void BlinkOutlines()
    {
        // Energy bar
        if ( is_energy_blinking )
        {
            float duration = BLINK_DURATION; // seconds
            energy_blink_timer += Time.deltaTime * Time.timeScale;
            float weight = Mathf.PingPong( energy_blink_timer * 3.0f * 2.0f / duration, 1.0f ); // blink 3 times
            Color red = new Color( 1.0f, 0.0f, 0.0f, 0.80f);
            energy_outline.color = Blend( energy_outline_default_color, red, weight );

            if ( energy_blink_timer >= duration )
            {
                is_energy_blinking = false;
            }
        }
        else
        {
            energy_outline.color = energy_outline_default_color;
        }

        // Shield bar
        if ( is_shield_blinking )
        {
            float duration = BLINK_DURATION; // seconds
            shield_blink_timer += Time.deltaTime * Time.timeScale;
            float weight = Mathf.PingPong( shield_blink_timer * 3.0f * 2.0f / duration, 1.0f ); // blink 3 times
            Color red = new Color( 1.0f, 0.0f, 0.0f, 0.80f);
            shield_outline.color = Blend( shield_outline_default_color, red, weight );
            if ( is_shield_text_blinking )
            {
                shield_value.GetComponent<Outline>().effectColor = Blend( shield_text_outline_default_color, red, weight );
            }

            if ( shield_blink_timer >= duration )
            {
                is_shield_blinking = false;
                is_shield_text_blinking = false;
            }
        }
        else
        {
            shield_outline.color = shield_outline_default_color;
        }
    }

    /// <summary>
    /// Blends two colors along RGBA 
    /// (TODO: use HSV? It goes pretty fast, so maybe no need)
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
    /// Reduce the size of the red "dropped recently" bar
    /// </summary>
    private void DropLosses()
    {
        // Energy
        if ( energy_prev_frame >= player.GetEnergy() && energy_prev_frame >= energy_remembered_value )
        {
            energy_remembered_value = energy_prev_frame;
        }

        if ( energy_remembered_value >= player.GetEnergy() )
        {
            float percent_energy = player.GetEnergy() / player.GetEnergyMax();
            energy_loss.rectTransform.position = new Vector3( energy_bar.rectTransform.position.x + percent_energy * BAR_LENGTH, energy_loss.rectTransform.position.y, 0.0f );
            energy_loss.rectTransform.sizeDelta = new Vector2( ( energy_remembered_value - player.GetEnergy() ) / player.GetEnergyMax() * BAR_LENGTH, energy_loss.rectTransform.sizeDelta.y );
        }
        else
        {
            energy_loss.rectTransform.sizeDelta = new Vector2( 0.0f, energy_loss.rectTransform.sizeDelta.y );
        }

        // Shield
        if ( shield_prev_frame >= player.GetShields() && shield_prev_frame >= shield_remembered_value )
        {
            shield_remembered_value = shield_prev_frame;
        }

        if ( shield_remembered_value >= player.GetShields() )
        {
            float percent_shield = player.GetShields() / player.GetShieldsMax();
            shield_loss.rectTransform.position = new Vector3( shield_bar.rectTransform.position.x + percent_shield * BAR_LENGTH, shield_loss.rectTransform.position.y, 0.0f );
            shield_loss.rectTransform.sizeDelta = new Vector2( (shield_remembered_value - player.GetShields() ) / player.GetShieldsMax() * BAR_LENGTH, shield_loss.rectTransform.sizeDelta.y );
        }
        else
        {
            shield_loss.rectTransform.sizeDelta = new Vector2( 0.0f, shield_loss.rectTransform.sizeDelta.y );
        }

        // update old records.
        float drop_speed = 180.0f; // pixels per second
        energy_remembered_value -= drop_speed * Time.deltaTime * Time.timeScale;
        shield_remembered_value -= drop_speed * Time.deltaTime * Time.timeScale;
        energy_prev_frame = player.GetEnergy();
        shield_prev_frame = player.GetShields();
    }

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
}
