using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the collision behaviour of a tile.
/// </summary>
[System.Serializable]
public class CollisionType
{
    #region vars
    [SerializeField]
    private bool is_blocking = true; // Layer-restricted (nonblocking layer only).

    [SerializeField]
    private bool can_climb_side = false;
    [SerializeField]
    private bool can_climb_bottom = false;
    [SerializeField]
    private bool can_fall_through = false; // Layer-restricted (jumpthrough objects layer only).
    [SerializeField]
    private bool can_vault_over = false;

    [SerializeField]
    private bool can_hookshot_to = false;

    [SerializeField]
    private bool can_walk_off_left_edge = false;
    [SerializeField]
    private bool can_walk_off_right_edge = false;
    #endregion

    /// <summary>
    /// Can the player down + jump to pass through the floor?
    /// </summary>
    public bool CanFallthrough
    {
        /// To jump up through passable floors use CollisionMasks.UpwardsCollisionMask
        get { return can_fall_through; }
        set { can_fall_through = value; }
    }

    /// <summary>
    /// Can you vault over the object?
    /// </summary>
    public bool CanVaultOver
    {
        get { return can_vault_over; }
        set { can_vault_over = value; }
    }

    /// <summary>
    /// Can the sides of this object be climbed?
    /// </summary>
    public bool IsWallClimbable
    {
        get { return can_climb_side; }
        set { can_climb_side = value; }
    }

    /// <summary>
    /// Can the player latch onto the bottom of the object (ceiling) and crawl around?
    /// </summary>
    public bool IsCeilingClimbable
    {
        get { return can_climb_bottom; }
        set { can_climb_bottom = value; }
    }

    /// <summary>
    /// Can the player walk off the object's left edge?
    /// (Also used to cross between adjoining objects)
    /// </summary>
    public bool CanWalkOffLeftEdge
    {
        get { return can_walk_off_left_edge; }
        set { can_walk_off_left_edge = value; }
    }

    /// <summary>
    /// Can the player walk off the object's right edge?
    /// (Also used to cross between adjoining objects)
    /// </summary>
    public bool CanWalkOffRightEdge
    {
        get { return can_walk_off_right_edge; }
        set { can_walk_off_right_edge = value; }
    }

    /// <summary>
    /// Can the player pull themselves to this object with the hookshot?
    /// </summary>
    public bool CanHookshotTo
    {
        get { return can_hookshot_to; }
        set { can_hookshot_to = value; }
    }

    /// <summary>
    ///  Is this blocking? 
    ///  (use no for "background" objects and unset for destructible objects)
    ///  (Remember to move non-blocking objects to the nonblocking layer)
    /// </summary>
    public bool IsBlocking
    {
        get { return is_blocking; }
        set { is_blocking = value; }
    }
}
