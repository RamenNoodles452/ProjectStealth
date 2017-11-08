using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-GabeV
// Referencer singleton stores references to useful "global variables"
public class Referencer : MonoBehaviour
{
    #region vars
    public static Referencer Instance;

    public Player player;
    //TODO: enemies: register on creation
    #endregion

    // This script is don't destroy on load (handled by gamestate via gameobject)

    // Pre-initialization
    private void Awake()
    {
        if ( Instance == null )
        {
            Instance = this;
            RegisterPlayer();
        }
        else if ( Instance != this )
        {
            Debug.LogError( "Uh oh! Someone tried to create multiple instances of the master referencer!" );
        }
    }

    public void RemovePlayer()
    {
        player = null;
    }

    public void RegisterPlayer()
    {
        player = GameObject.Find("PlayerCharacter").GetComponent<Player>();
    }

    /// <summary>
    /// Removes all references to enemies.
    /// To be called before loading a new scene.
    /// </summary>
    public void RemoveAllEnemies()
    {
        //TODO:
    }

    /// <summary>
    /// Adds a reference to an enemy.
    /// To be called by each enemy on awake when a scene is loaded.
    /// </summary>
    /// <param name="enemy">The enemy to cache a reference to</param>
    public void AddEnemy( GameObject enemy )
    {
        //TODO:
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
