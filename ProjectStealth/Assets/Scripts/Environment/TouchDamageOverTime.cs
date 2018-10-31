using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script that causes continuous damage while a player is touching something, like acid.
// - Gabriel Violette
public class TouchDamageOverTime : MonoBehaviour
{
    #region vars
    [SerializeField]
    private float damage_per_second = 100.0f;
    #endregion

    // Use this for initialization
    void Start ()
    {
        #region config checks
        // Validate the build.
        Collider2D collider = GetComponent<BoxCollider2D>();
        if ( collider == null ) { collider = GetComponent<CircleCollider2D>(); }

        if ( collider == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing component: Collider2D on " + this.gameObject );
            #endif
            Destroy( this );
            return;
        }
        if ( ! collider.isTrigger )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Invalid configuration: Collider2D is not a trigger on " + this.gameObject );
            #endif
            Destroy( this );
            return;
        }

        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        if ( rigidbody == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing component: Rigidbody2D on " + this.gameObject );
            #endif
            Destroy( this );
            return;
        }
        if ( ! rigidbody.isKinematic )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Invalid configuration: Rigidbody2D is not kinematic on " + this.gameObject );
            #endif
            Destroy( this );
            return;
        }
        #endregion
    }

    private void OnTriggerEnter2D( Collider2D collision )
    {
        // TODO: play sound / indicate
    }

    private void OnTriggerStay2D( Collider2D collision )
    {
        if ( Utils.IsPlayersCollider( collision ) )
        {
            Referencer.instance.player.Hit( Time.deltaTime * Time.timeScale * damage_per_second );
        }
        // TODO: work on enemies, too.
    }

    private void OnTriggerExit2D( Collider2D collision )
    {
        // TODO: indicator cleanup?
    }
}
