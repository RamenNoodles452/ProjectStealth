using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Renders shadows.
public class ShadowRenderer : MonoBehaviour
{
    // Update is called once per frame
    void LateUpdate ()
    {
        // Use late update to ensure the camera has finished moving BEFORE this sets the mesh position based on it: prevents "lagging shadows".
        GetComponent<MeshFilter>().mesh = FullscreenMesh.Mesh( 0.5f, Vector3.zero ); //Generate a simple full-screen mesh at z = 0.5 for rendering lights / shadows.
    }
}
