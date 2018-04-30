using UnityEngine;

public class TakeCoverAction : MonoBehaviour
{
    /// <summary>
    /// This module allows a character to take cover behind low obstacles
    /// This will reduce the height of the player character and make them pressed right up against the obstacle
    /// This is used to avoid line of sight with enemies
    /// If an obstacle has the variable "IsCover" in the CollisionType script, you can vault over it
    /// 
    /// CharacterStats Vars:
    /// is_taking_cover
    /// </summary>

    private CharacterStats char_stats;
    private IInputManager input_manager;

    private float cover_timer;
    private const float COVER_TIME = 0.10f;

    void Start ()
    {
        char_stats = GetComponent<CharacterStats>();
        input_manager = GetComponent<IInputManager>();
        cover_timer = 0.0f;
    }
	
	void Update ()
    {
		if (char_stats.is_touching_vault_obstacle && char_stats.current_move_state == CharEnums.MoveState.IsSneaking &&
			((char_stats.facing_direction == CharEnums.FacingDirection.Left && input_manager.HorizontalAxis < 0.0f) || 
				(char_stats.facing_direction == CharEnums.FacingDirection.Right && input_manager.HorizontalAxis > 0.0f))) //TODO: expose left/right API for this
        {
            if (cover_timer < COVER_TIME)
            {
                cover_timer += Time.deltaTime * TimeScale.timeScale;
            }
            if (cover_timer >= COVER_TIME)
            {
                if (char_stats.is_taking_cover == false)
                {
                    char_stats.is_taking_cover = true;
                    char_stats.CrouchingHitBox();
                }
            }
        }
        else
        {
            cover_timer = 0.0f;
            if (char_stats.is_taking_cover)
            {
                char_stats.is_taking_cover = false;
                char_stats.StandingHitBox();
            }
        }
    }
}
