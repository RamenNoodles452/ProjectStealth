using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes attached object act as a light.
// NOTE: object must be under the top-level level object in the hierarchy.
public class ShadowCastingLight : MonoBehaviour
{
    #region vars
    public int shadow_map_slot = -1;
    [Range( 0.0f, 512.0f )]
    public float range = 512.0f;
    #endregion

    // Use for pre-initialization
    private void Awake()
    {
        Register();
    }

    // Destroy is called when this script is destroyed
    private void OnDestroy()
    {
        Unregister();
    }

    // OnEnable is called when the gameObject/script is enabled. (also can be called on initialization)
    private void OnEnable()
    {
        Register();
    }

    // OnDisable is called when the gameObject/script is disabled.
    private void OnDisable()
    {
        Unregister();
    }

    /// <summary>
    /// Registers the light with the renderer, enabling it for shadow mapping
    /// </summary>
    private void Register()
    {
        if ( shadow_map_slot != -1 ) { return; } // already registered
        RenderEffects render_script = Camera.main.GetComponent<RenderEffects>();
        if ( render_script == null ) { return; }
        render_script.RegisterLight( this );
    }

    /// <summary>
    /// Unregisters the light with the renderer, disabling it for shadow mapping
    /// </summary>
    private void Unregister()
    {
        if ( shadow_map_slot == -1 ) { return; } // not registered, can't unregister
        if ( Camera.main == null ) { return; }
        RenderEffects render_script = Camera.main.GetComponent<RenderEffects>();
        if ( render_script == null ) { return; }
        render_script.UnregisterLight( this );
    }
}
