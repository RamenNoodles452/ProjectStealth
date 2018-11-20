using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Do common trigger build checks.
public class TriggerBuildValidator
{
    /// <summary>
    /// Checks that a trigger object is built correctly.
    /// </summary>
    /// <param name="obj">The gameobject to check.</param>
    /// <returns>True if the build is valid, false otherwise.</returns>
    public static bool Validate( GameObject obj )
    {
        Collider2D collider = obj.GetComponent<Collider2D>();
        if ( collider == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing component: Collider2D on " + obj );
            #endif
            return false;
        }
        if ( ! collider.isTrigger )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Invalid configuration: Collider2D is not a trigger on " + obj );
            #endif
            return false;
        }

        Rigidbody2D rigidbody = obj.GetComponent<Rigidbody2D>();
        if ( rigidbody == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Missing component: Rigidbody2D on " + obj );
            #endif
            return false;
        }
        if ( ! rigidbody.isKinematic )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Invalid configuration: Rigidbody2D is not kinematic on " + obj );
            #endif
            return false;
        }
        return true;
    }
}
