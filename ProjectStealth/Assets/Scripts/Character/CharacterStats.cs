using UnityEngine;
//using System.Collections;

// Stores character movement-related state data.
public class CharacterStats : MonoBehaviour
{
    #region vars
    // vars related to character core that are referenced in other scripts
    public CharEnums.MasterState current_master_state = CharEnums.MasterState.DefaultState;
    public CharEnums.MoveState   current_move_state   = CharEnums.MoveState.IsWalking;
    public CharEnums.MoveState   previous_move_state  = CharEnums.MoveState.IsWalking;

    // Collision geometry
    public Collider2D on_ground_collider = null; // the geometry the character is standing on. null if not standing on anything

    [HideInInspector]
    public BoxCollider2D char_collider; // defaults: offset[0,-2] size [24,40]
    //[HideInInspector]
    public Vector2 STANDING_COLLIDER_SIZE = new Vector2(25.0f, 39.0f);
    [HideInInspector]
    public Vector2 STANDING_COLLIDER_OFFSET = new Vector2(0.0f, -2.0f); // the collider box needs to be offset from center
    //[HideInInspector]
    public Vector2 CROUCHING_COLLIDER_SIZE = new Vector2(25.0f, 19.0f);
    [HideInInspector]
    public Vector2 CROUCHING_COLLIDER_OFFSET = new Vector2(0.0f, -12.0f); // the collider box needs to be offset from center

    public Vector2 velocity;
    [HideInInspector]
    public Vector2 acceleration; //this changes based on if a character is mid air or not.
    public CharEnums.FacingDirection facing_direction;
    public CharEnums.FacingDirection previous_facing_direction;

    public bool is_on_ground = false; //TODO: write a setter
    [HideInInspector]
    public bool is_jumping = false; //this is specifically for applying SimpleCharacterCore.JUMP_VERTICAL_SPEED to the character. is set to false once the character stops ascending
    [HideInInspector]
    public bool jump_turned = false;
    [HideInInspector]
    public float jump_input_time;

    // Taking cover vars
    public Collider2D touched_vault_obstacle = null;
    public bool is_taking_cover = false;

    // Crouching vars
    public bool is_crouching = false;

    // bezier curve vars for jumping over cover
    [HideInInspector]
    public Vector2 bezier_start_position;
    [HideInInspector]
    public Vector2 bezier_end_position;
    [HideInInspector]
    public Vector2 bezier_curve_position;
    [HideInInspector]
    public float bezier_distance;
    #endregion

    void Start()
    {
        char_collider = GetComponent<BoxCollider2D>();
        char_collider.size = STANDING_COLLIDER_SIZE;
        char_collider.offset = STANDING_COLLIDER_OFFSET;
        velocity = new Vector2( 0.0f, 0.0f );
        acceleration = new Vector2( 0.0f, 0.0f );
        jump_input_time = 0.0f;
    }

    public void ResetJump()
    {
        is_jumping = false;
        jump_turned = false;
    }

    /// <summary>
    /// Resizes the character's hit box to be smaller when crouching.
    /// </summary>
    public void CrouchingHitBox()
    {
        char_collider.size = CROUCHING_COLLIDER_SIZE;
        char_collider.offset = CROUCHING_COLLIDER_OFFSET;
    }

    /// <summary>
    /// Restores the character's hit box to normal size.
    /// </summary>
    public void StandingHitBox()
    {
        char_collider.size = STANDING_COLLIDER_SIZE;
        char_collider.offset = STANDING_COLLIDER_OFFSET;
    }

    /// <summary>
    /// Gets whether this character is in midair.
    /// If it is not, then it is Grounded.
    /// </summary>
    /// <value><c>true</c> if this character is in midair; otherwise, <c>false</c>.</value>
    public bool IsInMidair
    {
        get
        {
            return !is_on_ground;
        }
    }

    /// <summary>
    /// Gets whether this character is grounded.
    /// If it is not, then it is in Midair
    /// </summary>
    /// <value><c>true</c> if this character is grounded; otherwise, <c>false</c>.</value>
    public bool IsGrounded
    {
        get
        {
            return is_on_ground;
        }
    }

    /// <summary>
    /// Gets whether this character is running.
    /// </summary>
    /// <value><c>true</c> if this character is running; otherwise, <c>false</c>.</value>
    public bool IsRunning
    {
        get
        {
            return current_move_state == CharEnums.MoveState.IsRunning;
        }
    }

    /// <summary>
    /// Gets whether this character is walking.
    /// </summary>
    /// <value><c>true</c> if this character is walking; otherwise, <c>false</c>.</value>
    public bool IsWalking
    {
        get
        {
            return current_move_state == CharEnums.MoveState.IsWalking;
        }
    }

    /// <summary>
    /// Gets whether this character is sneaking.
    /// </summary>
    /// <value><c>true</c> if this character is sneaking; otherwise, <c>false</c>.</value>
    public bool IsSneaking
    {
        get
        {
            return current_move_state == CharEnums.MoveState.IsSneaking;
        }
    }

    /// <summary>
    /// Reverses the character's facing.
    /// </summary>
    public void AboutFace()
    {
        if ( facing_direction == CharEnums.FacingDirection.Right )
        {
            facing_direction = CharEnums.FacingDirection.Left;
        }
        else
        {
            facing_direction = CharEnums.FacingDirection.Right;
        }
    }

    /// <summary>
    /// Gets the X component of the "facing vector".
    /// </summary>
    /// <returns>1 if the character is facing right, -1 if the character is facing left.</returns>
    public int GetFacingXComponent()
    {
        if ( facing_direction == CharEnums.FacingDirection.Right )
        {
            return 1;
        }
        return -1;
    }

    /// <summary>
    /// Returns true if the character is facing left.
    /// </summary>
    /// <returns>True, if the character is facing left.</returns>
    public bool IsFacingLeft()
    {
        return facing_direction == CharEnums.FacingDirection.Left;
    }

    /// <summary>
    /// Returns true if the character is facing right.
    /// </summary>
    /// <returns>True, if the character is facing right.</returns>
    public bool IsFacingRight()
    {
        return facing_direction == CharEnums.FacingDirection.Right;
        // Could use !IsFacingLeft. Provided for readability and completeness' sake.
    }
}
