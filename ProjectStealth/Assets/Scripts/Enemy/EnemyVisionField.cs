using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// - GabeV
//Class for enemy vision fields
public class EnemyVisionField : MonoBehaviour
{
    #region vars
    public Enemy enemy; // number one
    #endregion


    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        // Initialize line renderer to show the vision field.
        LineRenderer line_renderer  = gameObject.AddComponent<LineRenderer>();
        line_renderer.material = new Material( Shader.Find( "Particles/Additive" ) );
        line_renderer.startColor = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
        line_renderer.endColor = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
        line_renderer.startWidth = 1.0f;
        line_renderer.endWidth = 1.0f;
        line_renderer.positionCount = 4;
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        // Show the vision field in debug mode.
        LineRenderer line_renderer = gameObject.GetComponent<LineRenderer>();
        if ( line_renderer == null )
        {
            Debug.LogError( "Vision field line renderer is null." );
        }

        PolygonCollider2D vision_polygon = gameObject.GetComponent<PolygonCollider2D>();
        Vector3 scale = this.gameObject.transform.localScale;
        for ( int i = 0; i < vision_polygon.points.Length; i++ )
        {
            line_renderer.SetPosition( i, this.gameObject.transform.position + new Vector3( vision_polygon.points[ i ].x * scale.x, vision_polygon.points[ i ].y * scale.y, 0.0f ) );
        }
        line_renderer.SetPosition( vision_polygon.points.Length, this.gameObject.transform.position + new Vector3( vision_polygon.points[ 0 ].x * scale.x, vision_polygon.points[ 0 ].y * scale.y, 0.0f ) );
#endif
    }

    private void OnTriggerEnter2D( Collider2D collision )
    {
        // has enemy already seen you?
        // TODO:

        if ( Utils.IsPlayersCollider( collision ) )
        {
            Look();
        }
    }

    private void OnTriggerStay2D( Collider2D collision )
    {
        // has enemy already seen you?
        // TODO:

        if ( Utils.IsPlayersCollider( collision ) )
        {
            Look();
        }
    }

    private void OnTriggerExit2D( Collider2D collision )
    {

    }

    private void Look()
    {
        // check if player is stealthed. Skip all these checks if they are.
        if ( Referencer.instance.player.IsCloaking() ) { Debug.Log( "unseen: cloaked" ); return; }

        // confirm seen by raycasting against player + wall colliders, ignoring triggers...
        // for all collisions, check if distance < distance to player
        Vector3 player_position = Referencer.instance.player.gameObject.transform.position;
        Vector3 enemy_position = this.transform.parent.gameObject.transform.position;
        Vector2 origin = new Vector2( enemy_position.x, enemy_position.y );
        Vector2 direction = new Vector2( player_position.x - enemy_position.x, player_position.y - enemy_position.y );
        float distance_to_player = Mathf.Sqrt( Mathf.Pow( player_position.x - enemy_position.x, 2) + Mathf.Pow( player_position.y - enemy_position.y, 2) );
        int layer_mask = LayerMask.GetMask( "geometry" );
        RaycastHit2D hit; // array?
        hit = Physics2D.Raycast( origin, direction, distance_to_player + 1.0f, layer_mask );
        // TO CONSIDER:
        // Overkill? if center to center raycast fails, try center to top/bottom right/left of player hitbox (allows game to have enemies see feet / heads poking out)
        //Physics2D.RaycastAll( origin, direction, distanceToPlayer + 1.0f, layerMask );

        if ( hit.collider == null )
        {
            // no walls between the player and the enemy, YOU WERE SEEN!
            // if player, send "seen" message to enemy
            //enemy.DoSomething();
            Debug.Log( "You were seen!" );
            GameState.instance.is_red_alert = true; // testing.
        }
        else
        {
            Debug.Log( "unseen: occluded" );
        }
    }
}
