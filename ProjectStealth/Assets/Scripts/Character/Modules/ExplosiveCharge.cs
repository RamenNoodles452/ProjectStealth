using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lets the player place and detonate a sticky explosive charge.
/// </summary>
public class ExplosiveCharge : MonoBehaviour
{   //TODO: make sure that on scene change counts get re-initialized.
    #region vars
    [SerializeField]
    private GameObject bomb_prefab;

    private IInputManager input_manager;
    private PlayerStats player_stats;
    private const int deployment_capacity = 1;
    private int deployed_count = 0;
    private GameObject[] bombs;
    #endregion

    /// <summary>
    /// Use this for early initialization.
    /// </summary>
    private void Awake()
    {
        input_manager = GetComponent<UserInputManager>();
        player_stats = GetComponent<PlayerStats>();

        if ( input_manager == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing component: UserInputManager." );
            #endif
        }
        if ( player_stats == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing component: PlayerStats." );
            #endif
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        bombs = new GameObject[ deployment_capacity ];
    }

    // Update is called once per frame
    void Update()
    {
        if ( ! player_stats.acquired_explosive ) { return; }

        //if ( input_manager. )
        if ( Input.GetKeyDown( KeyCode.B ) ) // TODO:
        {
            DoBombAction();
        }
    }

    // Handles placing or detonating bombs.
    private void DoBombAction()
    {
        if ( deployed_count < deployment_capacity )
        {
            Deploy();
        }
        else
        {
            Detonate();
        }
    }

    /// <summary>
    /// Places a sticky bomb.
    /// </summary>
    private void Deploy()
    {
        if ( deployed_count >= deployment_capacity ) { return; }
        Vector3 position = transform.position;
        GameObject bomb = GameObject.Instantiate( bomb_prefab, position, Quaternion.identity );
        // find the first open slot
        for ( int i = 0; i < bombs.Length; i++ )
        {
            if ( bombs[ i ] == null )
            {
                bombs[ i ] = bomb;
                Bomb bomb_script = bomb.GetComponent<Bomb>();
                if ( bomb_script != null )
                {
                    bomb_script.bomb_id = i;
                }
                break;
            }
        }
        deployed_count++;
    }

    /// <summary>
    /// Detonates all bombs.
    /// </summary>
    private void Detonate()
    {
        for ( int i = 0; i < bombs.Length; i++ )
        {
            if ( bombs[ i ] != null )
            {
                Bomb bomb_script = bombs[ i ].GetComponent<Bomb>();
                bomb_script.RemotelyDetonate();
                // Cleanup
                bombs[ i ] = null;
                deployed_count--;
            }
        }
    }

    /// <summary>
    /// Callback to notify this script that a bomb was destroyed.
    /// </summary>
    public void BombDestroyed( int id )
    {
        if ( id > 0 && id < bombs.Length )
        {
            bombs[ id ] = null;
        }
        else
        {
            #if UNITY_EDITOR
            Debug.LogError( "A bomb with an invalid bomb ID was destroyed." );
            #endif
        }

        deployed_count--;
        if ( deployed_count < 0 )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Negative bombs cannot be placed." );
            #endif
        }
    }
}
