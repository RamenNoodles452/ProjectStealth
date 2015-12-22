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

    public static int AllCollisionMask = GeoMask | ObjectMask | CharacterMask;
}

