using UnityEngine;
using System.Collections;

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

    [SerializeField]
    private float coverTimer;
    private const float COVER_TIME = 0.05f;

    // Use this for initialization
    void Start ()
    {
        charStats = GetComponent<CharacterStats>();
        inputManager = GetComponent<IInputManager>();

        coverTimer = 0.0f;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (charStats.IsTouchingVaultObstacle &&
           ((charStats.FacingDirection == -1 && inputManager.HorizontalAxis < 0f) || (charStats.FacingDirection == 1 && inputManager.HorizontalAxis > 0f)))
        {
            if (coverTimer < COVER_TIME)
            {
                coverTimer += Time.deltaTime * TimeScale.timeScale;
            }
            if (coverTimer >= COVER_TIME)
            {
                if (charStats.IsTakingCover == false)
                    charStats.IsTakingCover = true;
            }
        }
        else
        {
            coverTimer = 0.0f;
            if (charStats.IsTakingCover)
                charStats.IsTakingCover = false;
        }
        UpdateCollisions();
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
