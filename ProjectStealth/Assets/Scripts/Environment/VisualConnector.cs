using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ties two objects together with a tiled sprite.
// Use for pendulum ropes, chains, etc.
public class VisualConnector : MonoBehaviour
{
    #region vars
    [SerializeField]
    private GameObject a;
    [SerializeField]
    private GameObject b;
    #endregion

    // Use this for initialization
    void Start ()
    {
        if ( a == null || b == null )
        {
            Debug.LogError( "Configuration issue: missing object" );
            Destroy( this ); // remove component
        }
    }

    // Update is called once per frame
    void Update ()
    {
        float distance = Vector2.Distance( (Vector2) a.transform.position, (Vector2) b.transform.position );
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.size = new Vector2( distance, renderer.size.y );
        float angle =  Mathf.Atan2( b.transform.position.y - a.transform.position.y, b.transform.position.x - a.transform.position.x );
        transform.rotation = Quaternion.Euler( 0.0f, 0.0f, angle * Mathf.Rad2Deg );
        transform.position = a.transform.position + distance / 2.0f * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
    }
}
