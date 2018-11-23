using UnityEngine;
using System.Collections;

//Stores cached collision masks.
public static class CollisionMasks
{
    //collision layers
    // "normal" level geometry
    public static int geo_mask = LayerMask.GetMask("geometry");

    // "special" level geometry
    public static int jump_through_mask = LayerMask.GetMask("jumpthrough objects");

    // enemies + destructible / mobile / interactables
    // may want to fork into enemies and objects later
    public static int object_mask = LayerMask.GetMask("collision objects");

    // player character
    public static int character_mask = LayerMask.GetMask("character objects");

    public static int all_collision_mask = geo_mask | object_mask | character_mask | jump_through_mask;
    public static int upwards_collision_mask = geo_mask | object_mask | character_mask;
    public static int wall_grab_mask = geo_mask;

    // light
    // ASSUMES: all light-blocking geometry is on these layers. Should log an error if you try another layer.
    public static int light_occlusion_mask = geo_mask | object_mask;
}

