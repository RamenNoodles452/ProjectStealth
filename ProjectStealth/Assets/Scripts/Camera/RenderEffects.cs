using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RenderEffects : MonoBehaviour
{
    [Range(0,1)]
    public float desaturation = 1.0f;
    [SerializeField]
    private Shader desaturate_shader;
    private Material desaturate_material;

    public bool is_night_vision_on = true;
    [SerializeField]
    private Shader NVG_shader;
    private Material NVG_material;

    private void Awake()
    {
        desaturate_material = new Material( desaturate_shader );
        NVG_material = new Material( NVG_shader );
    }

    private void OnRenderImage( RenderTexture source, RenderTexture destination )
    {
        // Desaturate
        RenderTexture render_texture = new RenderTexture( source );
        if ( desaturation == 0.0f )
        {
            Graphics.Blit( source, render_texture );
        }
        else
        {
            desaturate_material.SetFloat( "_Desaturate", desaturation );
            Graphics.Blit( source, render_texture, desaturate_material );
        }

        // Night Vision Goggles
        RenderTexture render_texture2 = new RenderTexture( render_texture );
        if ( ! is_night_vision_on )
        {
            Graphics.Blit( source, render_texture2 );
        }
        else
        {
            Graphics.Blit( render_texture, render_texture2, NVG_material );
        }
        render_texture.Release();

        Graphics.Blit( render_texture2, destination );
        render_texture2.Release();
    }
}
