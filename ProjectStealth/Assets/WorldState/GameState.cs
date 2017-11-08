using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{

    #region vars
    public static GameState Instance; // Singleton

    //TODO: insantiate the player object within GameState
    //public CharacterStatus PlayerState;
    private Vector3 warpPosition;
    #endregion

    void Awake ()
    {
        if ( Instance == null )
        {
            //Debug.Log("Creating a new instance of the GameState");
            DontDestroyOnLoad( this.gameObject ); // Persist across Scenes
            Instance = this;
        }
		else if ( Instance != this )
        {
            Debug.LogWarning("Attempted to create two GameState instances - destroying");
            Destroy(gameObject);
        }
	}

    void Update()
    {

    }

    private void OnEnable()
    {
        // Since they went and deprecated OnLevelWasLoaded, now we have to do this
        SceneManager.sceneLoaded += OnLevelLoaded; // delegate subscribe
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded; // delegate unsubscribe
    }

    /// <summary>
    /// Warps the player to a new level, and loads a new level.
    /// </summary>
    /// <param name="levelName">The scene name of the level</param>
    /// <param name="warpCoordinates">The x and y coordinates to place the player (centerpoint) in the new level</param>
    public void WarpToLevel( string levelName, Vector2 warpCoordinates )
    {
        warpPosition = new Vector3( warpCoordinates.x, warpCoordinates.y, 0.0f );
        SceneManager.LoadScene(levelName); // TODO: check that levelname is not the same as current level?
        Referencer.Instance.RemoveAllEnemies();
    }

    /// <summary>
    /// Handles post-load manipulations
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    void OnLevelLoaded( Scene scene, LoadSceneMode mode )
    {
        if ( Referencer.Instance == null ) { return; }        // initial load (causes invalid respawn)
        if ( Referencer.Instance.player == null ) { return; } // initial load (causes invalid respawn)
        if ( warpPosition == Vector3.zero)                    // initial load
        {
            Referencer.Instance.player.SetCheckpoint( new Vector2( Referencer.Instance.player.gameObject.transform.position.x, Referencer.Instance.player.gameObject.transform.position.y ) );
        }
        else
        { 
            Referencer.Instance.player.gameObject.transform.position = warpPosition;
            Referencer.Instance.player.SetCheckpoint( new Vector2( warpPosition.x, warpPosition.y ) );
            // snap to!
            CameraMovement cameraMovement = Camera.main.GetComponent<CameraMovement>();
            if ( cameraMovement != null )
            {
                cameraMovement.SnapToFocalPoint();
            }
        }
        //TODO: snap main camera + set focus point
    }
}
