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
    /// IsTakingCover
    /// </summary>

    private CharacterStats charStats;
    private IInputManager inputManager;

    private float coverTimer;
    private const float COVER_TIME = 0.10f;

    void Start ()
    {
        charStats = GetComponent<CharacterStats>();
        inputManager = GetComponent<IInputManager>();
        coverTimer = 0.0f;
    }
	
	void Update ()
    {
        if (charStats.IsTouchingVaultObstacle && charStats.CurrentMoveState == CharEnums.MoveState.isSneaking &&
           ((charStats.FacingDirection == -1 && inputManager.HorizontalAxis < 0f) || (charStats.FacingDirection == 1 && inputManager.HorizontalAxis > 0f)))
        {
            if (coverTimer < COVER_TIME)
            {
                coverTimer += Time.deltaTime * TimeScale.timeScale;
            }
            if (coverTimer >= COVER_TIME)
            {
                if (charStats.IsTakingCover == false)
                {
                    charStats.IsTakingCover = true;
                    charStats.CrouchingHitBox();
                }
            }
        }
        else
        {
            coverTimer = 0.0f;
            if (charStats.IsTakingCover)
            {
                charStats.IsTakingCover = false;
                charStats.StandingHitBox();
            }
        }
    }
}
