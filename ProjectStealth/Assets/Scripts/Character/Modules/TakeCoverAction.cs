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
    /// IsTakingCOver
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
                    UpdateCollisions();
                }
            }
        }
        else
        {
            coverTimer = 0.0f;
            if (charStats.IsTakingCover)
            {
                charStats.IsTakingCover = false;
                UpdateCollisions();
            }
        }
    }

    /// <summary>
    /// When taking cover, the collider will be cut in half to reflect the crouching position
    /// </summary>
    void UpdateCollisions()
    {
        if (charStats.IsTakingCover)
        {
            charStats.CharCollider.size = charStats.CROUCHING_COLLIDER_SIZE;
            charStats.CharCollider.offset = charStats.CROUCHING_COLLIDER_OFFSET;
        }
        else
        {
            charStats.CharCollider.size = charStats.STANDING_COLLIDER_SIZE;
            charStats.CharCollider.offset = charStats.STANDING_COLLIDER_OFFSET;
        }

    }
}
