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
