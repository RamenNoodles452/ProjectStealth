using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach to geometry with a collider to make it cast shadows.
// NOTE: object must be beneath the top-level level object in the hierarchy.
public class LightBlocker : MonoBehaviour
{
    #region vars
    private BoxCollider2D my_collider;
    #endregion

    // Use this for initialization
    void Start ()
    {
        my_collider = GetComponent<BoxCollider2D>();
        if ( my_collider == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing BoxCollider2D: " + this.gameObject );
            #endif
            Destroy( this ); // remove component
            return;
        }
    }
    
    // Update is called once per frame
    void Update ()
    {
        
    }

    /// <summary>
    /// Returns a list of the edges of the collider.
    /// </summary>
    /// <param name="edges">A list of the edges of the collider</param>
    public void GetEdges( ref List<Vector2> edges )
    {
        float x = transform.position.x + my_collider.offset.x;
        float y = transform.position.y + my_collider.offset.y;
        Vector3[] vertex = new Vector3[4];
        vertex[ 0 ] = new Vector2( x - my_collider.size.x / 2.0f, y - my_collider.size.y / 2.0f );
        vertex[ 1 ] = new Vector2( x + my_collider.size.x / 2.0f, y - my_collider.size.y / 2.0f );
        vertex[ 2 ] = new Vector2( x + my_collider.size.x / 2.0f, y + my_collider.size.y / 2.0f );
        vertex[ 3 ] = new Vector2( x - my_collider.size.x / 2.0f, y + my_collider.size.y / 2.0f );

        edges.Add( vertex[ 0 ] ); // TODO: wrap in a class?
        edges.Add( vertex[ 1 ] );

        edges.Add( vertex[ 1 ] );
        edges.Add( vertex[ 2 ] );

        edges.Add( vertex[ 2 ] );
        edges.Add( vertex[ 3 ] );

        edges.Add( vertex[ 3 ] );
        edges.Add( vertex[ 0 ] );
    }
}
