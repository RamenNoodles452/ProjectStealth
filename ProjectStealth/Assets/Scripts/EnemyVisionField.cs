using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-GabeV
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
        // check if player is stealthed. Skip all these checks if they are.
        if ( Referencer.Instance.player.IsCloaking() ) { return; }

        // has enemy already seen you?
        // TODO:

        // confirm seen by raycasting against player + wall colliders, ignoring triggers...
        // for all collisions, check if distance < distance to player
        // what is player mask?
        // what is wall mask?
        Vector3 playerPosition = Referencer.Instance.player.gameObject.transform.position;
        Vector3 enemyPosition = this.transform.parent.gameObject.transform.position;
        Vector2 origin = new Vector2( enemyPosition.x, enemyPosition.y );
        Vector2 direction = new Vector2( playerPosition.x - enemyPosition.x, playerPosition.y - enemyPosition.y );
        float distanceToPlayer = Mathf.Sqrt( Mathf.Pow( playerPosition.x - enemyPosition.x, 2) + Mathf.Pow( playerPosition.y - enemyPosition.y, 2) );
        LayerMask layerMask = new LayerMask();

        //CollisionMasks.CharacterMask;

        //RaycastHit2D hit; // array?
        //Physics2D.RaycastAll( origin, direction, distanceToPlayer + 1.0f, layerMask );

        // Overkill? if center to center raycast fails, try center to top/bottom right/left of player hitbox (allows game to have enemies see feet / heads poking out)

        // if player, send "seen" message to enemy
        //enemy.DoSomething();
    }

    private void OnTriggerStay2D( Collider2D collision )
    {
        // check if player is stealthed. Skip all these checks if they are.
        if ( Referencer.Instance.player.IsCloaking() ) { return; }

        // has enemy already seen you?
        // TODO:
    }

    private void OnTriggerExit2D( Collider2D collision )
    {
        
    }
}
