using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// GabeV
// Kills the player (character) if they step out of the level
public class KillBox : MonoBehaviour
{
    Player player;
    public BoxCollider2D bound_box;

    // Use this for initialization
    void Start()
    {
        player = Referencer.instance.player; //GameObject.Find("PlayerCharacter").GetComponent<Player>(); //bad
        bound_box = this.gameObject.GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if ( player == null )
        {
            Debug.Log( "Null player reference in KillBox" ); // in lieu of exception
            return;
        }

        Vector2 playerCenter = player.CenterPoint();

        // bound box should be at (0,0), but use position just in case
        float minx = bound_box.transform.position.x + bound_box.offset.x - bound_box.size.x / 2.0f;
        float maxx = bound_box.transform.position.x + bound_box.offset.x + bound_box.size.x / 2.0f;
        float miny = bound_box.transform.position.y + bound_box.offset.y - bound_box.size.y / 2.0f;
        float maxy = bound_box.transform.position.y + bound_box.offset.y + bound_box.size.y / 2.0f;

        if ( playerCenter.x >= minx && playerCenter.x <= maxx && playerCenter.y >= miny && playerCenter.y <= maxy )
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
