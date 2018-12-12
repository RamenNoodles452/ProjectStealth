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

    #if UNITY_EDITOR
    // Editor-only error catch, omitted auto-fix for build version to keep this as lightweight and performant as possible.
    private void Awake()
    {
        if ( GetComponent<Tilemap>() != null )
        {
            Debug.LogError( "A CustomTileData component was added to an object with a tilemap. Remove the CustomTileData component." );
        }
    }
    #endif
}
