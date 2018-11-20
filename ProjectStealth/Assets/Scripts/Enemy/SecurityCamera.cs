using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    #region vars
    // range of motion
    private float angle;
    private float anchor_angle = 270.0f;          // initial (anchored) facing, in degrees
    private float relative_min_rotation = -45.0f; // the extent of allowed rotation from anchor, in degrees
    private float relative_max_rotation =  45.0f; // the extent of allowed rotation from anchor, in degress
    private float rotational_speed = 67.5f;       // degrees per second
    private float angle_target;

    // detection sector
    private float detection_range = 128.0f;       // arc radius, in pixels
    private float detection_half_angle = 30.0f;   // arc degrees

    private const float alert_delay = 1.0f;
    private float alert_timer = 0.0f;
    private bool is_in_view = false;
    private bool is_noticed = false;
    [SerializeField]
    private bool is_alerted = false;
    #endregion

    // Use this for initialization
    void Start ()
    {
        angle_target = anchor_angle;
        SetAngle( anchor_angle );
       // GetComponent<CircleCollider2D>().radius = detection_range; // not using collider anymore bc more performant not to

        #if UNITY_EDITOR
        if ( relative_max_rotation < 0.0f ) { Debug.LogError( "Invalid configuration: negative max" ); }
        if ( relative_min_rotation > 0.0f ) { Debug.LogError( "Invalid configuration: positive min" ); }
        #endif
    }
    
    // Update is called once per frame
    void Update ()
    {
        Detect();

        //Set color
        if ( is_alerted )
        {
            GetComponent<SpriteRenderer>().color = new Color( 1.0f, 0.0f, 0.0f, 0.375f );
        }
        else if ( is_in_view )
        {
            GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 0.0f, 0.375f );
        }
        else
        {
            GetComponent<SpriteRenderer>().color = new Color( 0.0f, 1.0f, 0.0f, 0.375f );
        }

        // Tracking logic
        if ( is_noticed )
        {
            float angle_to_player = AngleToPlayer( Referencer.instance.player.transform.position );
            angle_target = angle_to_player;

            if ( is_in_view )
            {
                alert_timer += Time.deltaTime * Time.timeScale;
            }
            else
            {
                alert_timer -= Time.deltaTime * Time.timeScale;
            }

            if ( alert_timer >= alert_delay )
            {
                is_alerted = true;
            }
            else if ( alert_timer <= -1.0f )
            {
                is_noticed = false;
                angle_target = anchor_angle;
            }
        }

        float sign = 0.0f;
        if ( angle < angle_target ) { sign = 1.0f; }
        else if ( angle > angle_target ) { sign = -1.0f; }

        float increment = sign * rotational_speed * Time.deltaTime * Time.timeScale;
        if      ( angle + increment > anchor_angle + relative_max_rotation )  { angle = anchor_angle + relative_max_rotation; }
        else if ( angle + increment < anchor_angle + relative_min_rotation )  { angle = anchor_angle + relative_min_rotation; }
        else if ( angle <= angle_target && angle + increment > angle_target ) { angle = angle_target; }
        else if ( angle >= angle_target && angle + increment < angle_target ) { angle = angle_target; }
        else { angle = angle + increment; }

        SetAngle( angle );

        // reset per-frame data
        is_in_view = false;
    }

    /*
    private void OnTriggerEnter2D( Collider2D collision )
    {
        Detect( collision );
    }

    private void OnTriggerStay2D( Collider2D collision )
    {
        Detect( collision );
    }

    private void OnTriggerExit2D( Collider2D collision )
    {
        //TODO:
    }
    */

    private void Detect() // Collider2D collision )
    {
        //if ( ! Utils.IsPlayersCollider( collision ) )  { return; } // Not player, don't care.
        if ( Referencer.instance.player.IsCloaking() ) { return; } // Can't see them anyway, don't bother.

        // out of sight range?
        Vector3 player_position = Referencer.instance.player.gameObject.transform.position;
        if ( Vector2.Distance( (Vector2) player_position, (Vector2) transform.position ) > detection_range ) { return; }

        float angle_to_player = AngleToPlayer( player_position );
        if ( Mathf.Abs( angle_to_player - angle ) > detection_half_angle ) { return; }

        // visible.
        is_in_view = true;
        
        is_noticed = true;
        alert_timer = Mathf.Max( alert_timer, 0.0f );
    }

    private void SetAngle( float angle )
    {
        this.angle = angle;
        transform.parent.transform.rotation = Quaternion.Euler( 0.0f, 0.0f, angle );
    }

    private float AngleToPlayer( Vector3 player_position )
    {
        float angle_to_player = Mathf.Atan2( player_position.y - transform.position.y, player_position.x - transform.position.x ) * Mathf.Rad2Deg;
        while ( angle_to_player <   0.0f ) { angle_to_player += 360.0f; }
        while ( angle_to_player > 360.0f ) { angle_to_player -= 360.0f; }
        return angle_to_player;
    }
}
