using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays overlays on top of characters.
/// </summary>
public class CharacterOverlay : MonoBehaviour
{
    #region vars
    private float hurt_timer;
    private bool is_pulse_on;
    private float pulse_timer;
    private Color pulse_color;
    private SpriteRenderer sprite_renderer;
    #endregion

    // Use this for early initialization
    private void Awake()
    {
        sprite_renderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        const float PULSE_DURATION = 0.25f;

        // hurt flash
        if ( hurt_timer > 0.0f )
        {
            hurt_timer = Mathf.Max( hurt_timer - Time.deltaTime * Time.timeScale, 0.0f );
            if ( Mathf.Sin( hurt_timer * Mathf.PI * 2.0f * 10.0f ) >= 0.0f )
            {
                sprite_renderer.color = new Color( 1.0f, 1.0f, 1.0f, 0.75f );
                return; // if hurt, override other overlays
            }
        }

        // pulsing overlay
        if ( is_pulse_on )
        {
            pulse_timer += Time.deltaTime * Time.timeScale;
            while ( pulse_timer > PULSE_DURATION ) { pulse_timer -= PULSE_DURATION; }
            float alpha = 0.375f + 0.125f * 0.5f * Mathf.Cos( pulse_timer * Mathf.PI * 2.0f / PULSE_DURATION ); // 0.25f : 0.5f
            sprite_renderer.color = new Color( pulse_color.r, pulse_color.g, pulse_color.b, alpha );
        }
        else
        {
            if ( sprite_renderer.color.a >= 0.0f )
            {
                sprite_renderer.color = new Color( 1.0f, 1.0f, 1.0f, 0.0f );
            }
        }
    }

    /// <summary>
    /// Starts flashing when hurt.
    /// </summary>
    public void StartHurtBlink()
    {
        const float HURT_DURATION = 1.0f;
        hurt_timer = HURT_DURATION;
    }

    /// <summary>
    /// Shows a pulsing overlay. 
    /// (only 1 allowed at a time, doesn't support tracking multiple overlays)
    /// </summary>
    /// <param name="color">The color of the overlay.</param>
    public void ShowOverlay( Color color )
    {
        is_pulse_on = true;
        pulse_color = color;
        pulse_timer = 0.0f;
    }

    /// <summary>
    /// Stops showing the pulsing overlay.
    /// </summary>
    public void HideOverlay()
    {
        is_pulse_on = false;
    }
}
