using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Grappling hookshot
public class GrapplingHook : MonoBehaviour
{
    #region vars
    private const float MAX_RANGE      = 300.0f; // pixels
    private const float LAUNCH_SPEED   = 960.0f; // pixels per second
    private const float RETRACT_SPEED  = 480.0f; // pixels per second

    private float distance_to_target   = 0.0f;
    private float distance_from_target = 0.0f;
    private bool  is_on                = false;
    private bool  is_retracting        = false;
    private float angle;
    private Vector2 target;
    //private Player player_script;
    private PlayerStats player_stats;
    private CharacterStats char_stats;
    private GameObject hook_instance;
    private GameObject chain_instance;

    [SerializeField]
    private GameObject hook_prefab;
    [SerializeField]
    private GameObject chain_prefab;
    [SerializeField]
    private GameObject noise_prefab;
    [SerializeField]
    private GameObject spark_prefab;
    #endregion

    // Use this for initialization
    void Start ()
    {
        //player_script = GetComponent<Player>();
        player_stats  = GetComponent<PlayerStats>();
        char_stats    = GetComponent<CharacterStats>();
    }
    
    // Update is called once per frame
    void Update ()
    {
        ParseInput();
        if ( is_on ) { Function(); }
    }

    void ParseInput()
    {
        if ( Input.GetKeyDown( KeyCode.Y ) && char_stats.current_master_state == CharEnums.MasterState.DefaultState )
        {
            Vector3 aim_position_3D = Camera.main.ScreenToWorldPoint( Input.mousePosition );
            Vector2 aim_position = new Vector2( aim_position_3D.x, aim_position_3D.y );
            Fire( aim_position );
        }
    }
    
    // Launch the hookshot!
    void Fire( Vector2 target )
    {
        bool hit_good_surface = false;
        bool hit_bad_surface = false;
        bool hit_enemy = false;

        // raycast, check if object is hookshottable (pass through pass through platforms)
        Vector3 character_position_3D = char_stats.char_collider.bounds.center;
        Vector2 character_position = new Vector2( character_position_3D.x, character_position_3D.y );
        Vector2 direction = target - character_position;

        // Use "upward" collision mask b/c hookshot is allowed to go through fallthrough platforms.
        RaycastHit2D hit = Physics2D.Raycast( character_position, direction, MAX_RANGE, CollisionMasks.upwards_collision_mask );
        if ( hit.collider == null )         { return; } // TODO: hit nothing: misfire / animate failure / do nothing?

        // TODO: hmm.... So if an enemy bullet gets in the way while aiming, hookshotting fails? Fishy. Might need to adjust masking?
        // TODO: check for enemies.
        CollisionType collision_type = hit.collider.GetComponent<CollisionType>();
        if ( collision_type == null )       { return; }
        if ( collision_type.CanHookshotTo ) { hit_good_surface = true; }
        else { hit_bad_surface = true; }

        distance_to_target   = hit.distance;
        distance_from_target = 0.0f;
        // TODO: consider snapping to a predefined position on a predefined designed hookshottable object. Makes edge case handling easier.
        // The alternative best way for static geometry would be to raycast from the 4 corners of the player hitbox, and if the center ray works,
        // fire the hookshot, attach, and retract it. Then STOP when you reach the distance where a corner ray hit an obstacle.
        // won't work vs. moving platforms well, so might need to do collision checks every frame (or ignore them?).
        BoxCollider2D box = hit.collider.GetComponent<BoxCollider2D>();
        if ( box != null )
        {
            target = box.bounds.center;
            distance_to_target = Vector2.Distance( character_position, box.bounds.center );
        }

        if      ( hit_good_surface ) { StartGrapple( target ); }
        else if ( hit_enemy )        { GetOverHere(); }
        else if ( hit_bad_surface )  { NoisilyBounceOffWall(); }
    }

    private void StartGrapple( Vector2 target )
    {
        char_stats.current_master_state = CharEnums.MasterState.RappelState; // turn off default move behaviour.
        this.target = target;
        is_on = true;
        is_retracting = false;

        Vector3 character_position_3D = char_stats.char_collider.bounds.center;
        Vector2 character_position = new Vector2( character_position_3D.x, character_position_3D.y );
        angle = Mathf.Atan2( target.y - character_position.y, target.x - character_position.x );
        Quaternion quaternion = Quaternion.Euler( 0.0f, 0.0f, angle * Mathf.Rad2Deg );
        distance_to_target = Vector2.Distance(target, character_position); 
        distance_from_target = 0.0f;

        hook_instance  = GameObject.Instantiate( hook_prefab,  character_position, quaternion );
        chain_instance = GameObject.Instantiate( chain_prefab, character_position, quaternion );
    }

    /// <summary>
    /// Fires the hookshot, and FAILS.
    /// Can be used to draw attention, though.
    /// </summary>
    private void NoisilyBounceOffWall()
    {
        // make a dink noise and spark
    }

    /// <summary>
    /// Pulls the enemy to you... unless it's heavy. Or ungrappleable.
    /// </summary>
    private void GetOverHere()
    {
        // Is enemy hookshottable?
    }

    private void Function()
    {
        float scale;

        if ( ! is_retracting )
        {
            scale = LAUNCH_SPEED * Time.deltaTime * Time.timeScale;
            distance_to_target -= scale;
            if ( distance_to_target <= 0.0f )
            {
                scale += distance_to_target; // don't overshoot
                is_retracting = true;
            }
            distance_from_target += scale;

            hook_instance.transform.position += scale * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
            chain_instance.transform.position += scale * 0.5f * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
            chain_instance.GetComponent<SpriteRenderer>().size = new Vector2( chain_instance.GetComponent<SpriteRenderer>().size.x + scale * 1.0f, 7.0f );
        }
        else
        {
            scale = RETRACT_SPEED * Time.deltaTime * Time.timeScale;
            distance_from_target -= scale;
            if ( distance_from_target <= 0.0f )
            {
                gameObject.transform.position = new Vector3( target.x, target.y, gameObject.transform.position.z );
                is_on = false;
                is_retracting = false;
                char_stats.current_master_state = CharEnums.MasterState.DefaultState;
                GameObject.Destroy( hook_instance  );
                GameObject.Destroy( chain_instance );
            }
            else
            {
                gameObject.transform.position += scale * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
                chain_instance.transform.position += scale * 0.5f * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
                chain_instance.GetComponent<SpriteRenderer>().size = new Vector2( chain_instance.GetComponent<SpriteRenderer>().size.x - scale * 1.0f, 7.0f );
            }
        }
    }

    public void ResetState()
    {
        is_on = false;
        is_retracting = false;
        if ( hook_instance != null )  { GameObject.Destroy( hook_instance ); }
        if ( chain_instance != null ) { GameObject.Destroy( chain_instance ); }
        char_stats.current_master_state = CharEnums.MasterState.DefaultState;
    }
}
