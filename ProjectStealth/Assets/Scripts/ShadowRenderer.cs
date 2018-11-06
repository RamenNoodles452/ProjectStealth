using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Renders shadows.
public class ShadowRenderer : MonoBehaviour
{
    #region vars
    private Mesh mesh;
    #endregion

    // Awake is called before Start, for initialization
    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        #if UNITY_EDITOR
        if ( mesh == null )
        {
            Debug.LogError( "Missing MeshFilter component!" );
        }
        #endif
    }

    // Update is called once per frame
    void Update ()
    {
        mesh = Mesh();
    }

    // Generates a simple mesh at z = 0.5 for rendering lights / shadows.
    private Mesh Mesh()
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs0 = new List<Vector2>();
        int[] indices = new int[6];

        Vector3 depth_modifier = ( Camera.main.transform.position.z - 0.5f ) * new Vector3( 0.0f, 0.0f, 1.0f );
        verts.Add( Camera.main.ScreenToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f )                  - depth_modifier ) );
        verts.Add( Camera.main.ScreenToWorldPoint( new Vector3( Screen.width, 0.0f, 0.0f )          - depth_modifier ) );
        verts.Add( Camera.main.ScreenToWorldPoint( new Vector3( Screen.width, Screen.height, 0.0f ) - depth_modifier ) );
        verts.Add( Camera.main.ScreenToWorldPoint( new Vector3( 0.0f, Screen.height, 0.0f )         - depth_modifier ) );

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
