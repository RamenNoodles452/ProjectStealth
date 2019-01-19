using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Class for utility functions
public class Utils
{
    /// <summary>
    /// Checks if a collider is the player's collider
    /// </summary>
    /// <param name="collider">The collider to check</param>
    /// <returns>True if the collider is attached to the player</returns>
    public static bool IsPlayersCollider( Collider2D collider )
    {
        if ( collider.gameObject.layer == LayerMask.NameToLayer( "character objects" ) )
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a collider is level geometry
    /// </summary>
    /// <param name="collider">The collider to check</param>
    /// <returns>True if the collider is level geometry</returns>
    public static bool IsGeometryCollider( Collider2D collider )
    {
        if ( collider.gameObject.layer == LayerMask.NameToLayer( "geometry" ) )
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a collider is an enemy's collider
    /// </summary>
    /// <param name="collider">The collider to check</param>
    /// <returns>True if the collider belongs to an enemy</returns>
    public static bool IsEnemyCollider( Collider2D collider )
    {
        if ( collider.gameObject.layer == LayerMask.NameToLayer( "enemy" ) )
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the collision type data out of a tile-bound object.
    /// </summary>
    /// <param name="collider">The collider of the object representing the tile</param>
    /// <returns></returns>
    public static CustomTileData GetCustomTileData( Collider2D collider )
    {
        if ( collider == null ) { return null; }

        #region Non-TileMap
        // Tile-bound object
        CustomTileData custom_data = collider.gameObject.GetComponent<CustomTileData>();
        if ( custom_data != null )
        {
            return custom_data;
        }
        #endregion

        // We don't need this anymore, since we basically just use tilemap as a quick instantiator for gameobjects, b/c that is simpler and works with fewer edge cases.
        // Preserved in case we need to re-use the approach.
        /// <param name="hit_point">Only used when checking the collision type of tilemap tiles. 
        ///     This should be a point in worldspace within the tile to retrieve the data from.
        ///     Typically should be obtained by taking a raycast hit point (will be outside the "hit" tile), and going 1-16 pixels further in the direction of the ray.
        /// </param>
        /*
        // Performance: Luckily, we only need this method ~ once / frame.
        #region TileMap
        // Tilemap tile
        Tilemap tile_map = collider.gameObject.GetComponent<Tilemap>();
        if ( tile_map != null )
        {
            // need to get tile data out of the collider
            Grid layout_grid = tile_map.layoutGrid;
            if ( layout_grid == null ) { return null; }

            Vector3Int grid_position = layout_grid.WorldToCell( new Vector3( hit_point.x, hit_point.y, 0.0f ) );
            Debug.Log( grid_position );
            if ( ! tile_map.HasTile( grid_position ) ) { return null; }

            TileBase tile = tile_map.GetTile( grid_position );
            if ( tile == null ) { return null; }

            TileData tile_data = new TileData();
            tile.GetTileData( grid_position, null, ref tile_data );
            GameObject obj = tile_data.gameObject;
            if ( obj == null ) { return null; }

            custom_data = obj.GetComponent<CustomTileData>();
            if ( custom_data == null ) { return null; }

            return custom_data;
        }
        #endregion
        */

        return null;
    }

    /// <summary>
    /// Determines if a specified rectangular area contains any blocking stuff.
    /// </summary>
    /// <param name="x">The x coordiate of the left edge of the rectangular area to check.</param>
    /// <param name="y">The y coordinate of the top edge of the rectangular area to check.</param>
    /// <param name="width">The width of the rectangular area to check.</param>
    /// <param name="height">The height of the rectangular area to check.</param>
    /// <returns>True if the specified area contains any level geometry or objects (excluding enemies) that are considered blocking.</returns>
    public static bool AreaContainsBlockingGeometry( float x, float y, float width, float height )
    {
        Collider2D[] colliders_within_area = Physics2D.OverlapAreaAll( new Vector2( x, y ), new Vector2( x + width, y - height ), CollisionMasks.static_mask );
        foreach ( Collider2D collider in colliders_within_area )
        {
            // Check if the collider inside the region is blocking.
            CustomTileData tile_data = Utils.GetCustomTileData( collider );
            if ( tile_data == null ) { return true; } // not an excepted tile, guess this is an ordinary blocking object.
            // TODO: may want additional checks here ^

            CollisionType collision_type = tile_data.collision_type;
            if ( !( collision_type.CanFallthrough || !collision_type.IsBlocking ) ) { return true; } // not an excepted tile type, this is a blocking tile.
        }
        return false;
    }
}
