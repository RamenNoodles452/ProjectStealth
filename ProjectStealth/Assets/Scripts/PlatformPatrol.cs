using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes a platform move around on a set patrol path.
public class PlatformPatrol : MonoBehaviour 
{
	#region vars
	private PatrolPath path;

	public float speed = 32.0f; // pixels / second
	public Vector3 delta;

	private bool attached = false; //TODO: make this function for enemies and players in multiplicity.
	private Player player;
	#endregion

	// Use this for initialization
	void Start () 
	{
		path = GetComponent<PatrolPath>();
		player = Referencer.instance.player.GetComponent<Player>();

		#if UNITY_EDITOR
		if ( path == null )
		{
			Debug.LogError( "Lift configured improperly: missing path." );
		}
		if ( player == null ) 
		{
			Debug.LogError( "Lift couldn't find player." );
		}
		#endif
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 aim = path.Current();
		float angle = Mathf.Atan2( aim.y - transform.position.y, aim.x - transform.position.x );
		float distance = Mathf.Sqrt( Mathf.Pow( aim.x - transform.position.x, 2.0f ) + Mathf.Pow( aim.y - transform.position.y, 2.0f ) );
		bool arrive = distance <= (speed * Time.deltaTime * Time.timeScale);

		if ( ! arrive ) 
		{
			delta = (speed * Time.deltaTime * Time.timeScale) * new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), 0.0f );
			transform.position += delta;
			if ( attached )
			{
				player.MoveWithCollision( delta );
			}
		}
		else
		{
			bool was_reset;
			delta = aim - transform.position;
			transform.position = aim;
			if ( attached )
			{
				player.MoveWithCollision( delta );
			}

			aim = path.Next( out was_reset );

			if ( was_reset && path.loop_mode == PathLoopMode.SNAP_BACK ) 
			{
				transform.position = aim;
				aim = path.Next();
				if ( attached ) { DetachPlayer(); }
			}
		}
	}

	public void AttachPlayer()
	{
		attached = true;
		player.MoveWithCollision( delta );
	}

	public void DetachPlayer()
	{
		attached = false;
	}
}
