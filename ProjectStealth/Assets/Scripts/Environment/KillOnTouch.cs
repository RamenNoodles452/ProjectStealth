using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Instantly kills the player when they touch this object.
public class KillOnTouch : MonoBehaviour
{

    // Use this for initialization
    void Start ()
    {
        #region config checks
        // Validate the build.
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if ( collider == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing component: BoxCollider2D." );
            #endif
            Destroy( this );
            return;
        }
        if ( !collider.isTrigger )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Invalid configuration: BoxCollider2D is not a trigger." );
            #endif
            Destroy( this );
            return;
        }

        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        if ( rigidbody == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing component: Rigidbody2D." );
            #endif
            Destroy( this );
            return;
        }
        if ( !rigidbody.isKinematic )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Invalid configuration: Rigidbody2D is not kinematic." );
            #endif
            Destroy( this );
            return;
        }
        #endregion
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
