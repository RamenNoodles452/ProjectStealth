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

    public List<Noise> noises = new List<Noise>();
    #endregion

    // This script is don't destroy on load (handled by gamestate via gameobject)

    // Pre-initialization
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            RegisterPlayer();
        }
        else if (Instance != this)
        {
            Debug.LogError("Uh oh! Someone tried to create multiple instances of the master referencer!");
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

    public void PrepareToChangeScenes()
    {
        RemoveAllEnemies();
        RemoveAllNoises();
    }

    /// <summary>
    /// Removes all references to enemies.
    /// To be called before loading a new scene.
    /// </summary>
    public void RemoveAllEnemies()
    {
        //TODO:
    }

    private static bool AllEnemies( GameObject enemy )
    {
        return true;
    }

    /// <summary>
    /// Adds a reference to an enemy.
    /// To be called by each enemy on awake when a scene is loaded.
    /// </summary>
    /// <param name="enemy">The enemy to cache a reference to</param>
    public void AddEnemy(GameObject enemy)
    {
        //TODO:
    }

    public void RemoveEnemy( GameObject enemy )
    {
        //TODO:
    }

    public void RemoveAllNoises()
    {
        noises.RemoveAll( AllNoise );
    }

    /// <summary>
    /// Search matching predicate function for removing all noises
    /// </summary>
    /// <param name="noise"></param>
    /// <returns>True, always</returns>
    private static bool AllNoise( Noise noise )
    {
        return true;
    }

    public void AddNoise( Noise noise )
    {
        //TODO: return true/false?
        noises.Add( noise );
    }

    public void RemoveNoise( Noise noise )
    {
        //TODO: return true/false?
        noises.Remove( noise );
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
