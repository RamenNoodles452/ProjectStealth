using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Make a UI image pulse between two colors.
/// </summary>
public class UIImageColorPulse : MonoBehaviour
{
    #region vars
    [SerializeField]
    private Color color_a;
    [SerializeField]
    private Color color_b;
    [SerializeField]
    private float transition_a_to_b_duration = 1.0f;
    [SerializeField]
    private float transition_b_to_a_duration = 1.0f;
    [SerializeField]
    private float stay_color_a_duration = 1.0f;
    [SerializeField]
    private float stay_color_b_duration = 1.0f;

    private Image image;
    private float timer;
    private int state; // 0 = a, 1 = transition a-b, 2 = b, 3 = transition b-a.
    #endregion

    /// <summary>
    /// USe this for early initialization.
    /// </summary>
    void Awake ()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if ( Time.timeScale == 0.0f ) { timer += Time.unscaledDeltaTime; }
        else { timer += Time.deltaTime * Time.timeScale; }

        if ( state == 0 ) // stay color a
        {
            image.color = color_a;
            if ( timer >= stay_color_a_duration )
            {
                timer -= stay_color_a_duration;
                state++;
                return;
            }
        }
        else if ( state == 1 ) // a to b
        {
            // gross RGB lerp, should HSV lerp.
            if ( timer >= transition_a_to_b_duration )
            {
                timer -= transition_a_to_b_duration;
                state++;
                return;
            }
            float t = timer / transition_a_to_b_duration;
            Color color = new Color( Mathf.Lerp( color_a.r, color_b.r, t ), Mathf.Lerp( color_a.g, color_b.g, t ), Mathf.Lerp( color_a.b, color_b.b, t ), Mathf.Lerp( color_a.a, color_b.a, t ) );
            image.color = color;
        }
        else if ( state == 2 ) // stay color b
        {
            image.color = color_b;
            if ( timer >= stay_color_b_duration )
            {
                timer -= stay_color_b_duration;
                state++;
                return;
            }
        }
        else if ( state == 3 ) // b to a
        {
            // gross RGB lerp, should HSV lerp.
            if ( timer >= transition_b_to_a_duration )
            {
                timer -= transition_b_to_a_duration;
                state = 0;
                return;
            }
            float t = timer / transition_b_to_a_duration;
            Color color = new Color( Mathf.Lerp( color_b.r, color_a.r, t ), Mathf.Lerp( color_b.g, color_a.g, t ), Mathf.Lerp( color_b.b, color_a.b, t ), Mathf.Lerp( color_b.a, color_a.a, t ) );
            image.color = color;
        }
        else
        {
            state = 0;
        }
    }
}
