using UnityEngine;
using System.Collections;

public class GameState : MonoBehaviour {

    public static GameState Instance; // Singleton
    
	//TODO: insantiate the player object within GameState
	//public CharacterStatus PlayerState;

    void Awake () {
        if (Instance == null)
        {
            Debug.Log("Creating a new instance of the GameState");
			DontDestroyOnLoad(gameObject); // Persist across Scenes
            Instance = this;
        }
		else if (Instance != this)
        {
            Debug.LogWarning("Attempted to create two GameState instances - destroying");
            Destroy(gameObject);
        }
	}

    void Update()
    {

    }
}
