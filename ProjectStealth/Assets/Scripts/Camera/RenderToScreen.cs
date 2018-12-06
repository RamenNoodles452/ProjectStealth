using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Uses a second camera to draw the results of the main camera to the screen.
public class RenderToScreen : MonoBehaviour
{
    #region vars
    [SerializeField]
    private RenderTexture render_texture;
    #endregion

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        
    }

    // Called when scene rendering is complete
    private void OnRenderImage( RenderTexture source, RenderTexture destination )
    {
        Graphics.Blit( render_texture, destination );
    }
}
