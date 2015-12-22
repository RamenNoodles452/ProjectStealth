using UnityEngine;
using System.Collections;

public class GameState : MonoBehaviour {

    public static GameState Instance; // Singleton
    public CharacterStatus PlayerState;

    void Awake () {
        if (Instance == null)
        {
            Debug.Log("Creating a new instance of the GameState");
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Attempted to create two GameState instances - destroying");
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject); // Persist across Scenes
	}
}
