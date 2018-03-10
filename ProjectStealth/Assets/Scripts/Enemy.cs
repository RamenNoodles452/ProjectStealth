using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-GabeV
//A basic enemy AI
public class Enemy : MonoBehaviour
{
    #region vars
    private bool canHear = true;
    private float listeningRadius = 0.0f;

    // snooze, vigilance, investigate, alert, combat

    #region vision
    public GameObject visionSubObject;
    private PolygonCollider2D visionTriangle;
    public float visionHalfAngle = 30.0f; // degrees
    public float visionRange = 100.0f; // pixels
    #endregion
    #endregion

    // Use this for pre-initialization
    private void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {
        Referencer.Instance.RegisterEnemy( this.gameObject );

        visionSubObject.GetComponent<EnemyVisionField>().enemy = this; // circular reference
        visionTriangle = visionSubObject.GetComponent<PolygonCollider2D>();
        if ( visionHalfAngle >= 90.0f || visionHalfAngle < 0.0f )
        {
            Debug.Log( "Invalid vision cone angle!" );
            visionHalfAngle = 30.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // if you're not already on alert...
        Listen();
        Watch();
    }

    /// <summary>
    /// Detect noise caused by the player
    /// </summary>
    private void Listen()
    {
        if ( ! canHear ) { return; }

        foreach ( Noise noise in Referencer.Instance.noises )
        {
            if ( IsDistanceBetweenPointsLessThan( noise.position.x, noise.position.y, this.gameObject.transform.position.x, this.gameObject.transform.position.y, noise.radius + listeningRadius ) )
            {
                // Detected!
                // TODO: if available, set investigation mode
                Debug.Log( "Sound detected!" );
                GameState.Instance.IsRedAlert = true; // test
            }
        }
    }

    /// <summary>
    /// Square distance checker to determine if 
    /// the distance between two points is less than a maximum distance.
    /// </summary>
    /// <param name="x1">x coordinate of point 1</param>
    /// <param name="y1">y coordinate of point 1</param>
    /// <param name="x2">x coordinate of point 2</param>
    /// <param name="y2">y coordinate of point 2</param>
    /// <param name="distance">distance to compare against</param>
    /// <returns>True if the distance between two points is less than the maximum distance</returns>
    private bool IsDistanceBetweenPointsLessThan( float x1, float y1, float x2, float y2, float distance )
    {
        return Mathf.Pow( x1 - x2, 2 ) + Mathf.Pow( y1 - y2, 2 ) <= Mathf.Pow( distance, 2 );
    }

    private void Watch()
    {
        // manipulate vision polygon, allow directional flipping
        // TODO: only call on initialization and direction change

        Vector2[] path = new Vector2[3]; 
        path[0] = new Vector2( 0.0f, 0.0f ); // local space
        path[1] = new Vector2( visionRange * Mathf.Cos( visionHalfAngle * Mathf.Deg2Rad ), visionRange * Mathf.Sin( visionHalfAngle * Mathf.Deg2Rad ) );
        path[2] = new Vector2( visionRange * Mathf.Cos( -visionHalfAngle * Mathf.Deg2Rad ), visionRange * Mathf.Sin( -visionHalfAngle * Mathf.Deg2Rad ) );

        visionTriangle.SetPath( 0, path );
    }

    public void PlayerSeen()
    {
        // !
        // play MGS sound
        Debug.Log( "Spotted!" );
    }
}
