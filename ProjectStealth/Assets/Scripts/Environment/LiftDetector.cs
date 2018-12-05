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
            HorizontallyAlignWithPlayer();

            //reset state from climbing, etc.
            if ( Referencer.instance.player.GetComponent<CharacterStats>().current_master_state == CharEnums.MasterState.ClimbState )
            {
                Referencer.instance.player.GetComponent<CharacterAnimationLogic>().WallSlideTouchGround(); // play slide down and touch ground animation
                Referencer.instance.player.GetComponent<MagGripUpgrade>().StopClimbing();
            }
        }
    }

    // Called while a collider is in this object's hitbox
    private void OnTriggerStay2D( Collider2D other )
    {
        if ( Utils.IsPlayersCollider( other ) )
        {
            if ( Referencer.instance.player.BecameIdleThisFrame )
            {
                HorizontallyAlignWithPlayer();
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

    /// <summary>
    /// Aligns the sub-pixel x coordinate offset of this object with the player's sub-pixel x coordinate offset, so they move together smoothly.
    /// The slight transform should not be noticable, should not aggregate when performed multiple times, and should be removed by pathing AI, making this change STABLE.
    /// </summary>
    private void HorizontallyAlignWithPlayer()
    {
        // C# modulus is remainder, want modulo.
        float player_non_int = ( ( Referencer.instance.player.transform.position.x % 1.0f ) + 1.0f ) % 1.0f;

        // Since sprite position is truncated via (int), x = (n, n +1) are the same pixel as current x. We want to choose a new x coordinate in that range.
        float pixel_x = (int) transform.parent.position.x;
        float new_x = pixel_x + player_non_int;

        // Prevent negative numbers from shifting 1 px every time. (+1.0 + 0.2 = +1.2, still px coordinate of 1. -1.0 + 0.2 = -0.8, shifted px coordinate by 1.)
        if ( pixel_x < 0.0f && player_non_int > 0.0f) { new_x -= 1.0f; }

        transform.parent.position += new Vector3( new_x - transform.parent.position.x, 0.0f, 0.0f );

        //Debug.Log( transform.parent.position.x + " => " + Referencer.instance.player.transform.position.x );

        // If sprite position is rounded, x = (n - 0.5, n + 0.5) are the same pixel as current x. We want to choose a new x coordinate in that range.
        //if ( transform.parent.position.x >= 0 ) { pixel_x = Mathf.Floor( transform.parent.position.x + 0.5f ); } // 280.49 -> 280, 279.5 -> 280
        //else { pixel_x = Mathf.Ceil( transform.parent.position.x - 0.5f ); } // -280.5 -> -281, -280.49 -> -280, -279.5 -> -280
        //float new_x = pixel_x + player_non_int; // [ 0, 1 )
        //if ( player_non_int > 0.5f ) { new_x = new_x - 1.0f; }
    }
}
