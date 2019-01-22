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
    private bool  did_fail             = false;
    private float angle;
    private Vector2 target;
    private IInputManager input_manager;
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
        input_manager = GetComponent<IInputManager>();
    }
    
    // Update is called once per frame
    void Update ()
    {
        ParseInput();
        if ( is_on ) { Function(); }
    }

    /// <summary>
    /// Parses fire key and aiming.
    /// </summary>
    void ParseInput()
    {
        if ( input_manager.GadgetInputInst && player_stats.gadget == GadgetEnum.Hookshot && char_stats.current_master_state == CharEnums.MasterState.DefaultState )
        {
            Vector3 aim_position_3D = Camera.main.ScreenToWorldPoint( Input.mousePosition );
            Vector2 aim_position = new Vector2( aim_position_3D.x, aim_position_3D.y );
            Fire( aim_position );
        }
    }
    
    /// <summary>
    /// Launches the hookshot!
    /// </summary>
    /// <param name="target">The position to fire the hookshot at.</param>
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
        RaycastHit2D hit = Physics2D.Raycast( character_position, direction, MAX_RANGE, CollisionMasks.hookshot_mask );
        if ( hit.collider == null )         { return; } // TODO: hit nothing: misfire / animate failure / do nothing?

        // TODO: hmm.... So if an enemy bullet gets in the way while aiming, hookshotting fails? Fishy. Might need to adjust masking?
        // TODO: check for enemies.
        CustomTileData tile_data = Utils.GetCustomTileData( hit.collider );
        CollisionType collision_type = null;
        if ( tile_data != null ) { collision_type = tile_data.collision_type; }


        if ( collision_type == null )       { return; }
        if ( collision_type.CanHookshotTo ) { hit_good_surface = true; }
        else                                { hit_bad_surface  = true; }

        distance_to_target   = hit.distance;
        distance_from_target = 0.0f;
        if ( hit_bad_surface ) { target = distance_to_target * direction.normalized + character_position; }
        // TODO: consider snapping to a predefined position on a predefined designed hookshottable object. Makes edge case handling easier.
        // The alternative best way for static geometry would be to raycast from the 4 corners of the player hitbox, and if the center ray works,
        // fire the hookshot, attach, and retract it. Then STOP when you reach the distance where a corner ray hit an obstacle.
        // won't work vs. moving platforms well, so might need to do collision checks every frame (or ignore them?).
        if ( hit_good_surface )
        {
            BoxCollider2D box = tile_data.gameObject.GetComponent<BoxCollider2D>();
            if ( box != null )
            {
                target = box.bounds.center;
                distance_to_target = Vector2.Distance( character_position, box.bounds.center );
            }
        }

        if      ( hit_good_surface ) { StartGrapple( target ); }
        else if ( hit_enemy )        { GetOverHere(); }
        else if ( hit_bad_surface )  { NoisilyBounceOffWall( target ); }
    }

    /// <summary>
    /// Starts the process of pulling yourself to the grapple point.
    /// </summary>
    /// <param name="target"></param>
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
    private void NoisilyBounceOffWall( Vector2 target )
    {
        // makes a dink noise and sparks
        did_fail = true;
        StartGrapple( target );
    }

    /// <summary>
    /// Pulls the enemy to you... unless it's heavy. Or ungrappleable.
    /// </summary>
    private void GetOverHere()
    {
        // Is enemy hookshottable + pullable?
        bool is_hookshottable = true;
        if ( ! is_hookshottable ) { return; }
        bool is_pullable = false;
        if ( ! is_pullable ) { ComingOverThere(); return; }

        // pull the enemy!
        // complications: enemies can move, need to stun during pull, but also maybe before so you can hit them? Or defer the pull check?
        // TODO:
    }

    /// <summary>
    /// Pulls you to a heavy enemy.
    /// </summary>
    private void ComingOverThere()
    {
        // pull
        // complications: enemies can move, probably need to stun during pull, also maybe before so you can hit them? Or defer the check?
        // TODO:
        // StartGrapple( target );
    }

    /// <summary>
    /// Makes the grappling hook shot function.
    /// </summary>
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

                if ( did_fail ) // make dink noise and spark
                {
                    GameObject noise_obj = null;
                    if ( noise_prefab != null ) { noise_obj = GameObject.Instantiate( noise_prefab, hook_instance.transform.position, Quaternion.identity ); }
                    if ( noise_obj != null )
                    {
                        Noise noise = noise_obj.GetComponent<Noise>();
                        if ( noise != null )
                        {
                            noise.radius = 48.0f;
                            noise.lifetime = 0.5f;
                        }
                    }
                    GameObject.Instantiate( spark_prefab, hook_instance.transform.position, Quaternion.Euler( 0.0f, 0.0f, 180.0f + hook_instance.transform.rotation.eulerAngles.z ) );
                    AudioSource audio_source = hook_instance.GetComponent<AudioSource>();
                    if ( audio_source != null ) { audio_source.Play(); }
                }
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
                if ( ! did_fail ) { gameObject.transform.position = new Vector3( target.x, target.y, gameObject.transform.position.z ); }
                ResetState();
            }
            else
            {
                if ( ! did_fail )
                {
                    gameObject.transform.position     += scale * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
                    chain_instance.transform.position += scale * 0.5f * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
                }
                else
                {
                    hook_instance.transform.position  -= scale * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
                    chain_instance.transform.position -= scale * 0.5f * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
                }
                chain_instance.GetComponent<SpriteRenderer>().size = new Vector2( chain_instance.GetComponent<SpriteRenderer>().size.x - scale * 1.0f, 7.0f );
            }
        }
    }

    /// <summary>
    /// Forces player out of grapple mode, turns off the hookshot, and cleans up.
    /// </summary>
    public void ResetState()
    {
        is_on = false;
        is_retracting = false;
        did_fail = false;
        if ( hook_instance != null )  { GameObject.Destroy( hook_instance ); }
        if ( chain_instance != null ) { GameObject.Destroy( chain_instance ); }
        char_stats.current_master_state = CharEnums.MasterState.DefaultState;
    }
}
