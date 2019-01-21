using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Causes a light to flicker.
/// </summary>
public class FlickeringLight : MonoBehaviour
{
    #region vars
    public bool is_flickering;
    [SerializeField]
    private float flicker_duration = 5.0f; // seconds. negative values mean infinite duration.

    private float flicker_timer = 0.0f;
    private Light flickering_light;
    private float initial_intensity;
    #endregion

    // Use for early initialization ( references )
    private void Awake()
    {
        flickering_light = this.gameObject.GetComponent<Light>();
        initial_intensity = flickering_light.intensity;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimers();

        if ( ! is_flickering ) { return; }

        float factor = 1.0f;
        if ( flicker_duration >= 0.0f ) { factor = 0.5f + 0.5f * Mathf.Sin( (flicker_timer / flicker_duration + Random.Range( -0.15f, 0.15f ) ) * Mathf.PI * 6.0f ); }
        float intensity = Random.Range( 0.0f, initial_intensity * 1.2f ) * factor;
        flickering_light.intensity = intensity;
    }

    /// <summary>
    /// Counts down until the light resumes normal operation.
    /// </summary>
    private void UpdateTimers()
    {
        if ( flicker_duration < 0.0f ) { return; }
        if ( ! is_flickering ) { return; }
        flicker_timer = Mathf.Max( 0.0f, flicker_timer - Time.deltaTime * Time.timeScale );
        if ( flicker_timer <= 0.0f )
        {
            is_flickering = false;
            flickering_light.enabled = true;
            flickering_light.intensity = initial_intensity;
        }
    }

    /// <summary>
    /// Causes the light to begin flickering.
    /// </summary>
    public void StartFlickering()
    {
        is_flickering = true;
        flicker_timer = flicker_duration;
    }
}
