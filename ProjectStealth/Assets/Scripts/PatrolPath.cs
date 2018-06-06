using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores enemy patrol behaviour.

// Needed to inherit from some Unity class so the custom editor typecast from UnityEngine.Object works.
public class PatrolPath : MonoBehaviour
{
	#region vars
	public bool is_flying;      // ignore such trivial things as collision & gravity
	public Vector3[] positions;
	// public float[] delays; // probably better to merge positions & delays instead of using parallel arrays
	private int index;
	#endregion

	/// <summary>
	/// Gets the next point on the path.
	/// </summary>
	public Vector3 Next()
	{
		if ( positions == null ) { return transform.position; }
		if ( positions.Length <= 0 ) { return transform.position; }

		index++;
		if ( index >= positions.Length )
		{
			index = 0;
		}
		return positions[index];
	}

	/// <summary>
	/// Get the current point on the path.
	/// </summary>
	public Vector3 Current()
	{
		if ( positions == null ) { return transform.position; }
		if ( positions.Length <= 0 ) { return transform.position; }

		return positions[index];
	}
}
