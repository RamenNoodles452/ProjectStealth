using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletChargedRay : MonoBehaviour
{
    #region vars
    private bool is_player_owned = false;

    private LineRenderer line_renderer;
    private LineRenderer central_line_renderer;

    private float timer = 0.0f;
    private const float MAX_WIDTH = 12.0f; // pixels
    private const float DURATION = 0.5f; // seconds
    #endregion

    // Use this for early initialization
    private void Awake()
    {
        line_renderer = GetComponent<LineRenderer>();
        central_line_renderer = transform.GetChild( 0 ).GetComponent<LineRenderer>();
    }

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        if ( timer >= DURATION )
        {
            return;
        }

        float t = Mathf.Min( timer, DURATION ) / DURATION;
        float width = Mathf.Lerp( MAX_WIDTH, 0.0f, t );
        line_renderer.widthMultiplier = width;
        central_line_renderer.widthMultiplier = Mathf.Max( 0.0f, width - 6.0f );

        float g = Mathf.Lerp( 0.75f, 0.25f, t );
        Color color = new Color( 0.0f, g, 1.0f, 1.0f );
        line_renderer.startColor = color;
        line_renderer.endColor = color;

        // TODO: screen shake
        if ( is_player_owned ) {  }

        timer += Time.deltaTime * Time.timeScale;
    }

    /// <summary>
    /// Used to set up the visual display and damage data of the charged ray laser beam
    /// </summary>
    /// <param name="start_point">The origin of the ray.</param>
    /// <param name="end_point">The end of the ray.</param>
    /// <param name="damage">The amount of damage the ray will deal to the target it hits.</param>
    /// <param name="is_player_owned">True if this charged beam is from the player, false otherwise.</param>
    public void Setup( Vector2 start_point, Vector2 end_point, float damage, bool is_player_owned )
    {
        this.is_player_owned = is_player_owned;

        float distance = Vector2.Distance( start_point, end_point );
        line_renderer.SetPosition( 1, new Vector3( distance, 0.0f, 0.0f ) );
        central_line_renderer.SetPosition( 1, new Vector3( distance, 0.0f, 0.0f ) );
        
        ParticleSystem particle_system = GetComponentInChildren<ParticleSystem>();
        ParticleSystem.ShapeModule shape = particle_system.shape;
        shape.scale = new Vector3( distance, 1.0f, 1.0f );
        shape.position = new Vector3( distance / 2.0f, 0.0f, 0.0f );

        float angle = Mathf.Atan2( end_point.y - start_point.y, end_point.x - start_point.x );
        transform.Rotate( new Vector3( 0.0f, 0.0f, angle * Mathf.Rad2Deg ) );

        // hit detection and damage
        int mask = CollisionMasks.player_shooting_mask;
        if ( ! is_player_owned ) { mask = CollisionMasks.enemy_shooting_mask; }
        RaycastHit2D hit = Physics2D.Raycast( start_point, end_point-start_point, distance + 1.0f, mask );
        if ( hit.collider == null ) { return; } // didn't hit anything
        if ( Utils.IsPlayersCollider( hit.collider ) )
        {
            if ( ! is_player_owned )
            {
                PlayerStats char_stats = hit.collider.gameObject.GetComponent<PlayerStats>();
                if ( char_stats == null ) { return; }
                char_stats.Hit( damage );
            }
            else { return; }
        }
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
            }
            else { return; }
        }
    }
}
