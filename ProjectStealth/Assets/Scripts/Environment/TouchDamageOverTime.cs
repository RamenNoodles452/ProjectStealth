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
        if ( ! TriggerBuildValidator.Validate( this.gameObject ) ) { Destroy( this ); }
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
