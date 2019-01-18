using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//-GabeV
// Referencer singleton stores references to useful "global variables"
public class Referencer : MonoBehaviour
{
    #region vars
    public static Referencer instance;

    public Player player;
    public UIScript hud_ui;
    public Grid geometry_tilemap_grid;
    public Tilemap geometry_tilemap;

    public List<GameObject> enemies = new List<GameObject>();
    public List<Noise> noises = new List<Noise>();

    // I was going to use this, then found out it would cause fewer collision problems to just make tile gameobjects handle their own.
    // It's nice to be able to access the objects representing tiles quickly, but right now we never use it, so it's a waste of memory.
    // TODO: make use of this.
    public Dictionary<int,GameObject> tile_objects;
    #endregion

    // This script is don't destroy on load (handled by gamestate via gameobject)

    // Pre-initialization
    private void Awake()
    {
        if ( instance == null )
        {
            instance = this;
            RegisterPlayer();
            RegisterScene();
            RegisterHUDUI();
        }
        else if ( instance != this )
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
        player = GameObject.Find( "PlayerCharacter" ).GetComponent<Player>();
    }

    public void RegisterHUDUI()
    {
        hud_ui = GameObject.Find( "UI Canvas" ).GetComponent<UIScript>();
    }

    /// <summary>
    /// Registers certain important objects in the scene.
    /// </summary>
    public void RegisterScene()
    {
        geometry_tilemap_grid = GameObject.Find( "Grid" ).GetComponent<Grid>();
        if ( geometry_tilemap_grid == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Fatal setup error: Scene is missing a tilemap grid object, or it was named incorrectly." );
            #endif
            return;
        }

        geometry_tilemap = geometry_tilemap_grid.GetComponentInChildren<Tilemap>();
        geometry_tilemap.CompressBounds(); // ensures bounds will be minimal by the time StartUp gets called in CustomTile.
        tile_objects = new Dictionary<int, GameObject>();
    }

    /// <summary>
    /// Retrieves the gameobject associated with the specified tile, which stores our useful custom data for that tile.
    /// </summary>
    /// <param name="position">The position of the tile, in cell space.</param>
    /// <returns>The gameobject "bound" to the given tile.</returns>
    public GameObject GetTileObjectAtPosition( Vector3Int position )
    {
        int key = HashTilePosition( position );
        if ( ! tile_objects.ContainsKey( key ) ) { return null; }
        return tile_objects[ key ];
    }

    /// <summary>
    /// Called by tilemap tiles on initialization, to store each tile object's association with a position in the tile map.
    /// </summary>
    /// <param name="position">The position, in cell space, of the tile.</param>
    /// <param name="game_object">The object storing the custom tile data for the tile at position.</param>
    public void RegisterTileGameObject( Vector3Int position, GameObject game_object )
    {
        if ( gameObject == null ) { return; } // would be pointless.
        if ( position.x < 0.0f )  { return; }
        if ( position.y < 0.0f )  { return; }

        // Duplicates aren't allowed.
        int key = HashTilePosition( position );
        if ( tile_objects.ContainsKey( key ) )
        {
            #if UNITY_EDITOR
            Debug.LogError( "Duplicate key in tilemap objects." );
            Debug.Log( "key: " + key + " position: " + position + " game object: " + game_object + " grid size: " + geometry_tilemap.cellBounds.size );
            #endif
            return;
        }
        tile_objects.Add( key, game_object );
    }

    /// <summary>
    /// Gets an integer hash key from a tile's position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private int HashTilePosition( Vector3Int position )
    {
        BoundsInt bounds = geometry_tilemap.cellBounds; // NOTE: size cannot be permitted to change.
        // NOTE: no Z, use 1 layer for geometry.
        #if UNITY_EDITOR
        if ( bounds.size.z > 1 ) { Debug.LogError( "It looks like the geometry tilemap is using multiple layers. This isn't supported." ); }
        #endif
        return ( position.x - bounds.position.x ) + ( position.y - bounds.position.y ) * bounds.size.x;
    }

    /// <summary>
    /// Should be called before unloading a scene
    /// </summary>
    public void PrepareToChangeScenes()
    {
        RemoveAllEnemies();
        RemoveAllNoises();
    }

    /// <summary>
    /// Removes all references to enemies.
    /// To be called before loading a new scene.
    /// </summary>
    private void RemoveAllEnemies()
    {
        enemies.RemoveAll( AllEnemies );
    }

    /// <summary>
    /// Search matching predicate function for removing all enemies
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns>True, always</returns>
    private static bool AllEnemies( GameObject enemy ) { return true; }

    /// <summary>
    /// Adds a reference to an enemy.
    /// To be called by each enemy on awake when a scene is loaded.
    /// </summary>
    /// <param name="enemy">The enemy to cache a reference to</param>
    public void RegisterEnemy( GameObject enemy )
    {
        enemies.Add( enemy );
    }

    public void RemoveEnemy( GameObject enemy )
    {
        enemies.Remove( enemy );
    }

    /// <summary>
    /// Removes all references to noises.
    /// To be called before loading a new scene.
    /// </summary>
    private void RemoveAllNoises()
    {
        noises.RemoveAll( AllNoise );
    }

    /// <summary>
    /// Search matching predicate function for removing all noises
    /// </summary>
    /// <param name="noise"></param>
    /// <returns>True, always</returns>
    private static bool AllNoise( Noise noise ) { return true; }

    public void RegisterNoise( Noise noise )
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
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
