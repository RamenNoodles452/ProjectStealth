using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores enemy patrol behaviour.

// Needed to inherit from some Unity class so the custom editor typecast from UnityEngine.Object works.
public class Path : MonoBehaviour
{
    #region vars
    // NOTE: if you add a public var, to get it to display, you may need to update PointEditor.cs
    [SerializeField]
    public bool is_flying;      // ignore such trivial things as collision & gravity
    [SerializeField]
    public PathLoopMode loop_mode;

    public PathNode[] path;
    private int index;
    #endregion

    /// <summary>
    /// Gets the next point on the path.
    /// </summary>
    public Vector3 Next()
    {
        bool was_reset;
        return Next( out was_reset );
    }

    /// <summary>
    /// Gets the next point on the path.
    /// </summary>
    public Vector3 Next( out bool was_reset )
    {
        was_reset = false;
        if ( path == null ) { return transform.position; }
        if ( path.Length <= 0 ) { return transform.position; }

        index++;
        if ( index >= path.Length )
        {
            index = 0;
            was_reset = true;
        }
        return path[ index ].position;
    }

    /// <summary>
    /// Get the current point on the path.
    /// </summary>
    public Vector3 Current()
    {
        if ( path == null ) { return transform.position; }
        if ( path.Length <= 0 ) { return transform.position; }

        return path[ index ].position;
    }

    /// <returns>The number of seconds to wait once the current destination is reached.</returns>
    public float CurrentDelay()
    {
        if ( path == null ) { return 0.0f; }
        if ( path.Length <= 0 ) { return 0.0f; }
        return path[ index ].delay;
    }
}

public enum PathLoopMode
{
    LOOP, SNAP_BACK
}

[System.Serializable]
public class PathNode
{
    public Vector3 position;
    public float   delay = 0.0f;
}
