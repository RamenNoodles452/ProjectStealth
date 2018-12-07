using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightOccluder : MonoBehaviour
{
    #region vars
    private float MAX_DISTANCE = 0.0f;
    private const float SMALL_DELTA = 0.01f; // radians
    private const float DEEPEST_Z = 11.0f;
    #endregion

    // Use this for initialization
    void Start ()
    {

    }
    
    // Update is called once per frame
    void Update ()
    {
        // Get all colliders
        Collider2D[] colliders = GetCollidersInScene();

        List<CastData> list = new List<CastData>();
        CastToScreenCorners( ref list );
        CastAtColliders( colliders, ref list );
        ConstructTriangleFan( ref list );
    }


    /// <summary>
    /// Gets all the line of sight affecting colliders in the relevant area of the scene.
    /// </summary>
    /// <returns>An array of all the colliders that affect line of sight</returns>
    private Collider2D[] GetCollidersInScene()
    {
        // Grab all light-occluding geometry on the screen + extra buffer
        // ( error-free would need all in the scene, this should work OK enough though, with certain topology restrictions)
        Vector3 min3D    = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f ) );
        Vector3 max3D    = Camera.main.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
        Vector3 center3D = Camera.main.ViewportToWorldPoint( new Vector3( 0.5f, 0.5f, 0.0f ) );

        Vector2 min = new Vector2( min3D.x, min3D.y );
        Vector2 max = new Vector2( max3D.x, max3D.y );
        Vector2 center = new Vector2( center3D.x, center3D.y );
        // Set max distance. TODO: optimize
        Vector2 size   = new Vector2( max3D.x - min3D.x, max3D.y - min3D.y );
        MAX_DISTANCE = size.magnitude; // 1/2 WOULD work, if the camera were not offset from the player.

        // Make the area 2 screens wide and 2 screen high, so things slightly offscreen don't end up causing odd shadowing.
        min = new Vector2( min.x - size.x, min.y - size.y );
        max = new Vector2( max.x + size.x, max.y + size.y );

        return Physics2D.OverlapAreaAll( min, max, CollisionMasks.light_occlusion_mask );
    }

    /// <summary>
    /// Casts sight rays to the 4 corners of the screen.
    /// These casts need to be added so the sight occlusion blackout mesh gets drawn correctly.
    /// </summary>
    /// <param name="list">The list of sight raycast data. Will have additional information added to it.</param>
    private void CastToScreenCorners( ref List<CastData> list )
    {
        // For the special case: no colliders on screen, make the mask fullscreen so no blackout.
        Vector3 min3D    = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f ) );
        Vector3 max3D    = Camera.main.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
        Vector2 min = new Vector2( min3D.x, min3D.y );
        Vector2 max = new Vector2( max3D.x, max3D.y );

        TriRayCast( min, ref list );
        TriRayCast( new Vector2( min.x, max.y ), ref list );
        TriRayCast( max, ref list );
        TriRayCast( new Vector2( max.x, min.y ), ref list );
    }

    /// <summary>
    /// Casts sight rays against the 4 corners of all rect geometry (virtually all of it)
    /// </summary>
    /// <param name="colliders">The array of colliders to cast rays against.</param>
    /// <param name="list">The list of sight raycast data. Will have additional information added to it.</param>
    private void CastAtColliders( Collider2D[] colliders, ref List<CastData> list )
    {
        foreach ( Collider2D collider in colliders )
        {
            if ( collider == null ) { return; }

            ProcessCompositeCollider( collider, ref list );
            ProcessBoxCollider( collider, ref list );
        }
    }

    /// <summary>
    /// Gets all the end points out of the composite collider, and casts rays against them.
    /// </summary>
    /// <param name="collider">The collider to process</param>
    /// <param name="list">Outputs the resulting raycast results</param>
    private void ProcessCompositeCollider( Collider2D collider, ref List<CastData> list )
    {
        // Composite collider 2D geometry
        CompositeCollider2D composite = collider.gameObject.GetComponent<CompositeCollider2D>();
        if ( composite == null ) { return; }
        
        for ( int i = 0; i < composite.pathCount; i++ )
        {
            Vector2[] points = new Vector2[ composite.GetPathPointCount( i ) ];
            int point_count = composite.GetPath( i, points );
            for ( int j = 0; j < point_count; j++ )
            {
                TriRayCast( points[ j ], ref list );
            }
        }
    }

    /// <summary>
    /// Gets the corners out of the box collider, and casts rays against them.
    /// </summary>
    /// <param name="collider">The collider to process</param>
    /// <param name="list">Outputs the resulting raycast results</param>
    private void ProcessBoxCollider( Collider2D collider, ref List<CastData> list )
    {
        // Box collider 2D geometry
        BoxCollider2D box = collider.gameObject.GetComponent<BoxCollider2D>();
        if ( box == null ) { return; }

        // raycast 3 rays at each corner
        Vector2 top_left     = new Vector2( box.bounds.min.x, box.bounds.max.y );
        Vector2 top_right    = new Vector2( box.bounds.max.x, box.bounds.max.y );
        Vector2 bottom_left  = new Vector2( box.bounds.min.x, box.bounds.min.y );
        Vector2 bottom_right = new Vector2( box.bounds.max.x, box.bounds.min.y );

        // store distance/pt of impact of each ray, sort by angle
        TriRayCast( top_left, ref list );
        TriRayCast( top_right, ref list );
        TriRayCast( bottom_left, ref list );
        TriRayCast( bottom_right, ref list );
    }

    /// <summary>
    /// Creates a triangle fan "mask" by connecting the dots with the player's position, to cut out of the fullscreen blackout mesh.
    /// </summary>
    /// <param name="list">The list of sight raycast data, to construct the mesh out of.</param>
    private void ConstructTriangleFan( ref List<CastData> list )
    {
        // Validation.
        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        if ( mesh_filter == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing component: MeshFilter" );
            #endif
            return;
        }

        // Mesh data
        float z = DEEPEST_Z; // mask needs to be behind the furthest back geometry in the scene, so the stencil buffer doesn't mask out unintended stuff.
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        
        // Add center point as 0th vert.
        vertices.Add( new Vector3( (int) transform.position.x, (int) transform.position.y, z ) - new Vector3( (int) transform.position.x, (int) transform.position.y, transform.position.z ) );
        uvs.Add( new Vector2( 0.0f, 0.0f ) );

        // Add all other verts, in counterclockwise angular order from angle 0 - 360 degrees.
        list.Sort();
        foreach ( CastData cast in list )
        {
            vertices.Add( new Vector3( (int) Mathf.Round( cast.point.x ), (int) Mathf.Round( cast.point.y ), z ) - transform.position );
            uvs.Add( new Vector2( 0.0f, 0.0f ) );
        }

        // Build the triangle fan out of the verts
        int[] indecies = new int[ vertices.Count * 3 ];
        for ( int i = 0; i < vertices.Count; i++ )
        {
            indecies[ i * 3 ]     = 0;     // center point
            indecies[ i * 3 + 1 ] = i;     // ith vertex
            indecies[ i * 3 + 2 ] = i + 1; // next vertex (or 1st if ith vertex is last vertex)
            if ( i == vertices.Count - 1 ) { indecies[ i * 3 + 2] = 1; }
        }

        // Actually create the mesh, and display it.
        Mesh mesh = new Mesh();
        mesh.SetVertices( vertices );
        mesh.SetUVs( 0, uvs );
        mesh.SetIndices( indecies, MeshTopology.Triangles, 0 );
        mesh_filter.mesh = mesh;
    }

    /// <summary>
    /// Casts 3 rays from this object to the specified point, and stores the results in list.
    /// </summary>
    /// <param name="point">The point to cast the ray to.</param>
    /// <param name="list">List of ray cast data, used to aggregate results</param>
    private void TriRayCast( Vector2 point, ref List<CastData> list )
    {
        Vector2 origin = new Vector2( (int) transform.position.x, (int) transform.position.y );
        float angle = Mathf.Atan2( point.y - origin.y, point.x - origin.x );

        CastData[] cast_data = new CastData[3];
        cast_data[ 0 ] = RayCast( origin, angle - SMALL_DELTA );
        cast_data[ 1 ] = RayCast( origin, angle );
        cast_data[ 2 ] = RayCast( origin, angle + SMALL_DELTA );

        for ( int i = 0; i < 3; i++ )
        {
            list.Add( cast_data[ i ] );
        }

        // Jittery, went with the less efficient approach for smoothness.
        // Optimization: collapse if distance is within 1 px
        //list.Add( cast_data[ 1 ] );
        //if ( Mathf.Abs( cast_data[ 1 ].distance - cast_data[ 0 ].distance ) >= 1.0f ) { list.Add( cast_data[ 0 ] ); }
        //if ( Mathf.Abs( cast_data[ 1 ].distance - cast_data[ 2 ].distance ) >= 1.0f ) { list.Add( cast_data[ 2 ] ); }
    }

    /// <summary>
    /// Casts a ray from the specified origin at the specified angle.
    /// </summary>
    /// <param name="origin">The starting point of the ray.</param>
    /// <param name="angle">The angle to fire the ray at, in radians.</param>
    /// <returns></returns>
    private CastData RayCast( Vector2 origin, float angle )
    {
        Vector2 direction = new Vector2( Mathf.Cos( angle ), Mathf.Sin( angle ) );

        RaycastHit2D hit = Physics2D.Raycast( origin, direction, MAX_DISTANCE, CollisionMasks.light_occlusion_mask );
        Vector2 impact_point;
        float distance;

        if ( hit.collider == null )
        {
            impact_point = origin + direction * MAX_DISTANCE;
            distance = MAX_DISTANCE;
        }
        else
        {
            impact_point = hit.point;
            distance = hit.distance;
        }

        #if UNITY_EDITOR
        //Debug.DrawLine( origin, impact_point );
        #endif

        return new CastData( angle, /*distance,*/ impact_point );
    }
}

// Stores subset of raycast data in sortable format.
public class CastData: System.IComparable<CastData>
{
    #region vars
    public float angle;
    //public float distance;
    public Vector2 point;
    #endregion

    public int CompareTo( CastData other ) // sortable
    {
        return angle.CompareTo( other.angle );
    }

    public CastData( float angle, /*float distance,*/ Vector2 point )
    {
        this.angle = ( ( angle % ( Mathf.PI * 2.0f ) ) + ( Mathf.PI * 2.0f ) ) % ( Mathf.PI * 2.0f ); // in 0 - 2PI range
        //this.distance = distance;
        this.point = point;
    }
}
