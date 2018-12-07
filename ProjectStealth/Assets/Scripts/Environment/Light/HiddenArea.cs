using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Conceals an area behind an opaque sprite, until the player enters the area.
public class HiddenArea : MonoBehaviour
{
    #region vars
    private bool hidden = true;
    private SpriteRenderer sprite_renderer;
    #endregion

    // Use this for early initialization
    private void Awake()
    {
        sprite_renderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start ()
    {
        if ( ! TriggerBuildValidator.Validate( this.gameObject ) ) { Destroy( this ); }
    }
    
    // Update is called once per frame
    void Update ()
    {
        
    }

    /// <summary>
    /// Called when a collider enters this object's collider
    /// </summary>
    /// <param name="collision">The collider of the object that entered</param>
    private void OnTriggerEnter2D( Collider2D collision )
    {
        if ( Utils.IsPlayersCollider( collision ) )
        {
            hidden = false;
            Color color = sprite_renderer.color;
            color.a = 0.0f;
            sprite_renderer.color = color;
        }
    }

    /// <summary>
    /// Called when a collider exits this object's collider
    /// </summary>
    /// <param name="collision">The collider of the object that exited</param>
    private void OnTriggerExit2D( Collider2D collision )
    {
        if ( Utils.IsPlayersCollider( collision ) )
        {
            hidden = true;
            Color color = sprite_renderer.color;
            color.a = 1.0f;
            sprite_renderer.color = color;
        }
    }
}
