using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DesaturateEffect : MonoBehaviour
{
    [Range(0,1)]
    public float desaturation = 1.0f;
    [SerializeField]
    private Shader shader;
    private Material material;

    private void Awake()
    {
        material = new Material( shader );
    }

    private void OnRenderImage( RenderTexture source, RenderTexture destination )
    {
        if ( desaturation == 0.0f )
        {
            Graphics.Blit( source, destination );
            return;
        }
        else
        {
            material.SetFloat( "_Desaturate", desaturation );
            Graphics.Blit( source, destination, material );
        }
    }
}
