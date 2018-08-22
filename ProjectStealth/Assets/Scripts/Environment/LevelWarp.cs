using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// - GabeV
// Instantiable object to link players to a different level/scene.
public class LevelWarp : MonoBehaviour
{
    #region vars
    public string level_name;
    public Vector2 warp_position;
    #endregion

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D( Collider2D collision )
    {
        if ( collision.GetComponent<Player>() != null )
        {
            Debug.Log( "Move to another level!" );
            GameState.instance.WarpToLevel( level_name, warp_position );
        }
    }
}
