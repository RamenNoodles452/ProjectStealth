using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Disables the TilemapRenderer attached to this GameObject.
/// Since we use tile -> prefab linkage, and the prefabs have their own sprites with custom sprite shaders, we don't need it 
/// (except in edit mode, where the prefabs don't exist or display).
/// </summary>
public class DisableTileRenderer : MonoBehaviour
{
    private void Awake()
    {
        TilemapRenderer renderer = GetComponent<TilemapRenderer>();
        if ( renderer == null ) { return; }
        renderer.enabled = false;
    }
}
