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
    public GameObject leftConnect;
    public GameObject rightConnect;

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
}
