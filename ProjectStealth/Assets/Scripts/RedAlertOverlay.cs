using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//-GabeV
//Class for "Red Alert" visual effect
public class RedAlertOverlay : MonoBehaviour
{
    #region vars
    private float alpha;
    private float increment = 1.5f;
    private bool fade_in;
    private Image image;
    #endregion

    // Use this for initialization
    void Start()
    {
        alpha = 0;
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        // enabled check
        if ( !GameState.instance.is_red_alert ) { alpha = 0.0f; }
        else
        {
            if ( fade_in )
            {
                alpha += increment * Time.deltaTime * Time.timeScale;
                if ( alpha >= 1.0f ) { alpha = 1.0f; fade_in = false; }
            }
            else
            {
                alpha -= increment * Time.deltaTime * Time.timeScale;
                if ( alpha <= 0.0f ) { alpha = 0.0f; fade_in = true; }
            }
        }

        image.color = new Color( 0.9f, 0.0f, 0.0f, alpha * 0.25f );
    }
}
