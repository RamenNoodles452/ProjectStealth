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
    /// Gets the collision type data out of a tile or tile-like object.
    /// </summary>
    /// <param name="collider">The collider of the tile-like object, or the composite collider of the tilemap.</param>
    /// <param name="hit_point">Only used when checking the collision type of tilemap tiles. 
    ///     This should be a point in worldspace within the tile to retrieve the data from.
    ///     Typically should be obtained by taking a raycast hit point (will be outside the "hit" tile), and going 1-16 pixels further in the direction of the ray.
    /// </param>
    /// <returns></returns>
    public static CollisionType GetCollisionType( Collider2D collider, Vector2 hit_point )
    {
        // Performance: Luckily, we only need this method ~ once / frame.
        if ( collider == null ) { return null; }

        #region Non-TileMap
        // Tile-like object
        // NOTE: never put a CustomTileData script on a tilemap object (this is error logged).
        CustomTileData custom_data = collider.gameObject.GetComponent<CustomTileData>();
        if ( custom_data != null )
        {
            return custom_data.collision_type;
        }
        #endregion

        #region TileMap
        // Tilemap tile
        Tilemap tile_map = collider.gameObject.GetComponent<Tilemap>();
        if ( tile_map != null )
        {
            // need to get tile data out of the collider
            Grid layout_grid = tile_map.layoutGrid;
            if ( layout_grid == null ) { return null; }

            Vector3Int grid_position = layout_grid.WorldToCell( new Vector3( hit_point.x, hit_point.y, 0.0f ) );
            //Debug.Log( grid_position );
            if ( ! tile_map.HasTile( grid_position ) ) { return null; }

            TileBase tile = tile_map.GetTile( grid_position );
            if ( tile == null ) { return null; }

            TileData tile_data = new TileData();
            tile.GetTileData( grid_position, null, ref tile_data );
            GameObject obj = tile_data.gameObject;
            if ( obj == null ) { return null; }

            custom_data = obj.GetComponent<CustomTileData>();
            if ( custom_data == null ) { return null; }

            return custom_data.collision_type;
        }
        #endregion

        return null;
    }
}
