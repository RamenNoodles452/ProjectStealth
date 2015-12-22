using UnityEngine;
using System.Collections;

public class SceneDriver : MonoBehaviour {

    public GameObject gameStateObject;

	// Use this for initialization
	void Awake ()
    {
        if (GameState.Instance == null)
        {
            Instantiate(gameStateObject);
        }
    }
}
