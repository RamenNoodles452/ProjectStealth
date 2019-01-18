using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
// Used to force meshes onto a sorting layer.
public class SetSortingLayer : MonoBehaviour
{
    #region vars
    public string sorting_layer;
    public int order_in_layer;
    private Renderer my_renderer;
    #endregion

    // Use this for initialization
    void Start ()
    {
        my_renderer = GetComponent<Renderer>();
        if ( my_renderer == null ) { return; }
        my_renderer.sortingLayerName = sorting_layer;
        my_renderer.sortingOrder = order_in_layer;
    }
}
