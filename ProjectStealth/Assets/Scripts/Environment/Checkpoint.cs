using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            Vector2 checkpointCoordinates = new Vector2( this.gameObject.transform.position.x, this.gameObject.transform.position.y );
            Referencer.Instance.player.SetCheckpoint( checkpointCoordinates );
        }
    }
}
