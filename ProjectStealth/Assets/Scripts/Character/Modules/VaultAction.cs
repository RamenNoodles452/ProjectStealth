﻿using UnityEngine;

public class VaultAction : MonoBehaviour
{
    /// <summary>
    /// This module allows a character who is taking cover behind a low obstacle to vault over it
    /// Input: (While taking cover) Directional Import towards the obstacle + Interact
    /// 
    /// This module allows a character who is sprinting to automatically vault over low obstacles
    /// Input: (While sprinting) Run into a low obstacle at full speed
    /// </summary>

    #region vars
    private CharacterStats char_stats;
    private IInputManager input_manager;
    private GenericMovementLib mov_lib;

    private bool is_vaulting = false;
    #endregion

    // Early Initialization
    void Awake()
    {
        char_stats    = GetComponent<CharacterStats>();
        input_manager = GetComponent<IInputManager>();
        mov_lib       = GetComponent<GenericMovementLib>();
    }

    // Update is called every frame.
    void Update()
    {
        if ( char_stats.touched_vault_obstacle != null && (char_stats.is_taking_cover && input_manager.InteractInputInst || char_stats.IsRunning) )
        {
            if ( char_stats.current_move_state == CharEnums.MoveState.IsRunning )
            {
                char_stats.current_move_state = CharEnums.MoveState.IsSneaking;
            }
            char_stats.current_master_state = CharEnums.MasterState.VaultState;
            input_manager.InteractInputInst = false;
            input_manager.IgnoreInput = true;

            // translate body to on the ledge
            char_stats.bezier_distance = 0.0f;
            char_stats.bezier_start_position = (Vector2) char_stats.char_collider.bounds.center - char_stats.char_collider.offset;
            if ( char_stats.IsFacingRight() )
            {
                char_stats.bezier_end_position = new Vector2( char_stats.char_collider.bounds.center.x - char_stats.char_collider.offset.x +
                    char_stats.touched_vault_obstacle.bounds.size.x + char_stats.char_collider.size.x,
                    char_stats.char_collider.bounds.center.y - char_stats.char_collider.offset.y );
            }
            else
            {
                char_stats.bezier_end_position = new Vector2( char_stats.char_collider.bounds.center.x - char_stats.char_collider.offset.x -
                    char_stats.touched_vault_obstacle.bounds.size.x - char_stats.char_collider.size.x,
                    char_stats.char_collider.bounds.center.y - char_stats.char_collider.offset.y );
            }
            char_stats.bezier_curve_position = new Vector2( char_stats.touched_vault_obstacle.bounds.center.x, char_stats.touched_vault_obstacle.bounds.max.y + char_stats.CROUCHING_COLLIDER_SIZE.y * 3 );
            is_vaulting = true;
        }

        if ( is_vaulting )
        {
            Vector2 position = mov_lib.BezierCurveMovement( char_stats.bezier_distance, char_stats.bezier_start_position, char_stats.bezier_end_position, char_stats.bezier_curve_position );
            transform.position = new Vector3( position.x, position.y, transform.position.z ); // preserve Z

            if ( char_stats.bezier_distance < 1.0f )
            {
                char_stats.bezier_distance = char_stats.bezier_distance + Time.deltaTime * Time.timeScale * 3.5f; //...what? WHAT? WAT!?
            }
            else
            {
                is_vaulting = false;
                char_stats.touched_vault_obstacle = null;
                input_manager.IgnoreInput = false;
                char_stats.current_master_state = CharEnums.MasterState.DefaultState;
            }
        }
    }
}