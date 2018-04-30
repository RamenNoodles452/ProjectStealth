using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enemy bullet
public class Bullet : MonoBehaviour 
{
	#region vars
	private float lifetime = 3.0f;
	private float timer = 0.0f;
	private float speed = 100.0f;
	private float angle = 0.0f;
	private bool is_homing = false;
	#endregion

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( is_homing )
		{
		    Vector3 player_position = Referencer.instance.player.transform.position;
			angle = Mathf.Atan2( player_position.y - transform.position.y, player_position.x - transform.position.x );
		}
		transform.position += new Vector3( speed * Mathf.Cos( angle ), speed * Mathf.Sin( angle ), 0.0f ) * Time.deltaTime * TimeScale.timeScale;

		timer += Time.deltaTime * TimeScale.timeScale;
		if ( timer > lifetime )
		{
			Destroy( this.gameObject );
		}
	}

	public float Angle
	{
		get 
		{
			return angle;
		}
		set
		{
			angle = value;
		}
	}
}
