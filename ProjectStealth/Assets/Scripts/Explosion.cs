using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ~Deals damage in a radius.
/// </summary>
public class Explosion : MonoBehaviour
{
    #region vars
    public float radius = 32.0f; // in pixels
    public float damage = 0.0f;
    public bool is_player_immune = false;
    public bool is_enemy_immune = false;
    #endregion

    // Use this for initialization
    void Start ()
    {
        if ( ! TriggerBuildValidator.Validate( this.gameObject ) ) { return; }

        // Can't just do a radius test b/c that would be center-center. We want to hit if a foot or arm gets hit, too.
        CircleCollider2D circle_collider = GetComponent<CircleCollider2D>();
        circle_collider.radius = radius;
    }

    /// <summary>
    /// Triggered when another collider enters this object's trigger radius.
    /// </summary>
    /// <param name="collision">The collider2D of the other object, within this explosion's radius</param>
    private void OnTriggerEnter2D( Collider2D collision )
    {
        Explode( collision );
    }

    /// <summary>
    /// Checks if a collider is a valid enemy or player and deals damage.
    /// </summary>
    /// <param name="collision">The collider2D of the object within this explosion's radius</param>
    private void Explode( Collider2D collision )
    {
        if ( Utils.IsPlayersCollider( collision ) )
        {
            if ( is_player_immune ) { return; }
            PlayerStats player_stats = collision.gameObject.GetComponent<PlayerStats>();
            if ( player_stats == null ) { return; }
            player_stats.Hit( damage );
        }
        else if ( Utils.IsEnemyCollider( collision ) )
        {
            if ( is_enemy_immune ) { return; }
            EnemyStats enemy_stats = collision.gameObject.GetComponent<EnemyStats>();
            if ( enemy_stats == null ) { return; }
            enemy_stats.Hit( damage );
        }
    }
}
