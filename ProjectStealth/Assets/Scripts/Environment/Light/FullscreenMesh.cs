using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Draws a full screen mesh.
public class FullscreenMesh : MonoBehaviour
{
    #region vars
    private MeshFilter mesh_filter;
    #endregion

    // Use this for early initialization
    private void Awake()
    {
        mesh_filter = GetComponent<MeshFilter>();
        #if UNITY_EDITOR
        if ( mesh_filter == null )
        {
            Debug.LogError( "Missing component: MeshFilter" );
        }
        #endif
    }

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // LateUpdate is called once per frame
    void LateUpdate ()
    {
        // Use late update so this happens after the camera moves, preventing lagging shadows.
        if ( mesh_filter == null )
        {
            return;
        }
        mesh_filter.mesh = FullscreenMesh.Mesh( 0.5f, -1.0f * new Vector3( transform.position.x, transform.position.y, 0.0f ) );
    }

    /// <summary>
    /// Generates a simple full-screen mesh at the provided z depth for rendering lights / shadows.
    /// </summary>
    /// <param name="z">The target z coordinate to render at. Draw order may be shader-determined, so this may not matter</param>
    /// <param name="offset">The offset to apply to the mesh</param>
    /// <returns>The mesh.</returns>
    public static Mesh Mesh( float z, Vector3 offset )
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs0 = new List<Vector2>();
        int[] indices = new int[6];

        Vector3 depth_modifier = ( Camera.main.transform.position.z - z ) * new Vector3( 0.0f, 0.0f, 1.0f );

        float x = Screen.width;
        float y = Screen.height;
        if ( Camera.main.targetTexture != null ) // If camera is rendering to a texture, offsets need to be calculated a bit differently.
        {
            x = Camera.main.targetTexture.width;
            y = Camera.main.targetTexture.height;
        }

        verts.Add( Camera.main.ScreenToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f ) - depth_modifier ) + offset );
        verts.Add( Camera.main.ScreenToWorldPoint( new Vector3(    x, 0.0f, 0.0f ) - depth_modifier ) + offset );
        verts.Add( Camera.main.ScreenToWorldPoint( new Vector3(    x,    y, 0.0f ) - depth_modifier ) + offset );
        verts.Add( Camera.main.ScreenToWorldPoint( new Vector3( 0.0f,    y, 0.0f ) - depth_modifier ) + offset );

        uvs0.Add( new Vector2( 0.0f, 0.0f ) );
        uvs0.Add( new Vector2( 1.0f, 0.0f ) );
        uvs0.Add( new Vector2( 1.0f, 1.0f ) );
        uvs0.Add( new Vector2( 0.0f, 1.0f ) );

        indices[ 0 ] = 0;
        indices[ 1 ] = 1;
        indices[ 2 ] = 2;
        indices[ 3 ] = 0;
        indices[ 4 ] = 2;
        indices[ 5 ] = 3;

        Mesh mesh = new Mesh();
        mesh.SetVertices( verts );
        mesh.SetUVs( 0, uvs0 );
        mesh.SetIndices( indices, MeshTopology.Triangles, 0 );
        return mesh;
    }
}
