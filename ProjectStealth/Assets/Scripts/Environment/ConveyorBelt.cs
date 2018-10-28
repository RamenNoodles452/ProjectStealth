using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Conveyor Belt script
// - Gabriel Violette
public class ConveyorBelt : MonoBehaviour
{
    #region vars
    [SerializeField]
    private float speed = 48.0f; // pixels per second. Negative to reverse.
    private bool attached;

    private CharacterStats char_stats;
    #endregion

    // Use this for initialization
    void Start()
    {
        char_stats = Referencer.instance.player.GetComponent<CharacterStats>();

        Initialize();
    }

    /// <summary>
    /// Initialization (to make the sprite and animation look correct)
    /// </summary>
    private void Initialize()
    {
        attached = false;

        // Not especially performant, but saves memory, and should only rarely be called.
        BoxCollider2D collider = this.gameObject.GetComponent<BoxCollider2D>();
        BoxCollider2D platform = this.transform.parent.GetComponent<BoxCollider2D>();
        platform.size = new Vector2( collider.bounds.max.x - collider.bounds.min.x, 14.0f );

        Transform top_belt    = this.transform.Find( "Top Belt" );
        Transform bottom_belt = this.transform.Find( "Bottom Belt");
        Transform left_end = this.transform.Find( "Left End" );
        Transform right_end = this.transform.Find( "Right End" );

        SpriteRenderer top_sprite    = top_belt.GetComponent<SpriteRenderer>();
        SpriteRenderer bottom_sprite = bottom_belt.GetComponent<SpriteRenderer>();
        top_sprite.size    = new Vector2( collider.bounds.max.x - collider.bounds.min.x - 8.0f, top_sprite.size.y );
        bottom_sprite.size = new Vector2( collider.bounds.max.x - collider.bounds.min.x - 8.0f, bottom_sprite.size.y );

        left_end.position  = new Vector3( collider.bounds.min.x + 2.0f, left_end.transform.position.y, left_end.transform.position.z );
        right_end.position = new Vector3( collider.bounds.max.x - 2.0f, right_end.transform.position.y, right_end.transform.position.z );

        Animator top_animator    =  top_belt.GetComponent<Animator>();
        Animator bottom_animator = bottom_belt.GetComponent<Animator>();
        Animator left_animator   = left_end.GetComponent<Animator>();
        Animator right_animator = right_end.GetComponent<Animator>();
        float speed_multiplier = Mathf.Abs( speed ) / 20.0f;
        top_animator.SetFloat(    "Speed", speed_multiplier );
        bottom_animator.SetFloat( "Speed", speed_multiplier );
        left_animator.SetFloat(   "Speed", speed_multiplier );
        right_animator.SetFloat(  "Speed", speed_multiplier );
        top_animator.SetBool(    "Right", speed >= 0.0f );
        bottom_animator.SetBool( "Right", speed < 0.0f ); // reverse
        right_animator.SetBool(  "Right", speed >= 0.0f );
        left_animator.SetBool(   "Right", speed < 0.0f ); // reverse

        left_end.Find(  "Wheel" ).transform.Find( "Axle" ).GetComponent<Spin>().speed = -360.0f * speed / 48.0f;
        right_end.Find( "Wheel" ).transform.Find( "Axle" ).GetComponent<Spin>().speed = -360.0f * speed / 48.0f;

    }
    
    // Update is called once per frame
    void Update ()
    {
        if ( attached )
        {
            Vector3 delta = new Vector3( speed  * Time.deltaTime * Time.timeScale, 0.0f, 0.0f );
            Referencer.instance.player.MoveWithCollision( delta );
        }
    }

    #region trigger
    // Called when a collider touches the trigger collider
    private void OnTriggerEnter2D( Collider2D other )
    {
        if ( other.gameObject.layer == LayerMask.NameToLayer( "character objects" ) )
        {
            AttachPlayer();
        }
    }

    // Called when a collider is no longer touching the trigger collider, which it was previously touching
    private void OnTriggerExit2D( Collider2D other )
    {
        if ( other.gameObject.layer == LayerMask.NameToLayer( "character objects" ) )
        {
            DetachPlayer();
        }
    }
    #endregion

    #region Utility
    /// <summary>
    /// Changes the speed of the conveyor belt.
    /// </summary>
    /// <param name="new_speed"></param>
    public void ChangeSpeed( float new_speed )
    {
        speed = new_speed;
        Initialize();
    }

    /// <summary>
    /// Register the player as attached to the conveyor belt.
    /// While attached, the player will move with the belt.
    /// </summary>
    private void AttachPlayer()
    {
        // Belts should not pull you off walls, or interrupt rappelling.
        if ( char_stats.current_master_state == CharEnums.MasterState.ClimbState ) { return; }
        if ( char_stats.current_master_state== CharEnums.MasterState.RappelState ) { return; }
        attached = true;
    }

    /// <summary>
    /// Unregister the player as attached to the conveyor belt, freeing them from moving along with it.
    /// </summary>
    private void DetachPlayer()
    {
        attached = false;
    }
    #endregion
}
