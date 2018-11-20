using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes a platform move around on a set patrol path.
public class FollowPath : MonoBehaviour
{
    #region vars
    private Path path;

    public float speed = 32.0f; // pixels / second
    private Vector3 delta;
    private float timer;

    private bool is_waiting  = false;
    private bool is_attached = false; //TODO: make this function for multiple enemies as well as the player.
    //private GameObject[] attached_objects;
    #endregion

    // Use this for initialization
    void Start()
    {
        path = GetComponent<Path>();


        #if UNITY_EDITOR
        if ( path == null )
        {
            Debug.LogError( "Path configured improperly: missing path." );
        }
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 aim = path.Current();
        float angle = Mathf.Atan2( aim.y - transform.position.y, aim.x - transform.position.x );
        float distance = Mathf.Sqrt( Mathf.Pow( aim.x - transform.position.x, 2.0f ) + Mathf.Pow( aim.y - transform.position.y, 2.0f ) );
        bool arrive = distance <= (speed * Time.deltaTime * Time.timeScale);

        if ( ! arrive )
        {
            delta = ( speed * Time.deltaTime * Time.timeScale ) * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
            transform.position += delta;
            if ( is_attached )
            {
                Referencer.instance.player.MoveWithCollision( delta );
            }
        }
        else
        {
            // Move
            delta = aim - transform.position;
            transform.position = aim;
            if ( is_attached )
            {
                Referencer.instance.player.MoveWithCollision( delta );
            }

            // Wait?
            if ( is_waiting )
            {
                timer += Time.deltaTime * Time.timeScale;
                if ( timer < path.CurrentDelay() ) { return; }
                is_waiting = false;
            }
            else
            {
                if ( path.CurrentDelay() > 0.0f )
                {
                    is_waiting = true;
                    timer = 0.0f;
                    return;
                }
            }

            // Next node
            bool was_reset;
            aim = path.Next( out was_reset );

            if ( was_reset && path.loop_mode == PathLoopMode.SNAP_BACK )
            {
                transform.position = aim;
                aim = path.Next();
                if ( is_attached ) { DetachPlayer(); }
            }
        }
    }

    /// <summary>
    /// Forces the player to follow this path.
    /// Used by moving platforms, etc.
    /// </summary>
    public void AttachPlayer()
    {
        is_attached = true;
        Referencer.instance.player.MoveWithCollision( delta );
    }

    /// <summary>
    /// Makes the player stop following this path.
    /// Used to stop moving with platforms, etc.
    /// </summary>
    public void DetachPlayer()
    {
        is_attached = false;
    }
}
