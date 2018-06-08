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
		    platform_path.AttachPlayer();

			//reset state from climbing, etc.
			if ( Referencer.instance.player.GetComponent<CharacterStats>().current_master_state == CharEnums.MasterState.ClimbState )
			{
			    Referencer.instance.player.GetComponent<MagGripUpgrade>().StopClimbing();
			}
		}
	}

	void OnTriggerExit2D( Collider2D other )
	{
		if ( other.gameObject.layer == LayerMask.NameToLayer( "character objects" ) ) 
		{
			platform_path.DetachPlayer();
		}
	}
}
