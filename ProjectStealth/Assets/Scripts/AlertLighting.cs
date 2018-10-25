using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Changes the color of the light to RED when on alert.
public class AlertLighting : MonoBehaviour
{
    #region vars
    private Light light;
    private Color safe_color;
    private float intensity;
    private float timer;
    #endregion

    // Use this for initialization
    void Start ()
    {
        light = GetComponent<Light>();
        safe_color = light.color;
        intensity = light.intensity;
    }
    
    // Update is called once per frame
    void Update ()
    { 
        // TODO: optimize. Only edge-trigger this.
        if ( GameState.instance.is_red_alert )
        {
            light.color = new Color( 1.0f, 0.0f, 0.0f );

            timer += Time.deltaTime * Time.timeScale;
            if ( timer > Mathf.PI * 2.0f )
            {
                timer -= Mathf.PI * 2.0f;
            }
            light.intensity =  intensity - 0.5f * intensity * 0.5f * ( Mathf.Sin( timer ) + 1.0f );
        }
        else
        {
            light.color = safe_color;
            light.intensity = intensity;
        }
    }
}
