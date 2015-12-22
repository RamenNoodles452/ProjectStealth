//-GabeV
//Camera shake data class.
//Used to apply randomized shake to the camera.

using UnityEngine;
using System.Collections;

public class CameraShake 
{
	#region vars
	public Vector3 offset;
	private Vector2 magnitude;
	private float duration;
	private bool on;
	#endregion

	//Used to setup the shake's properties
	public void Begin( Vector2 p_magnitude, float p_duration )
	{
		magnitude = p_magnitude;
		duration = p_duration;
		on = true;
	}

	//Main function
	public void UpdateShake( float elapsedTime )
	{
		duration -= elapsedTime;
		if ( duration <= 0.0f ) 
		{
			duration = 0.0f;
			offset = Vector3.zero;
			magnitude = Vector2.zero;
			on = false;
		}
		else if ( on == true )
		{
			offset = new Vector3( Random.Range (-magnitude.x / 2.0f, magnitude.x / 2.0f), Random.Range (-magnitude.y / 2.0f, magnitude.y / 2.0f), 0.0f );
		}
	}

	//Use to end the effect early.
	public void End()
	{
		on = false;
	}

	//constructor stub
	public CameraShake()
	{
	}
}
