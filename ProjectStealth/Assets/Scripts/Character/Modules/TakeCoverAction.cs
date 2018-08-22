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

    #region vars
    private CharacterStats char_stats;
    private IInputManager input_manager;

    private float cover_timer;
    private const float COVER_TIME = 0.10f; //seconds
    #endregion

    void Start()
    {
        char_stats = GetComponent<CharacterStats>();
        input_manager = GetComponent<IInputManager>();
        cover_timer = 0.0f;
    }

    void Update()
    {
        if ( char_stats.touched_vault_obstacle != null && char_stats.IsSneaking &&
            ( ( char_stats.IsFacingLeft() && input_manager.HorizontalAxis < 0.0f ) ||
                ( char_stats.IsFacingRight() && input_manager.HorizontalAxis > 0.0f ) ) )
        {
            if ( cover_timer < COVER_TIME )
            {
                cover_timer += Time.deltaTime * Time.timeScale;
            }
            if ( cover_timer >= COVER_TIME )
            {
                if ( char_stats.is_taking_cover == false )
                {
                    char_stats.is_taking_cover = true;
                    char_stats.CrouchingHitBox();
                }
            }
        }
        else
        {
            cover_timer = 0.0f;
            if ( char_stats.is_taking_cover )
            {
                char_stats.is_taking_cover = false;
                char_stats.StandingHitBox();
            }
        }
    }
}
