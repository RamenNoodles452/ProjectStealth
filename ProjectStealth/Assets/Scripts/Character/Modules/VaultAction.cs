using UnityEngine;

public class VaultAction : MonoBehaviour
{
    /// <summary>
    /// This module allows a character who is taking cover behind a low obstacle to vault over it
    /// Input: (While taking cover) Directional Import towards the obstacle + Interact
    /// 
    /// This module allows a character who is sprinting to automatically vault over low obstacles
    /// Input: (While sprinting) Run into a low obstacle at full speed
    /// </summary>
    private CharacterStats charStats;
    private IInputManager inputManager;
    private GenericMovementLib movLib;

    private bool vaulting = false;

    void Start()
    {
        charStats = GetComponent<CharacterStats>();
        inputManager = GetComponent<IInputManager>();
        movLib = GetComponent<GenericMovementLib>();
    }

    void Update()
    {
        if (charStats.IsTakingCover && inputManager.InteractInputInst || charStats.IsTouchingVaultObstacle && charStats.CurrentMoveState == CharEnums.MoveState.isRunning)
        {
            if(charStats.CurrentMoveState == CharEnums.MoveState.isRunning)
                charStats.CurrentMoveState = CharEnums.MoveState.isSneaking;
            charStats.CurrentMasterState = CharEnums.MasterState.vaultState;
            inputManager.InteractInputInst = false;
            inputManager.InputOverride = true;

            // translate body to on the ledge
            charStats.BzrDistance = 0.0f;
            charStats.BzrStartPosition = (Vector2)charStats.CharCollider.bounds.center - charStats.CharCollider.offset;
            if (charStats.FacingDirection == 1)
            {
                charStats.BzrEndPosition = new Vector2(charStats.CharCollider.bounds.center.x - charStats.CharCollider.offset.x + 
                    charStats.IsTouchingVaultObstacle.bounds.size.x + charStats.CharCollider.size.x, 
                    charStats.CharCollider.bounds.center.y - charStats.CharCollider.offset.y);
            }
            else
            {
                charStats.BzrEndPosition = new Vector2(charStats.CharCollider.bounds.center.x - charStats.CharCollider.offset.x -
                    charStats.IsTouchingVaultObstacle.bounds.size.x - charStats.CharCollider.size.x,
                    charStats.CharCollider.bounds.center.y - charStats.CharCollider.offset.y);
            }
            charStats.BzrCurvePosition = new Vector2(charStats.IsTouchingVaultObstacle.bounds.center.x, charStats.IsTouchingVaultObstacle.bounds.max.y + charStats.CROUCHING_COLLIDER_SIZE.y * 3);
            vaulting = true;
        }
    }

    void FixedUpdate()
    {
        if (vaulting)
        {
            transform.position = movLib.BezierCurveMovement(charStats.BzrDistance, charStats.BzrStartPosition, charStats.BzrEndPosition, charStats.BzrCurvePosition);
            if (charStats.BzrDistance < 1.0f)
                charStats.BzrDistance = charStats.BzrDistance + Time.deltaTime * TimeScale.timeScale * 3.5f;
            else
            {
                vaulting = false;
                charStats.IsTouchingVaultObstacle = null;
                inputManager.InputOverride = false;
                charStats.CurrentMasterState = CharEnums.MasterState.defaultState;
            }
        }
    }
}