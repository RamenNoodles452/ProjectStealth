using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Path editor tool for editing patrol paths in the editor.
// TODO: update as we add functionality to the patrol paths.
// - Gabe V

[CustomEditor( typeof( PatrolPath ) )]
public class PointEditor : Editor
{
	#region vars
	public Vector3[] positions;
	#endregion

	void OnEnable() { }

	public void OnSceneGUI()
	{
		// target = implicitly passed, the object being inspected (should be of type Path, but passed as UnityEngine.Object)
		PatrolPath inspectee = ((PatrolPath)target);
		if ( inspectee == null ) { return; }

		// Show the path
		if ( inspectee.positions.Length > 0 )
		{
			positions = new Vector3[ inspectee.positions.Length ];
			for ( int i = 0; i < inspectee.positions.Length; i++ )
			{
				positions[i] = inspectee.positions[i];

				// Allow editing points on the path in the edtor
				EditorGUI.BeginChangeCheck();
				Vector3 new_position = Handles.PositionHandle( inspectee.positions[i], Quaternion.identity );

				if ( EditorGUI.EndChangeCheck() )
				{
					// round it to the nearest pixel.
					new_position = new Vector3( (int)new_position.x, (int)new_position.y, new_position.z );

					Undo.RecordObject (inspectee, "Change position of a point on the patrol path");
					inspectee.positions[i] = new_position;
				}
			}
			// Show the path in the editor.
			Handles.DrawAAPolyLine( positions );
		}
	}
}