using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Grappling hookshot
public class GrapplingHook : MonoBehaviour
{
    #region vars
    private const float MAX_RANGE = 100.0f; // pixels
    #endregion

    // Use this for initialization
    void Start ()
    {
        //
    }
    
    // Update is called once per frame
    void Update ()
    {
        
    }
    
    // Launch the hookshot!
    void Fire()
    {
        // raycast, check if object is hookshottable (pass through pass through platforms)
        // alt: check if enemy is hookshottable
        // Not hookshottable: Make a Dink! noise & spark.
    }
}
