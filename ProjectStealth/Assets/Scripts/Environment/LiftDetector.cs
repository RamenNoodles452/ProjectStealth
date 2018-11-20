using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to detect when a player is on a lift/moving platform.
// Attached to a separate trigger collider, not the platform's geometry collider.
public class LiftDetector : MonoBehaviour
{
    #region vars
    FollowPath platform_path;
    #endregion

    // Use this for initialization
    void Start()
    {
        // Validate the build.
        if ( ! TriggerBuildValidator.Validate( this.gameObject ) )
        {
            Destroy( this );
            return;
        }

        platform_path = GetComponentInParent<FollowPath>(); //TODO: arc path support?
        if ( platform_path == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Lift configuration issue: lift is not a moving platform" );
            #endif
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
