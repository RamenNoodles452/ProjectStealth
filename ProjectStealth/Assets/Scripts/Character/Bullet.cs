using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enemy bullet
public class Bullet : MonoBehaviour
{
    #region vars
    private float damage    = 50.0f;  // base player health is 100
    private float lifetime  = 3.0f;   // seconds //TODO: use DestroyMe instead.
    private float timer     = 0.0f;
    private float speed     = 200.0f; // pixels / second
    private float angle     = 0.0f;
    private bool  is_homing = false;
    #endregion

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if ( is_homing )
        {
            Vector3 player_position = Referencer.instance.player.transform.position;
            angle = Mathf.Atan2( player_position.y - transform.position.y, player_position.x - transform.position.x );
        }
        transform.position += new Vector3( speed * Mathf.Cos( angle ), speed * Mathf.Sin( angle ), 0.0f ) * Time.deltaTime * Time.timeScale;

        timer += Time.deltaTime * Time.timeScale;
        if ( timer > lifetime )
        {
            Destroy( this.gameObject );
        }
    }

    public float Angle
    {
        get
        {
            return angle;
        }
        set
        {
            angle = value;
        }
    }

    // Called when a collider enters this object's hitbox
    void OnTriggerEnter2D( Collider2D collider )
    {
        // So... bullets should have kinematic, not dynamic rigidbodies so that they don't "interact" with floors, walls, etc. by sliding around and doing physics-based simulations.
        // HOWEVER, only dynamic rigidbodies can have OnCollisionEnter2D stuff
        // THEREFORE, bullets must be triggers.
        // See: https://docs.Unity2d.com/Manual/CollidersOverview.html

        if ( Utils.IsPlayersCollider( collider ) )
        {
            PlayerStats player_stats = collider.gameObject.GetComponent<PlayerStats> ();
            if ( player_stats != null )
            {
                player_stats.Hit( damage );
            }
            Debug.Log( "bullet hit player" );
            Destroy( this.gameObject );
        }
        else if ( Utils.IsGeometryCollider( collider ) )
        {
            Debug.Log( "bullet hit wall" );
            Destroy( this.gameObject );
        }
    }
}
