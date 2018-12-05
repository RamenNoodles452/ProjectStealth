using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// disables kill on touch if the player is not moving.
// If they ARE moving, kill them.
public class MotionSensing : MonoBehaviour
{

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        if ( Referencer.instance.player.GetComponent<PlayerStats>().IsIdle )
        {
            if ( GetComponent<KillOnTouch>() != null ) { Destroy( GetComponent<KillOnTouch>() ); }
        }
        else
        {
            if ( GetComponent<KillOnTouch>() == null ) { gameObject.AddComponent<KillOnTouch>(); }
        }
    }
}
