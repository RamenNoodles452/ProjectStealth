using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Continuously copies the sprite from another SpriteRenderer into this object's SpriteMask.
/// Used to synchronize a sprite mask with an animated SpriteRenderer, for example.
/// </summary>
public class CopySprite : MonoBehaviour
{
    #region vars
    public SpriteRenderer sprite_renderer_to_copy;
    private SpriteMask my_sprite_mask;
    #endregion

    // Use this for initialization
    void Awake ()
    {
        my_sprite_mask = GetComponent<SpriteMask>();
        if ( my_sprite_mask == null )
        {
            Destroy( this );
            return;
        }

        if ( sprite_renderer_to_copy == null )
        {
            Destroy( this );
            return;
        }

        // Circular reference?
        if ( sprite_renderer_to_copy == my_sprite_mask )
        {
            Destroy( this );
        }
    }
    
    // Update is called once per frame
    void Update ()
    {
        my_sprite_mask.sprite = sprite_renderer_to_copy.sprite;
        transform.localScale = new Vector3( sprite_renderer_to_copy.flipX ? -1.0f : 1.0f, sprite_renderer_to_copy.flipY ? -1.0f : 1.0f, 1.0f );
    }
}
