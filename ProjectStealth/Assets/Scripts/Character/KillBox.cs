using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Kills the player (character) if they step out of the level
public class KillBox : MonoBehaviour
{
    Player player;
    public BoxCollider2D boundBox;

	// Use this for initialization
	void Start ()
    {
        player = Referencer.Instance.player; //GameObject.Find("PlayerCharacter").GetComponent<Player>(); //bad
        boundBox = this.gameObject.GetComponent<BoxCollider2D>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if ( player == null )
        {
            Debug.Log( "Null player reference in KillBox" ); // in lieu of exception
            return;
        }

        Vector2 playerCenter = player.CenterPoint();

        // bound box should be at (0,0), but use position just in case
        float minx = boundBox.transform.position.x + boundBox.offset.x - boundBox.size.x / 2.0f;
        float maxx = boundBox.transform.position.x + boundBox.offset.x + boundBox.size.x / 2.0f;
        float miny = boundBox.transform.position.y + boundBox.offset.y - boundBox.size.y / 2.0f;
        float maxy = boundBox.transform.position.y + boundBox.offset.y + boundBox.size.y / 2.0f;

        if ( playerCenter.x >=  minx && playerCenter.x <= maxx && playerCenter.y >= miny && playerCenter.y <= maxy)
        {
            //ok
        }
        else
        {
            Debug.Log( "Player went outside the map." );
            player.Kill(); //may need to specify animation
        }
	}
}
