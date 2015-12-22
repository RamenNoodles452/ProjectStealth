using UnityEngine;
using System.Collections;

/// <summary>
/// This version is the core functionality for characters that are too complex to be represented by a box collider
/// </summary>
public class ComplexCharacterCore : MonoBehaviour
{
    protected int previousFacingDirection = 1;
    public int FacingDirection = 1;
    public Animator Anim;
    public IInputManager InputManager;
    public PolygonCollider2D characterCollider;

    public bool OnTheGround = false;
    static float MAX_FALL_SPEED = 10.0f;
    static float GRAVITATIONAL_FORCE = 9.0f;
    public float verticalVelocity;
    float fallingTime;

    // Use this for initialization
    public virtual void Start()
    {
        characterCollider = GetComponent<PolygonCollider2D>();
        Anim = GetComponent<Animator>();
        InputManager = GetComponent<IInputManager>();
        verticalVelocity = 0.0f;
        fallingTime = 0.0f;
    }

    public virtual void Update()
    {
        // Give all characters gravity
        Gravity();
    }

    private void Gravity()
    {
        fallingTime = fallingTime + Time.deltaTime * TimeScale.timeScale;

        verticalVelocity = Mathf.Clamp(GRAVITATIONAL_FORCE * fallingTime, -MAX_FALL_SPEED, MAX_FALL_SPEED);
        transform.Translate(Vector2.down * verticalVelocity);

        //transform.Translate(Vector2.down * MAX_FALL_SPEED * Time.deltaTime * TimeScale.timeScale);
        //ObjectTransform.position.y = ObjectTransform.position.y - MAX_FALL_SPEED;
    }

    private void Collisions()
    {

    }
}
