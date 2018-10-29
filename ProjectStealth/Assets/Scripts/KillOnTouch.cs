using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnTouch : MonoBehaviour
{

    // Use this for initialization
    void Start ()
    {
        
    }

    // Update is called once per frame
    void Update ()
    {
        
    }

    // Called when a collider enters this object's hitbox
    private void OnTriggerEnter2D( Collider2D collision )
    {
        if ( Utils.IsPlayersCollider( collision ) )
        {
            Referencer.instance.player.Kill();
        }
    }

    /*
    private void OnTriggerExit2D( Collider2D collision )
    {
        if ( Utils.IsPlayersCollider( collision ) )
        {

        }
    }
    */
}
