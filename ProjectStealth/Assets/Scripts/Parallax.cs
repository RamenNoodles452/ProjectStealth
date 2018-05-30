using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles moving background objects counter to the camera
public class Parallax : MonoBehaviour 
{
	#region vars
	private const float MAX_DEPTH = 10.0f;
	private float scale;
	private Vector3 previous_camera_position;
	#endregion

	// Use this for initialization
	void Start () 
	{
		#if UNITY_EDITOR
		if ( transform.position.z < 0.0f || transform.position.z > MAX_DEPTH )
		{
			Debug.LogError( "Background parallax object's z coordinates are out of range" );
		}
		#endif

		scale = transform.position.z / MAX_DEPTH;
		previous_camera_position = Camera.main.transform.position;
	}

	// Update is called once per frame
	void Update () 
	{
		float delta = Camera.main.transform.position.x - previous_camera_position.x;
		transform.position = transform.position + new Vector3( delta * scale, 0.0f, 0.0f );

		previous_camera_position = Camera.main.transform.position;
	}
}
