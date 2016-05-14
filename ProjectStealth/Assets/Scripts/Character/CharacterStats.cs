using UnityEngine;
using System.Collections;

public class CharacterStats : MonoBehaviour 
{
	// vars related to character core that are referenced in other scripts
    public BoxCollider2D CharCollider;
    public Vector2 Velocity;

    public int FacingDirection = 1; //-1 / 1

	public bool OnTheGround = false;
    public bool IsJumping = false;
    public bool JumpTurned = false;

    public enum MasterState { defaultState, climbState, attackState };
    public MasterState CurrentMasterState = MasterState.defaultState;


    void Start()
    {
        CharCollider = GetComponent<BoxCollider2D>();
        Velocity = new Vector2(0.0f, 0.0f);

    }

    public void ResetJump()
    {
        IsJumping = false;
        JumpTurned = false;
    }
}
