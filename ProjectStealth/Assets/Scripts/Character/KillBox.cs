using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// GabeV
/// <summary>
/// Kills the player (character) if they step out of the level
/// </summary>
public class KillBox : MonoBehaviour
{
    #region vars
    private Player player;
    private BoxCollider2D bounding_box;
    #endregion

    /// <summary>
    /// Used for early initialization
    /// </summary>
    private void Awake()
    {
        bounding_box = this.gameObject.GetComponent<BoxCollider2D>();
    }

    // Use this for initialization
    void Start()
    {
        player = Referencer.instance.player;
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
        float min_x = bounding_box.transform.position.x + bounding_box.offset.x - bounding_box.size.x / 2.0f;
        float max_x = bounding_box.transform.position.x + bounding_box.offset.x + bounding_box.size.x / 2.0f;
        float min_y = bounding_box.transform.position.y + bounding_box.offset.y - bounding_box.size.y / 2.0f;
        float max_y = bounding_box.transform.position.y + bounding_box.offset.y + bounding_box.size.y / 2.0f;

        if ( playerCenter.x >= min_x && playerCenter.x <= max_x && playerCenter.y >= min_y && playerCenter.y <= max_y )
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
