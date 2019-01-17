using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Because sprites are child object for player, receive animation triggers, pass them to parent.
public class PlayerAnimationTrigger : MonoBehaviour
{

    //
    public void WallToGroundStop()
    {
        //transform.parent.GetComponent<MagGripUpgrade>().WallToGroundStop();
        Debug.LogError( "Hey, stop using this. You don't need to anymore." );
    }

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        
    }
}
