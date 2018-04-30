using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-GabeV
//A basic enemy AI
public class Enemy : MonoBehaviour
{
    #region vars
    private bool canHear = true;
    private float listening_radius = 0.0f;

    // snooze, vigilance, investigate, alert, combat

    #region vision
    public GameObject vision_subobject;
    private PolygonCollider2D vision_triangle;
    public float vision_half_angle = 30.0f; // degrees
    public float vision_range = 100.0f; // pixels
    #endregion

	private float fire_rate = 1.0f;
	private float fire_timer = 0.0f;
	public GameObject bullet_prefab;
    #endregion

    // Use this for pre-initialization
    private void Awake()
    {
		if ( vision_subobject == null )
		{
			vision_subobject = this.gameObject.transform.Find ("Vision Field").gameObject;
		}
    }

    // Use this for initialization
    void Start()
    {
        Referencer.instance.RegisterEnemy( this.gameObject );

        vision_subobject.GetComponent<EnemyVisionField>().enemy = this; // circular reference
        vision_triangle = vision_subobject.GetComponent<PolygonCollider2D>();
        if ( vision_half_angle >= 90.0f || vision_half_angle < 0.0f )
        {
            Debug.Log( "Invalid vision cone angle!" );
            vision_half_angle = 30.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // if you're not already on alert...
        Listen();
        Watch();

		if ( GameState.instance.is_red_alert )
		{
			fire_timer += Time.deltaTime * TimeScale.timeScale;
			if ( fire_timer > 1.0f / fire_rate )
			{
				fire_timer = 0.0f;
				Bullet bullet = Instantiate( bullet_prefab , this.transform.position, Quaternion.identity).GetComponent<Bullet>();
				Vector3 player_position = Referencer.instance.player.transform.position;
				bullet.Angle = Mathf.Atan2( player_position.y - transform.position.y, player_position.x - transform.position.x );
			}
		}
    }

    /// <summary>
    /// Detect noise caused by the player
    /// </summary>
    private void Listen()
    {
        if ( ! canHear ) { return; }

        foreach ( Noise noise in Referencer.instance.noises )
        {
            if ( IsDistanceBetweenPointsLessThan( noise.position.x, noise.position.y, this.gameObject.transform.position.x, this.gameObject.transform.position.y, noise.radius + listening_radius ) )
            {
                // Detected!
                // TODO: if available, set investigation mode
				#if UNITY_EDITOR
                Debug.Log( "Sound detected!" );
				#endif
				GameState.instance.is_red_alert = true; // test
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
        path[1] = new Vector2( vision_range * Mathf.Cos( vision_half_angle * Mathf.Deg2Rad ), vision_range * Mathf.Sin( vision_half_angle * Mathf.Deg2Rad ) );
        path[2] = new Vector2( vision_range * Mathf.Cos( -vision_half_angle * Mathf.Deg2Rad ), vision_range * Mathf.Sin( -vision_half_angle * Mathf.Deg2Rad ) );

        vision_triangle.SetPath( 0, path );
    }

    public void PlayerSeen()
    {
        // !
        // play MGS sound
        Debug.Log( "Spotted!" );
    }
}
