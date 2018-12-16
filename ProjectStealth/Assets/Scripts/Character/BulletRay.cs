using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Fires a fast-moving projectile that raycasts to determine if collisions happened.
public class BulletRay : MonoBehaviour
{
    #region vars
    public float angle; // radians
    public float speed = 640.0f; // pixels per second
    public bool is_player_owned = false;
    public float damage = 50.0f;

    private const float MAX_LENGTH = 64.0f; // pixels
    private float current_length;
    private LineRenderer line_renderer;
    private bool is_dying = false;
    #endregion

    // Use this for early initialization
    private void Awake()
    {
        line_renderer = GetComponent<LineRenderer>();
    }

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        transform.eulerAngles = new Vector3( 0.0f, 0.0f, angle * Mathf.Rad2Deg );

        float distance = speed * Time.deltaTime * Time.timeScale;
        Vector2 direction = new Vector2( Mathf.Cos( angle ), Mathf.Sin( angle ) );

        if ( ! is_dying )
        {
            // Move until you hit a wall.
            // Lengthen tail until max
            current_length = Mathf.Min( current_length + distance, MAX_LENGTH );
            line_renderer.SetPosition( 0, new Vector3( -current_length, 0.0f, 0.0f ) );

            // Check if you hit a wall or enemy
            int mask;
            if ( is_player_owned ) { mask = CollisionMasks.player_shooting_mask; }
            else { mask = CollisionMasks.enemy_shooting_mask; }
            RaycastHit2D hit = Physics2D.Raycast( transform.position, direction, distance + 1.0f, mask );

            // Now, move visually.
            transform.Translate( new Vector3( direction.x * distance, direction.y * distance, 0.0f ), Space.World );

            if ( hit.collider == null ) { return; } // Didn't hit anything, keep going.
            // It hit something.
            // Player?
            if ( Utils.IsPlayersCollider( hit.collider ) )
            {
                if ( ! is_player_owned ) 
                {
                    PlayerStats char_stats = hit.collider.gameObject.GetComponent<PlayerStats>();
                    if ( char_stats == null ) { return; }
                    char_stats.Hit( damage );
                    is_dying = true;
                }
                else { return; } // player can't hit themselves with their own bullet. Ignore this hit.
            }
            // Enemy?
            if ( Utils.IsEnemyCollider( hit.collider ) )
            {
                if ( is_player_owned )
                {
                    EnemyStats enemy_stats = hit.collider.gameObject.GetComponent<EnemyStats>();
                    if ( enemy_stats == null )
                    {
                        #if UNITY_EDITOR
                        Debug.LogError( "An enemy was hit, but was missing an EnemyStats component." );
                        #endif
                        return;
                    }
                    enemy_stats.Hit( damage );
                    is_dying = true;
                }
                else { return; } // enemies can't hit themselves. Ignore this hit.
            }

            // prepare to clean up.
            transform.Translate( new Vector3( direction.x * -distance, direction.y * -distance, 0.0f ) ); // undo
            transform.Translate( new Vector3( direction.x * hit.distance, direction.y * hit.distance, 0.0f ) ); // move to impact point.
            is_dying = true;
        }
        else
        {
            // shorten tail.
            current_length = Mathf.Max( current_length - distance, 0.0f );
            line_renderer.SetPosition( 0, new Vector3( -current_length, 0.0f, 0.0f ) );

            if ( current_length <= 0.0f )
            {
                // TODO: impact sparks, noise?
                Destroy( this.gameObject );
            }
        }
    }
}
