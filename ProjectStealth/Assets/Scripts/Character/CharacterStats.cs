using UnityEngine;
//using System.Collections;

public class CharacterStats : MonoBehaviour 
{
    // vars related to character core that are referenced in other scripts
    public CharEnums.MasterState CurrentMasterState = CharEnums.MasterState.defaultState;
    public CharEnums.MoveState CurrentMoveState = CharEnums.MoveState.isWalking;

    [HideInInspector]
    public BoxCollider2D CharCollider; // defaults: offset[0,-2] size [26,40]
    [HideInInspector]
    public Vector2 STANDING_COLLIDER_SIZE = new Vector2(26f, 40f);
    [HideInInspector]
    public Vector2 STANDING_COLLIDER_OFFSET = new Vector2(0f, -2f);
    [HideInInspector]
    public Vector2 CROUCHING_COLLIDER_SIZE = new Vector2(26f, 20f);
    [HideInInspector]
    public Vector2 CROUCHING_COLLIDER_OFFSET = new Vector2(0f, -12f);

    public Vector2 Velocity;
    public float WALK_SPEED = 1.0f; //used for cutscenes for PC, guards will walk when not alerted
    public float SNEAK_SPEED = 2.0f; //default speed, enemies that were walking will use this speed when on guard
    public float RUN_SPEED = 4.5f;

    [HideInInspector]
    public float CharacterAccel = 0.0f; //this changes based on if a character is mid air or not.
    public int FacingDirection = 1; // [-1,1]

	public bool OnTheGround = false;
    [HideInInspector]
    public bool IsJumping = false; //this is specifically for applying SimpleCharacterCore.JUMP_VERTICAL_SPEED to the character. is set to false once the character stops ascending
    [HideInInspector]
    public bool JumpTurned = false;
    [HideInInspector]
    public float JumpInputTime;

    // Taking cover vars
    public Collider2D IsTouchingVaultObstacle = null;
    public bool IsTakingCover = false;

    // Crouching vars
    public bool IsCrouching = false;

    // bezier curve vars for getting up ledges and jumping over cover
    [HideInInspector]
    public Vector2 BzrStartPosition;
    [HideInInspector]
    public Vector2 BzrEndPosition;
    [HideInInspector]
    public Vector2 BzrCurvePosition;
    [HideInInspector]
    public float BzrDistance;


    void Start()
    {
        CharCollider = GetComponent<BoxCollider2D>();
        CharCollider.size = STANDING_COLLIDER_SIZE;
        CharCollider.offset = STANDING_COLLIDER_OFFSET;
        Velocity = new Vector2(0.0f, 0.0f);
        JumpInputTime = 0.0f;
    }

    public void ResetJump()
    {
        IsJumping = false;
        JumpTurned = false;
    }

    public void CrouchingHitBox()
    {
        CharCollider.size = CROUCHING_COLLIDER_SIZE;
        CharCollider.offset = CROUCHING_COLLIDER_OFFSET;
    }

    public void StandingHitBox()
    {
        CharCollider.size = STANDING_COLLIDER_SIZE;
        CharCollider.offset = STANDING_COLLIDER_OFFSET;
    }
}
