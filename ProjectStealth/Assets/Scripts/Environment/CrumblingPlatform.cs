using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// platform that vanishes a short time after the player touches it.
public class CrumblingPlatform : MonoBehaviour
{
    #region vars
    [SerializeField]
    private float fall_time = 0.27f; // player speed is 120 pixels / second (4.075 tiles / sec)

    private const float REGEN_DELAY = 3.0f;

    private bool  is_on = true;
    private bool  is_crumbling = false;
    private float timer = 0.0f;

    private SpriteRenderer sprite_renderer;
    private BoxCollider2D  platform_collider;
    private GameObject     background_object;
    #endregion

    // Use for early initialization
    private void Awake()
    {
        GameObject parent = transform.parent.gameObject;
        sprite_renderer = parent.GetComponent<SpriteRenderer>();
        platform_collider = parent.GetComponent<BoxCollider2D>();
        background_object = parent.transform.Find( "Background" ).gameObject;

        // Validate
        if ( ! TriggerBuildValidator.Validate( this.gameObject ) ) { Destroy( this ); }
        #if UNITY_EDITOR
        if ( sprite_renderer == null )   { Debug.LogError( "Invalid configuration for crumbling platform: missing sprite renderer." ); }
        if ( platform_collider == null ) { Debug.LogError( "Invalid configuration for crumbling platform: missing box collider 2D." ); }
        if ( background_object == null ) { Debug.LogError( "Invalid configuration for crumbling platform: missing background." ); }
        #endif
    }

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        if ( is_on )
        {
            if ( is_crumbling )
            { 
                timer += Time.deltaTime * Time.timeScale;
                if ( timer >= fall_time )
                {
                    // hide
                    platform_collider.enabled = false;
                    sprite_renderer.enabled   = false;
                    background_object.SetActive( false );
                    is_on = false;
                    is_crumbling = false;
                    timer = 0.0f;
                }
            }
        }
        else
        {
            timer += Time.deltaTime * Time.timeScale;
            if ( timer >= REGEN_DELAY )
            {
                // show
                platform_collider.enabled = true;
                sprite_renderer.enabled   = true;
                background_object.SetActive( true );
                is_on = true;
                timer = 0.0f;
            }
        }
    }

    // Called when a collider touches this trigger collider
    private void OnTriggerEnter2D( Collider2D collision )
    {
        Crumble( collision );
    }

    // Called while a collider touches this trigger collider
    private void OnTriggerStay2D( Collider2D collision )
    {
        Crumble( collision );
    }

    /// <summary>
    /// Begins the crumbling process, if the player steps on the platform.
    /// </summary>
    /// <param name="collision">The collider touching this object</param>
    private void Crumble( Collider2D collision )
    {
        if ( ! is_on ) { return; }      // hidden, can't crumble
        if ( is_crumbling ) { return; } // crumbling already

        CharacterStats char_stats = Referencer.instance.player.GetComponent<CharacterStats>();
        if ( char_stats.IsInMidair ) { return; } // player is in midair, don't start crumbling.

        if ( Utils.IsPlayersCollider( collision ) )
        {
            is_crumbling = true;
        }
    }
}
