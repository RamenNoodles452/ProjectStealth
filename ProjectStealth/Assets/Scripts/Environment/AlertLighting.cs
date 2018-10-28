using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Changes the color of the light to RED, and makes it pulse when on alert.
public class AlertLighting : MonoBehaviour
{
    #region vars
    [SerializeField]
    private float period = 1.0f; // time to do a full pulse, in seconds
    [SerializeField]
    [Range (0.0f, 1.0f)]
    private float  minimum_brightness = 0.10f; // as a % of maximum brightness

    private Light light;
    private Color safe_color;
    private float intensity;
    private float timer;
    #endregion

    // Use this for initialization
    void Start()
    {
        light = GetComponent<Light>();
        safe_color = light.color;
        intensity = light.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        if ( GameState.instance.is_red_alert )
        {
            light.color = new Color( 1.0f, 0.0f, 0.0f );

            timer += Time.deltaTime * Time.timeScale * Mathf.PI * 2.0f / period;
            if ( timer > Mathf.PI * 2.0f )
            {
                timer -= Mathf.PI * 2.0f;
            }
            // 100% : minumum brightness % intensity
            light.intensity = intensity * ( 1.0f - ( 1.0f - minimum_brightness ) * ( 0.5f * ( Mathf.Sin( timer ) + 1.0f ) ) );
        }
        else
        {
            light.color = safe_color;
            light.intensity = intensity;
        }
    }
}
