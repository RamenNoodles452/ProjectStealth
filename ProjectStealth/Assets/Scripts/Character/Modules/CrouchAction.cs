using UnityEngine;

public class CrouchAction : MonoBehaviour
{
    /// <summary>
    /// 
    /// CharacterStats Vars:
    /// is_crouching
    /// </summary>

    private CharacterStats char_stats;
    private IInputManager input_manager;

    void Start()
    {
        char_stats = GetComponent<CharacterStats>();
        input_manager = GetComponent<IInputManager>();
    }

    void Update()
    {
		if (char_stats.IsGrounded && char_stats.current_move_state == CharEnums.MoveState.IsSneaking && input_manager.VerticalAxis < 0.0f) //TODO: we really need to expose a stopped API.
        {
            if (char_stats.is_crouching == false)
            {
                char_stats.is_crouching = true;
                char_stats.CrouchingHitBox();
            }
        }
        else
        {
            if (char_stats.is_crouching)
            {
                char_stats.is_crouching = false;
                char_stats.StandingHitBox();
            }
        }
    }
}
