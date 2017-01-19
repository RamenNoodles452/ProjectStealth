using UnityEngine;
//using System.Collections;

public class CharacterStats : MonoBehaviour 
{
	// vars related to character core that are referenced in other scripts
    [HideInInspector]
    public BoxCollider2D CharCollider;
    public Vector2 Velocity;

    [HideInInspector]
    public float CharacterAccel = 0.0f; //this changes based on if a character is mid air or not.

    public int FacingDirection = 1; //-1 / 1

	public bool OnTheGround = false;
    //[HideInInspector]
    public bool IsJumping = false;
    [HideInInspector]
    public bool JumpTurned = false;
    [HideInInspector]
    public float JumpInputTime;

    public CharEnums.MasterState CurrentMasterState = CharEnums.MasterState.defaultState;
    public CharEnums.MoveState currentMoveState = CharEnums.MoveState.isWalking;

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
        Velocity = new Vector2(0.0f, 0.0f);
        JumpInputTime = 0.0f;
    }

    public void ResetJump()
    {
        IsJumping = false;
        JumpTurned = false;
    }
}
