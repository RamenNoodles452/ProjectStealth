using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelWarp : MonoBehaviour
{
    #region vars
    public string levelName;
    public Vector2 warpPosition;
    #endregion

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
        if ( collision.GetComponent<Player>() != null )
        {
            Debug.Log("Move to another level!");
            GameState.Instance.WarpToLevel( levelName, warpPosition );
        }
    }
}
