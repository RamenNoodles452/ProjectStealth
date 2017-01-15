using UnityEngine;
using System.Collections;

public class ClimbType : MonoBehaviour 
{
    [SerializeField]
    private bool wall_climb = true;
    [SerializeField]
    private bool ceiling_climb = false;
    [SerializeField]
    private bool ceiling_pass = false;

    // these are env objects that are of the same level that characters can smoothly transition over to
    [SerializeField]
    private bool left_connect;
    [SerializeField]
    private bool right_connect;

    public bool WallClimb
    {
        get { return wall_climb; }
    }
    public bool CeilingClimb
    {
        get { return ceiling_climb; }
    }
    public bool CeilingPass
    {
        get { return ceiling_pass; }
    }

    public bool LeftConnect
    {
        get { return left_connect; }
    }
    public bool RightConnect
    {
        get { return right_connect; }
    }
}
