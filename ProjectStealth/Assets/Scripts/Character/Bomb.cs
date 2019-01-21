using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Remotely detonatable bomb
/// </summary>
public class Bomb : MonoBehaviour
{
    #region vars
    public int bomb_id;
    [SerializeField]
    private GameObject explosion_prefab;
    [SerializeField]
    private GameObject noise_prefab;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if ( explosion_prefab == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing explosion prefab." );
            #endif
            Destroy( this.gameObject );
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: if stuck to enemy, move with them? Or just parent?
        // TODO: blinking lights
    }

    /// <summary>
    /// Called when the bomb is detonated by something external (was shot, etc.)
    /// </summary>
    private void Detonate()
    {
        ExplosiveCharge explosive = Referencer.instance.player.GetComponent<ExplosiveCharge>();
        explosive.BombDestroyed( bomb_id );
        Explode();
    }

    /// <summary>
    /// Called when the bomb is remotely detonated by the player;
    /// </summary>
    public void RemotelyDetonate()
    {
        Explode();
    }

    /// <summary>
    /// Creates the explosion, and destroys this object.
    /// </summary>
    private void Explode()
    {
        GameObject explosion_object = GameObject.Instantiate( explosion_prefab, transform.position, Quaternion.identity );
        Explosion explosion = explosion_object.GetComponent<Explosion>();
        if ( explosion != null )
        {
            explosion.damage = 100.0f;
            explosion.radius = 128.0f; // pixels
            explosion.is_enemy_immune  = false;
            explosion.is_player_immune = false; // You CAN blow yourself up with this, so be careful.
        }

        GameObject noise_object = GameObject.Instantiate( noise_prefab, transform.position, Quaternion.identity );
        Noise noise = noise_object.GetComponent<Noise>();
        if ( noise != null )
        {
            noise.position = noise_object.transform.position;
            noise.radius   = 256.0f; // pixels
            noise.lifetime = 1.0f;
        }

        Destroy( this.gameObject );
    }
}
