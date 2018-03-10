using UnityEngine;
using System.Collections;

public static class CollisionMasks
{
    //collision layers
	// "normal" level geometry
    private static int geoLayer = 8;
    public static int GeoMask = 1 << geoLayer;

	// "special" level geometry
	private static int jumpthroughLayer = 11;
	public static int JumpthroughMask = 1 << jumpthroughLayer;

	// enemies + destructible / mobile / interactables
	// may want to fork into enemies and objects later
    private static int objectLayer = 9;
    public static int ObjectMask = 1 << objectLayer;

	// player character
    private static int characterLayer = 10;
    public static int CharacterMask = 1 << characterLayer;

    public static int AllCollisionMask = GeoMask | ObjectMask | CharacterMask | JumpthroughMask;
    public static int UpwardsCollisionMask = GeoMask | ObjectMask | CharacterMask;
    public static int WallGrabMask = GeoMask;
}

