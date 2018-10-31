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
    #endregion

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        CharacterStats char_stats = Referencer.instance.player.GetComponent<CharacterStats>();
        if ( is_on )
        {
            if ( ! char_stats.is_on_ground && ! is_crumbling ) { return; }
            if ( char_stats.on_ground_collider == GetComponent<BoxCollider2D>() )
            {
                is_crumbling = true;
            }
            if ( is_crumbling )
            { 
                timer += Time.deltaTime * Time.timeScale;
                if ( timer >= fall_time )
                {
                    GetComponent<BoxCollider2D>().enabled = false;
                    GetComponent<SpriteRenderer>().enabled = false;
                    transform.Find( "Background" ).gameObject.SetActive( false );
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
                GetComponent<BoxCollider2D>().enabled = true;
                GetComponent<SpriteRenderer>().enabled = true;
                transform.Find( "Background" ).gameObject.SetActive( true );
                is_on = true;
                timer = 0.0f;
            }
        }
    }
}
