using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Instantly kills the player when they touch this object.
public class KillOnTouch : MonoBehaviour
{

    // Use this for initialization
    void Start ()
    {
        if ( ! TriggerBuildValidator.Validate( this.gameObject ) ) { Destroy( this ); }
    }

    // Called when a collider enters this object's hitbox
    private void OnTriggerEnter2D( Collider2D collision )
    {
        if ( Utils.IsPlayersCollider( collision ) )
        {
            Referencer.instance.player.Kill();
        }
        // TODO: consider damaging enemies, too.
    }
}
