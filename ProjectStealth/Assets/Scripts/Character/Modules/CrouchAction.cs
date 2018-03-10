using UnityEngine;

public class CrouchAction : MonoBehaviour
{
    /// <summary>
    /// 
    /// CharacterStats Vars:
    /// IsCrouching
    /// </summary>

    private CharacterStats charStats;
    private IInputManager inputManager;

    void Start()
    {
        charStats = GetComponent<CharacterStats>();
        inputManager = GetComponent<IInputManager>();
    }

    void Update()
    {
        if (charStats.OnTheGround && charStats.CurrentMoveState == CharEnums.MoveState.isSneaking && inputManager.VerticalAxis < 0f)
        {
            if (charStats.IsCrouching == false)
            {
                charStats.IsCrouching = true;
                charStats.CrouchingHitBox();
            }
        }
        else
        {
            if (charStats.IsCrouching)
            {
                charStats.IsCrouching = false;
                charStats.StandingHitBox();
            }
        }
    }
}
