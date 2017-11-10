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
    #endregion

    // Use this for pre-initialization
    private void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {
        Referencer.Instance.AddEnemy( this.gameObject );
    }

    // Update is called once per frame
    void Update()
    {

        // if you're not already on alert...
        NoiseCheck();
    }

    private void NoiseCheck()
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
}
