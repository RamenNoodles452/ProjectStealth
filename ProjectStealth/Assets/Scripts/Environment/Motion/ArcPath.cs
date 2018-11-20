using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Arc path: for pendulums and spinning objects
// - Gabriel Violette
public class ArcPath : MonoBehaviour
{
    #region vars
    [SerializeField]
    private GameObject center_object; // if set, center will follow this object, and radius will be calculated based on initial distance.
    public Vector2 center;
    public float speed = 24.0f;

    [SerializeField]
    private float radius = 24.0f;
    [SerializeField]
    bool is_full_circle = false; // If true, just loop. Else, pendulum.
    [SerializeField]
    private float start_angle = 0.0f; // in degrees
    [SerializeField]
    private float end_angle = 180.0f; // in degrees

    private float angular_velocity;
    private float angle;
    #endregion

    // Use this for initialization
    void Start ()
    {
        if ( center_object != null )
        {
            center = (Vector2) center_object.transform.position;
            radius = Vector2.Distance( center, (Vector2) transform.position );
        }

        // linear speed (in pixels / second) / circumference (in pixels) = cycles per second
        angular_velocity = speed / ( Mathf.PI * 2.0f * radius ) * 360.0f;
        // flip?
        if      ( angular_velocity > 0.0f && start_angle > end_angle ) { speed = -speed; }
        else if ( angular_velocity < 0.0f && start_angle < end_angle ) { speed = -speed; }
    }
    
    // Update is called once per frame
    void Update ()
    {
        if ( center_object != null ) { center = (Vector2) center_object.transform.position; }

        angular_velocity = speed / ( Mathf.PI * 2.0f * radius ) * 360.0f;
        float theta = angular_velocity * Time.deltaTime * Time.timeScale;
        // need to anchor to a centerpoint object, offset from it. Moving the centerpoint is doable, then.
        angle += theta;

        // keep in range
        if ( is_full_circle )
        {
            while ( angle > 360.0f ) { angle -= 360.0f; }
            while ( angle <   0.0f ) { angle += 360.0f; }
        }
        else // pendulum motion
        {
            float max_angle = Mathf.Max( start_angle, end_angle );
            float min_angle = Mathf.Min( start_angle, end_angle );

            if      ( angle > max_angle ) { speed = -speed; }
            else if ( angle < min_angle ) { speed = -speed; }
        }

        transform.position = new Vector3( center.x + radius * Mathf.Cos( angle * Mathf.Deg2Rad ), center.y + radius * Mathf.Sin( angle * Mathf.Deg2Rad ), transform.position.z );
    }
}
