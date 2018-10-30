using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Fires a laser from a point to either max range, or a wall.
public class LaserBeam : MonoBehaviour
{

    #region vars
    public bool is_on = true;

    private Transform start_light;
    private Transform end_light;
    private ParticleSystem particles;
    private const float MAX_RANGE = 500.0f;
    #endregion

    // Use this for initialization
    void Start ()
    {
        start_light = transform.Find( "Start" );
        end_light   = transform.Find( "End" );
        particles   = transform.Find( "Particles" ).GetComponent<ParticleSystem>();
    }
    
    // Update is called once per frame
    void Update ()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        SpriteRenderer sprite_renderer = GetComponent<SpriteRenderer>();

        // If off
        if ( ! is_on )
        {
            sprite_renderer.size = new Vector2( 0.0f, 0.0f );
            collider.enabled = false;
            start_light.gameObject.SetActive( false );
            end_light.gameObject.SetActive( false );
            particles.gameObject.SetActive( false );
            return;
        }

        // Laser should end at the first solid object it touches.
        float angle = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 direction = new Vector2( Mathf.Cos( angle ), Mathf.Sin( angle ) );

        RaycastHit2D hit = Physics2D.Raycast( transform.position, direction, MAX_RANGE, CollisionMasks.geo_mask | CollisionMasks.jump_through_mask | CollisionMasks.object_mask );
        float range;
        if ( hit.collider == null )
        {
            range = MAX_RANGE;
        }
        else
        {
            range = hit.distance;
        }

        // set up collider and graphics (rotation of transform handles rotational stuff)
        sprite_renderer.size = new Vector2( range, 8.0f );
        collider.enabled = true;
        collider.size = new Vector2( range, collider.size.y );
        collider.offset = new Vector2( range / 2.0f, 0.0f);

        start_light.gameObject.SetActive( true );
        end_light.gameObject.SetActive( true );
        end_light.transform.position = new Vector3( transform.position.x + Mathf.Cos( angle ) * range, transform.position.y + Mathf.Sin( angle ) * range, 0.0f );
        start_light.GetComponent<SpriteRenderer>().color = new Color( 1.0f, 0.8f, 0.8f, Random.Range( 0.5f, 0.75f ) );
        end_light.GetComponent<SpriteRenderer>().color   = new Color( 1.0f, 0.8f, 0.8f, Random.Range( 0.5f, 0.75f ) );

        // For some reason the particlesystem API is awkward.
        particles.gameObject.SetActive( true );
        //ParticleSystem.MainModule main = particles.main;
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.radius = range / 2.0f;
        shape.position = new Vector3( range / 2.0f, 0.0f, 0.0f );
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = range / 24.0f * 160.0f;

    }
}
