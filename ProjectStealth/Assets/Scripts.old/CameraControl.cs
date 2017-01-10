//-GabeV
//Camera Controller Script

using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour 
{
	#region vars
	public GameObject playerCharacter;

	private Camera cam;
	private CameraShake camShake;

	private Vector3[] cameraPositions;
	private int cameraOverwriteIndex = 0;
	//"looped" arrays for smoothing using simple filtering:
	//[a], [b], [c]
	// ^ replace
	//[1], [b], [c]
	//      ^ replace
	//[1], [2], [c]
	//           ^ replace
	#endregion

	void Awake()
	{
		Spider.cameraController = this;
		if ( playerCharacter == null) { playerCharacter = GameObject.Find ("Player"); } //eew, gross

		cam = Camera.main; //attach to camera, getcomponent<Camera> instead
		camShake = new CameraShake ();
		cameraPositions = new Vector3[10];

		//fill the camera position blending vector
		for ( int i = 0; i < cameraPositions.Length; i++ )
		{
			cameraPositions[i] = new Vector3( cam.transform.position.x, cam.transform.position.y, cam.transform.position.z );
		}
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		float dt = Time.deltaTime * TimeScale.timeScale;
		camShake.UpdateShake ( dt ); //shake effect

		//Get where we want the camera to end up
		Vector3 targetPos = new Vector3( playerCharacter.transform.position.x, playerCharacter.transform.position.y, cam.transform.position.z );

		//filter: smooth the camera follow.
		cameraPositions [cameraOverwriteIndex] = targetPos; //add the target to the rolling average
		cameraOverwriteIndex++;
		cameraOverwriteIndex = cameraOverwriteIndex % cameraPositions.Length;

		//get where the camera will actually point via average
		float avgx = 0.0f;
		float avgy = 0.0f;
		float avgz = 0.0f;
		for ( int i = 0; i < cameraPositions.Length; i++ )
		{
			avgx += cameraPositions[i].x;
			avgy += cameraPositions[i].y;
			avgz += cameraPositions[i].z;
		}
		avgx = avgx / ((float)cameraPositions.Length);
		avgy = avgy / ((float)cameraPositions.Length);
		avgz = avgz / ((float)cameraPositions.Length);
		Vector3 avgPos = new Vector3 (avgx, avgy, avgz);

		cam.transform.position = avgPos + camShake.offset; //set position
	}

	//Use to force the camera to shake
	public void Shake( Vector2 magnitude, float duration )
	{
		//magnitude is how much the camera moves in x / y space
		//duration is how long, in seconds, the shake lasts
		camShake.Begin ( magnitude, duration );
	}

	//Use to stop camera shake
	public void StopShake()
	{
		camShake.End ();
	}
}
