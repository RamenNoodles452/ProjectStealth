using UnityEngine;

public class CrouchAction : MonoBehaviour
{
    /// <summary>
    /// 
    /// CharacterStats Vars:
    /// is_crouching
    /// </summary>

    #region vars
    private CharacterStats char_stats;
    private IInputManager  input_manager;
    #endregion

    void Start()
    {
        char_stats    = GetComponent<CharacterStats>();
        input_manager = GetComponent<IInputManager>();
    }

    void Update()
    {
        if ( char_stats.current_master_state != CharEnums.MasterState.DefaultState ) { return; }

        if ( char_stats.IsGrounded && char_stats.current_move_state == CharEnums.MoveState.IsSneaking && input_manager.VerticalAxis < 0.0f )
        {
            if ( char_stats.is_crouching == false )
            {
                char_stats.is_crouching = true; // animation is handled through this
                char_stats.CrouchingHitBox();
            }
        }
        else
        {
            if ( char_stats.is_crouching )
            {
                char_stats.is_crouching = false;
                char_stats.StandingHitBox();
            }
        }
    }
}
