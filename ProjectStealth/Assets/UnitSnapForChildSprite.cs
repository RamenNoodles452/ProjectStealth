using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pixel-perfect assist script.
/// Snaps a child's transform to the correct offset for whole units on the (x,y) axes.
/// </summary>
public class UnitSnapForChildSprite : MonoBehaviour
{

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        
    }

    // Called once per frame, after all updates
    private void LateUpdate()
    {
        // Snap the sprite to the nearest whole unit in the x, y axes.
        Vector3 parent_position = transform.parent.position;
        Vector2Int snapped_position = new Vector2Int( (int) parent_position.x, (int) parent_position.y );
        // Position of child is set in world space
        transform.position = new Vector3( snapped_position.x, 
                                          snapped_position.y, 
                                          transform.position.z );
    }
}
