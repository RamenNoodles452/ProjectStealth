using UnityEngine;
using System.Collections;

/// <summary>
/// This version is the core functionality for characters that are too complex to be represented by a box collider
/// </summary>
/// NOTE: This duplicates some functionality of CharacterStats, and is more similar to it atm than SimpleCharacterCore, so the name of this class is a bit fishy.
public class ComplexCharacterCore : MonoBehaviour
{
	#region vars
	protected CharEnums.FacingDirection previous_facing_direction = CharEnums.FacingDirection.Right;
	public CharEnums.FacingDirection facing_direction = CharEnums.FacingDirection.Right;
    public Animator animator;
    public IInputManager input_manager;
    public PolygonCollider2D character_collider;

    public bool is_on_ground = false;
    static float MAX_FALL_SPEED = 600.0f;             // pixels per second
    static float GRAVITATIONAL_ACCELERATION = 540.0f; // pixels per second per second
    public float vertical_velocity;
    float falling_time;
	#endregion

    // Use this for initialization
    public virtual void Start()
    {
        character_collider = GetComponent<PolygonCollider2D>();
        animator = GetComponent<Animator>();
        input_manager = GetComponent<IInputManager>();
        vertical_velocity = 0.0f;
        falling_time = 0.0f;
    }

    public virtual void Update()
    {
        // Give all characters gravity
        Gravity();
    }

    private void Gravity()
    {
        falling_time = falling_time + Time.deltaTime * Time.timeScale;

        vertical_velocity = Mathf.Clamp(GRAVITATIONAL_ACCELERATION * falling_time, -MAX_FALL_SPEED, MAX_FALL_SPEED);
        transform.Translate(Vector2.down * vertical_velocity);

        //transform.Translate(Vector2.down * MAX_FALL_SPEED * Time.deltaTime * TimeScale.timeScale);
        //ObjectTransform.position.y = ObjectTransform.position.y - MAX_FALL_SPEED;
    }

    private void Collisions()
    {

    }
}
