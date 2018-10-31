using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles moving background objects counter to the camera
// Usage Note: when placing parallax objects in the editor, place them as if the camera were centered on them.
public class Parallax : MonoBehaviour
{
    #region vars
    private const float MAX_DEPTH = 10.0f;
    private float scale;
    private Vector3 previous_camera_position;
    #endregion

    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        bool is_in_range = true;
        if ( transform.position.z < 0.0f )
        {
            transform.position = new Vector3( transform.position.x, transform.position.y, 0.0f );
            is_in_range = false;
        }
        if ( transform.position.z > MAX_DEPTH )
        {
            transform.position = new Vector3( transform.position.x, transform.position.y, MAX_DEPTH );
            is_in_range = false;
        }
        if ( ! is_in_range )
        {
            Debug.LogError( "Background parallax object's z coordinates are out of range" );
        }
#endif

        scale = transform.position.z / MAX_DEPTH;
        previous_camera_position = Camera.main.transform.position;
        // Compensate for difference between the initial object placement 
        // (which assumed the camera was centered on the object) vs. the actual initial camera placement.
        float delta =  Camera.main.transform.position.x - this.transform.position.x;
        transform.position = transform.position + Offset( delta, scale );
    }

    // Update is called once per frame
    void Update()
    {
        float delta = Camera.main.transform.position.x - previous_camera_position.x;
        transform.position = transform.position + Offset( delta, scale );

        previous_camera_position = Camera.main.transform.position;
    }

    /// <summary>
    /// Calculate how much the object should move, based on how much the camera moved. (confined to X axis... for now)
    /// </summary>
    /// <param name="delta">The change in camera's X coordinate</param>
    /// <param name="scale">The amount that the change should be scaled by. Based on the object's depth.</param>
    private Vector3 Offset( float delta, float scale )
    {
        return new Vector3( delta * scale, 0.0f, 0.0f );
    }
}
