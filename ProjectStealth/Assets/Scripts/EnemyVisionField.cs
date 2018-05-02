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
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnTriggerEnter2D( Collider2D collision )
    {
        // has enemy already seen you?
        // TODO:

		Look ();
    }

    private void OnTriggerStay2D( Collider2D collision )
    {
		// has enemy already seen you?
		// TODO:

		Look ();
    }

    private void OnTriggerExit2D( Collider2D collision )
    {
        
    }

	private void Look()
	{
		// check if player is stealthed. Skip all these checks if they are.
		if ( Referencer.instance.player.IsCloaking() ) { Debug.Log("unseen: cloaked"); return; }

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
