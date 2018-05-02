using UnityEngine;
using System.Collections;

//Stores cached collision masks.
public static class CollisionMasks
{
    //collision layers
	// "normal" level geometry
	//private static int geo_layer = LayerMask.NameToLayer("geometry"); //8;
	public static int geo_mask = LayerMask.GetMask("geometry"); //1 << geo_layer;

	// "special" level geometry
	//private static int jump_through_layer = 11;
	public static int jump_through_mask = LayerMask.GetMask("jumpthrough objects"); //1 << jump_through_layer; 

	// enemies + destructible / mobile / interactables
	// may want to fork into enemies and objects later
    //private static int object_layer = 9;
	public static int object_mask = LayerMask.GetMask("collision objects"); //1 << object_layer;

	// player character
    //private static int character_layer = 10;
	public static int character_mask = LayerMask.GetMask("character objects"); //1 << character_layer;

    public static int all_collision_mask = geo_mask | object_mask | character_mask | jump_through_mask;
    public static int upwards_collision_mask = geo_mask | object_mask | character_mask;
    public static int wall_grab_mask = geo_mask;
}

