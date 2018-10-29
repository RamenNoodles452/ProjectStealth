using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftDetector : MonoBehaviour
{
    #region vars
    PlatformPatrol platform_path;
    #endregion

    // Use this for initialization
    void Start()
    {
        platform_path = GetComponentInParent<PlatformPatrol>();
        if ( platform_path == null )
        {
            Debug.LogError( "Lift configuration issue!" );
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Called when a collider enters this object's hitbox
    void OnTriggerEnter2D( Collider2D other )
    {
        if ( Utils.IsPlayersCollider( other ) )
        {
            platform_path.AttachPlayer();

            //reset state from climbing, etc.
            if ( Referencer.instance.player.GetComponent<CharacterStats>().current_master_state == CharEnums.MasterState.ClimbState )
            {
                Referencer.instance.player.GetComponent<MagGripUpgrade>().StopClimbing();
            }
        }
    }

    // Called when a collider exits the object's hitbox
    void OnTriggerExit2D( Collider2D other )
    {
        if ( Utils.IsPlayersCollider( other ) )
        {
            platform_path.DetachPlayer();
        }
    }
}
