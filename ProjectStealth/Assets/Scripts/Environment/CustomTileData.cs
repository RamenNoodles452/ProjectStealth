using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

/// <summary>
/// Class to store tile-bound data.
/// Determines collision behaviour of a "tile" object. Can also be added to non-tile objects that should act like tiles.
/// Can be extended to include whatever other data we need for tiles in the future, as well.
/// </summary>
public class CustomTileData : MonoBehaviour
{
    #region vars
    public CollisionType collision_type;
    #endregion

    private void Awake()
    {
        #if UNITY_EDITOR
        if ( collision_type.CanFallthrough && gameObject.layer != CollisionMasks.jump_through_mask )
        {
            Debug.LogError( gameObject.name + " Invalid configuration: Object's collision type is set to fallthrough, but is not on the jumpthrough objects layer." );
        }
        if ( ! collision_type.IsBlocking && gameObject.layer != CollisionMasks.non_blocking_mask )
        {
            Debug.LogError( gameObject.name + " Invalid configuration: Object's collision type is set to non-blocking, but is not on the nonblocking layer." );
        }
        #endif
    }
}
