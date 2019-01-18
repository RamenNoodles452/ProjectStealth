using UnityEngine;
using System.Collections;

//Stores cached collision masks.
public static class CollisionMasks
{
    #region layers
    //collision layers
    public static int default_mask = LayerMask.GetMask( "Default" ); // ignored by all but lighting.

    // "normal" level geometry
    public static int geo_mask = LayerMask.GetMask( "geometry" );

    // "special" level geometry
    public static int jump_through_mask = LayerMask.GetMask( "jumpthrough objects" );

    // non-blocking geometry
    public static int non_blocking_mask = LayerMask.GetMask( "nonblocking" );

    // destructible / mobile / interactables
    public static int object_mask = LayerMask.GetMask( "collision objects" );

    // enemy
    public static int enemy_mask = LayerMask.GetMask( "enemy" );

    // player character
    public static int character_mask = LayerMask.GetMask( "character objects" );
    #endregion

    public static int standard_collision_mask = geo_mask | object_mask | jump_through_mask | character_mask | enemy_mask;
    public static int upwards_collision_mask = geo_mask | object_mask | character_mask | enemy_mask;
    public static int wall_grab_mask = geo_mask;
    public static int ledge_grab_mask = geo_mask | jump_through_mask;
    public static int static_mask = geo_mask | jump_through_mask | object_mask;

    // light
    // ASSUMES: all light-blocking geometry is on these layers. Should log an error if you try another layer.
    public static int light_occlusion_mask = geo_mask | object_mask | non_blocking_mask | default_mask;

    // shooting
    public static int player_shooting_mask = geo_mask | object_mask | enemy_mask;
    public static int enemy_shooting_mask = geo_mask | object_mask | character_mask;
    public static int hookshot_mask = geo_mask | object_mask | non_blocking_mask;
}

