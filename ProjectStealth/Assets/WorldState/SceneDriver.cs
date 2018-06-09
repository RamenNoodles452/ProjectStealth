using UnityEngine;
using System.Collections;

public class SceneDriver : MonoBehaviour {

    public GameObject game_state_object;

	// Use this for initialization
	void Awake ()
    {
        if (GameState.instance == null)
        {
            Instantiate(game_state_object);
        }
    }
}
