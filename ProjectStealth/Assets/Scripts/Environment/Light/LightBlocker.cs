using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach to geometry with a collider to make it cast shadows.
// NOTE: object must be beneath the top-level level object in the hierarchy.
public class LightBlocker : MonoBehaviour
{
    #region vars
    private BoxCollider2D my_box_collider;
    private CompositeCollider2D my_composite_collider;
    #endregion

    // Use this for initialization
    void Start ()
    {
        my_box_collider = GetComponent<BoxCollider2D>();
        if ( my_box_collider == null )
        {
            my_composite_collider = GetComponent<CompositeCollider2D>();
            if ( my_composite_collider == null )
            {
                #if UNITY_EDITOR
                Debug.LogError( "Missing BoxCollider2D or CompositeCollider2D: " + this.gameObject );
                #endif
                Destroy( this ); // remove component
                return;
            }
        }

        // Catch bad configuration.
        if ( CollisionMasks.light_occlusion_mask != ( CollisionMasks.light_occlusion_mask | LayerMask.GetMask( LayerMask.LayerToName( this.gameObject.layer ) ) ) )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Light occluder object " + gameObject + " is not on a recognized light-occluding layer. " +
                "It will not correctly occlude light. Move it to the correct layer, or add the current layer to the CollisionMasks.light_occlusion_mask." );
            #endif
            Destroy( this ); // remove component, so there is no visual / functional disparity. (Light/shadow graphics don't require correct layering, but detection logic does).
            return;
        }
    }

    /// <summary>
    /// Returns a list of the edges of the collider.
    /// </summary>
    /// <param name="edges">A list of the edges of the collider</param>
    public void GetEdges( ref List<Vector2> edges )
    {
        GetBoxEdges( ref edges );
        GetCompositeEdges( ref edges );
    }

    /// <summary>
    /// Returns a list of the edges of the box collider.
    /// </summary>
    /// <param name="edges">A list of the edges of the collider. Output param, will have edges added.</param>
    private void GetBoxEdges( ref List<Vector2> edges )
    {
        if ( my_box_collider == null ) { return; }

        float x = transform.position.x + my_box_collider.offset.x;
        float y = transform.position.y + my_box_collider.offset.y;
        Vector3[] vertex = new Vector3[4];
        vertex[ 0 ] = new Vector2( x - my_box_collider.size.x / 2.0f, y - my_box_collider.size.y / 2.0f );
        vertex[ 1 ] = new Vector2( x + my_box_collider.size.x / 2.0f, y - my_box_collider.size.y / 2.0f );
        vertex[ 2 ] = new Vector2( x + my_box_collider.size.x / 2.0f, y + my_box_collider.size.y / 2.0f );
        vertex[ 3 ] = new Vector2( x - my_box_collider.size.x / 2.0f, y + my_box_collider.size.y / 2.0f );

        edges.Add( vertex[ 0 ] );
        edges.Add( vertex[ 1 ] );

        edges.Add( vertex[ 1 ] );
        edges.Add( vertex[ 2 ] );

        edges.Add( vertex[ 2 ] );
        edges.Add( vertex[ 3 ] );

        edges.Add( vertex[ 3 ] );
        edges.Add( vertex[ 0 ] );
    }

    /// <summary>
    /// Returns a list of the edges of the composite collider.
    /// </summary>
    /// <param name="edges">A list of the edges of the collider. Output param, will have edges added.</param>
    private void GetCompositeEdges( ref List<Vector2> edges )
    {
        if ( my_composite_collider == null ) { return; }

        for ( int i = 0; i < my_composite_collider.pathCount; i++ )
        {
            Vector2[] points = new Vector2[ my_composite_collider.GetPathPointCount( i ) ];
            int point_count = my_composite_collider.GetPath( i, points );
            for ( int j = 0; j < point_count; j++ )
            {
                edges.Add( points[ j ] );
                edges.Add( points[ ( j + 1 ) % point_count ] );
            }
        }
    }
}
