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

    /// <summary>
    /// Draws points to the screen for data visualization
    /// </summary>
    public void OnSceneGUI()
    {
        // target = implicitly passed, the object being inspected (should be of type Path, but passed as UnityEngine.Object)
        PatrolPath inspectee = ((PatrolPath)target);
        if ( inspectee == null ) { return; }
        if ( inspectee.path == null ) { return; }

        // Show the path
        if ( inspectee.path.Length > 0 )
        {
            positions = new Vector3[ inspectee.path.Length ];
            for ( int i = 0; i < inspectee.path.Length; i++ )
            {
                positions[ i ] = inspectee.path[ i ].position;

                // Allow editing points on the path in the edtor
                EditorGUI.BeginChangeCheck();
                Vector3 new_position = Handles.PositionHandle( inspectee.path[ i ].position, Quaternion.identity );

                if ( EditorGUI.EndChangeCheck() )
                {
                    // round it to the nearest pixel.
                    new_position = new Vector3( (int) new_position.x, (int) new_position.y, new_position.z );

                    Undo.RecordObject( inspectee, "Change position of a point on the patrol path" );
                    inspectee.path[ i ].position = new_position;
                }
            }
            // Show the path in the editor.
            Handles.DrawAAPolyLine( positions );
        }
    }

    /// <summary>
    /// Draws the inspector editor for the PatrolPath class
    /// </summary>
    public override void OnInspectorGUI()
    {
        PatrolPath path = (PatrolPath) target;                                 // target is implicit engine-set variable
        if ( path == null ) { return; }                                        // safety

        serializedObject.Update();                                             // does something? to serialized PathNode members in PatrolPath target
        SerializedProperty property = serializedObject.FindProperty( "path" ); // gets target.path
        if ( property == null ) { return; }                                    // safety

        EditorGUI.BeginChangeCheck();                                          // Allows you to actually edit field values
        EditorGUILayout.PropertyField( serializedObject.FindProperty( "is_flying" ) );
        EditorGUILayout.PropertyField( serializedObject.FindProperty( "loop_mode" ) );
        EditorGUILayout.PropertyField( property, true );                       // displays path and its children

        // Update Inspector UI state, allows you to edit fields
        if ( EditorGUI.EndChangeCheck() )
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}