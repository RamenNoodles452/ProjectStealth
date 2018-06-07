using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftDetector : MonoBehaviour 
{
	#region vars
	PlatformPatrol platform_path;
	#endregion

	// Use this for initialization
	void Start () 
	{
		platform_path = GetComponentInParent<PlatformPatrol>();
		if ( platform_path == null ) 
		{
			Debug.LogError( "Lift configuration issue!" );
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void OnTriggerEnter2D( Collider2D other )
	{
		if ( other.gameObject.layer == LayerMask.NameToLayer( "character objects" ) )
		{
		    Debug.Log( "on platform" );
		    platform_path.AttachPlayer();
		}
	}

	void OnTriggerExit2D( Collider2D other )
	{
		if ( other.gameObject.layer == LayerMask.NameToLayer( "character objects" ) ) 
		{
			Debug.Log ( "off platform" );
			platform_path.DetachPlayer();
		}
	}
}
