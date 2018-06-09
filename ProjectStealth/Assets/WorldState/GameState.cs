using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{

    #region vars
    public static GameState instance; // Singleton

    //TODO: insantiate the player object within GameState
    //public CharacterStatus PlayerState;
    private Vector3 warp_position;

    public bool is_red_alert = false;
    #endregion

    void Awake ()
    {
        if ( instance == null )
        {
            //Debug.Log("Creating a new instance of the GameState");
            DontDestroyOnLoad( this.gameObject ); // Persist across Scenes
            instance = this;
        }
		else if ( instance != this )
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
    public void WarpToLevel( string level_name, Vector2 warp_coordinates )
    {
        warp_position = new Vector3( warp_coordinates.x, warp_coordinates.y, 0.0f );
        SceneManager.LoadScene(level_name); // TODO: check that levelname is not the same as current level?

        Referencer.instance.PrepareToChangeScenes(); // Destroy references about to be invalidated
    }

    /// <summary>
    /// Handles post-load manipulations
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    void OnLevelLoaded( Scene scene, LoadSceneMode mode )
    {
        if ( Referencer.instance == null ) { return; }        // initial load (causes invalid respawn)
        if ( Referencer.instance.player == null ) { return; } // initial load (causes invalid respawn)
        if ( warp_position == Vector3.zero)                    // initial load
        {
            Vector2 checkpointCoordinates = new Vector2( Referencer.instance.player.gameObject.transform.position.x, Referencer.instance.player.gameObject.transform.position.y );
            Referencer.instance.player.SetCheckpoint( checkpointCoordinates );
        }
        else
        { 
            Referencer.instance.player.gameObject.transform.position = warp_position;
            Referencer.instance.player.SetCheckpoint( new Vector2( warp_position.x, warp_position.y ) );
            // snap to!
            CameraMovement cameraMovement = Camera.main.GetComponent<CameraMovement>();
            if ( cameraMovement != null )
            {
                cameraMovement.SnapToFocalPoint();
            }
        }
    }
}
