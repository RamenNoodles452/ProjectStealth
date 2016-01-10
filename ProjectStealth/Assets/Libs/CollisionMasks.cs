using UnityEngine;
using System.Collections;

public static class CollisionMasks
{
    //collision layers
    private static int geoLayer = 8;
    public static int GeoMask = 1 << geoLayer;

    private static int objectLayer = 9;
    public static int ObjectMask = 1 << objectLayer;

    private static int characterLayer = 10;
    public static int CharacterMask = 1 << characterLayer;

    private static int jumpthroughLayer = 11;
    public static int JumpthroughMask = 1 << jumpthroughLayer;

    public static int AllCollisionMask = GeoMask | ObjectMask | CharacterMask | JumpthroughMask;
    public static int UpwardsCollisionMask = GeoMask | ObjectMask | CharacterMask;
    public static int WallGrabMask = GeoMask;
}

