using UnityEngine;
using System.Collections;

//Stores cached collision masks.
//TODO: update this to be more readable by using unity's APIs. Potentially remove entirely, and cache locally.
//There's a built in API for this, y'know: LayerMask.GetMask + LayerMask.LayerToName
public static class CollisionMasks
{
    //collision layers
	// "normal" level geometry
    private static int geo_layer = 8;
    public static int geo_mask = 1 << geo_layer;

	// "special" level geometry
	private static int jump_through_layer = 11;
	public static int jump_through_mask = 1 << jump_through_layer; 

	// enemies + destructible / mobile / interactables
	// may want to fork into enemies and objects later
    private static int object_layer = 9;
    public static int object_mask = 1 << object_layer;

	// player character
    private static int character_layer = 10;
    public static int character_mask = 1 << character_layer;

    public static int AllCollisionMask = geo_mask | object_mask | character_mask | jump_through_mask;
    public static int UpwardsCollisionMask = geo_mask | object_mask | character_mask;
    public static int WallGrabMask = geo_mask;
}

