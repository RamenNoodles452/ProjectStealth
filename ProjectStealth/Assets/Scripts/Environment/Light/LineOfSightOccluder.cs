using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightOccluder : MonoBehaviour
{
    #region vars
    private float MAX_DISTANCE = 0.0f;
    private const float SMALL_DELTA = 0.01f; // radians
    #endregion

    // Use this for initialization
    void Start ()
    {

    }
    
    // Update is called once per frame
    void Update ()
    {

        // Grab all light-occluding geometry in the scene
        Vector3 min3D    = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 0.0f, 0.0f ) );
        Vector3 max3D    = Camera.main.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
        Vector3 center3D = Camera.main.ViewportToWorldPoint( new Vector3( 0.5f, 0.5f, 0.0f ) );

        Vector2 min = new Vector2( min3D.x, min3D.y );
        Vector2 max = new Vector2( max3D.x, max3D.y );
        Vector2 center = new Vector2( center3D.x, center3D.y );
        // Set max distance. TODO: optimize
        Vector2 size   = new Vector2( max3D.x - min3D.x, max3D.y - min3D.y );
        MAX_DISTANCE = size.magnitude / 1.5f; // 1/2 WOULD work, if the camera were not offset from the player.

        List<CastData> list = new List<CastData>();
        Collider2D[] hits = Physics2D.OverlapAreaAll( min, max, CollisionMasks.light_occlusion_mask );

        // For special case: no hits, draw fullscreen no blackout
        // cast to the 4 corners of the screen.
        RayCast( min, ref list );
        RayCast( new Vector2( min.x, max.y ), ref list );
        RayCast( max, ref list );
        RayCast( new Vector2( max.x, min.y ), ref list );

        foreach ( Collider2D hit in hits )
        {
            if ( hit == null ) { return; }

            BoxCollider2D box = hit.gameObject.GetComponent<BoxCollider2D>();
            if ( box == null ) { return; }

            // raycast 3 rays at each corner
            Vector2 top_left     = new Vector2( box.bounds.min.x, box.bounds.max.y );
            Vector2 top_right    = new Vector2( box.bounds.max.x, box.bounds.max.y );
            Vector2 bottom_left  = new Vector2( box.bounds.min.x, box.bounds.min.y );
            Vector2 bottom_right = new Vector2( box.bounds.max.x, box.bounds.min.y );

            // store distance/pt of impact of each ray, sort by angle
            RayCast( top_left, ref list );
            RayCast( top_right, ref list );
            RayCast( bottom_left, ref list );
            RayCast( bottom_right, ref list );
        }

        // create triangle fan by connecting dots w/ player position, to cut out of the blackness

        list.Sort();
        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        if ( mesh_filter == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing component: MeshFilter" );
            #endif
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        float z = 11.0f;
        // center point
        vertices.Add( new Vector3( (int) transform.position.x, (int) transform.position.y, z ) - new Vector3( (int) transform.position.x, (int) transform.position.y, transform.position.z ) );
        uvs.Add( new Vector2( 0.0f, 0.0f ) );

        foreach ( CastData cast in list )
        {
            vertices.Add( new Vector3( cast.point.x, cast.point.y, z ) - transform.position );
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
    private void RayCast( Vector2 point, ref List<CastData> list )
    {
        Vector2 origin = new Vector2( (int) transform.position.x, (int) transform.position.y );
        float angle = Mathf.Atan2( point.y - origin.y, point.x - origin.x );

        CastData[] cast_data = new CastData[3];
        cast_data[ 0 ] = Cast2( origin, angle - SMALL_DELTA );
        cast_data[ 1 ] = Cast2( origin, angle );
        cast_data[ 2 ] = Cast2( origin, angle + SMALL_DELTA );
        // TODO: collapse if distance is within 1 px

        list.Add( cast_data[ 0 ] );
        list.Add( cast_data[ 1 ] );
        list.Add( cast_data[ 2 ] );
    }

    private CastData Cast2( Vector2 origin, float angle )
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
        Debug.DrawLine( origin, impact_point );
        #endif

        return new CastData( angle, distance, impact_point );
    }
}

//
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

    public CastData( float angle, float distance, Vector2 point )
    {
        this.angle = ( ( angle % ( Mathf.PI * 2.0f ) ) + ( Mathf.PI * 2.0f ) ) % ( Mathf.PI * 2.0f ); // in 0 - 2PI range
        //this.distance = distance;
        this.point = point;
    }
}
