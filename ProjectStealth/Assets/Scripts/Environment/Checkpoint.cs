using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// - GabeV
// Instantiable object to checkpoint the player's progress when they touch it, so they start here if they die.
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
            Vector2 checkpoint_coordinates = new Vector2( this.gameObject.transform.position.x, this.gameObject.transform.position.y );
            Referencer.instance.player.SetCheckpoint( checkpoint_coordinates );
        }
    }
}
