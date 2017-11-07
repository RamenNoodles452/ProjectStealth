using UnityEngine;
using System.Collections;

public class CollisionType : MonoBehaviour 
{
    [SerializeField]
    private bool wall_climb = true;
    [SerializeField]
    private bool fall_through = false;
    [SerializeField]
    private bool vault_obstacle = false;
    private bool ceiling_climb = false;

    // these are env objects that are of the same level that characters can smoothly transition over to
    // or can be simply used to allow the character to walk off the object (such as obstacles)
    public bool walk_off_left = false;
    public bool walk_off_right = false;

    public bool WallClimb
    {
        get { return wall_climb; }
    }
    // This is used for down+jumping through floors
    // To jump up through passable floors CollisionMasks.UpwardsCollisionMask
    public bool Fallthrough
    {
        get { return fall_through; }
    }
    public bool VaultObstacle
    {
        get { return vault_obstacle; }
    }

    // This is to know when the player should latch onto the ceiling to crawl around on
    public bool CeilingClimb
    {
        get { return ceiling_climb; }
    }


    public bool WalkOffLeft
    {
        get { return walk_off_left; }
    }
    public bool WalkOffRight
    {
        get { return walk_off_right; }
    }
}
