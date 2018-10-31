using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
