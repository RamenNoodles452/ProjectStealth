using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

/// <summary>
/// This class is a custom tile for use with tilemaps.
/// GOAL: make collisionType data (and other data) accessible for each tile. (bind data to tiles)
/// We do this by instantiating a gameobject with an attached component that holds the tile data, 
/// and binding that gameobject to the GetTileData method's TileData.gameObject (instead of null, which is the default).
/// This is not very efficient, but it works, and I didn't have to write a custom TileMap, + TilePalette.
/// 
/// - Gabriel Violette
/// </summary>
[System.Serializable]
[CreateAssetMenu( fileName = "New Custom Tile", menuName = "Tiles/CustomTile" )] // Adds option to create new asset for this type of object
public class CustomTile : TileBase
{
    #region vars
    #endregion

    #region properties
    // Nonstandard naming convention, using Unity style here
    #region standard
    public Sprite sprite{ get; set; }
    public Color color { get; set; }
    public Matrix4x4 transform { get; set; }
    //public GameObject gameObject { get; set; }
    public TileFlags flags { get; set; }
    public Tile.ColliderType colliderType { get; set; }
    #endregion

    #region custom
    // THIS is the "big" custom addition to my custom tile assets.
    public GameObject prefab { get; set; }
    // Tracks the instance of the prefab, for retreival via GetTileData.
    private GameObject instance { get; set; }
    #endregion

    #region gutter garbage
    // This was one earlier approach: add data to the tile. 
    // It didn't work (Tilemap limitations). Left as a reminder not to try this.
    //[SerializeField]
    //public CollisionType collisionType { get; set; }
    #endregion
    #endregion

    /// <summary>
    /// Updates other tiles in the tilemap when this tile is added to the tilemap
    /// </summary>
    /// <param name="position">The position of the tile (world/grid/relative to?)</param>
    /// <param name="tilemap">The tilemap to refresh</param>
    public override void RefreshTile( Vector3Int position, ITilemap tilemap )
    {
        tilemap.RefreshTile( position );
        //base.RefreshTile( position, tilemap );
    }

    /// <summary>
    /// Gets the data (sprite, color, transform, gameobject, flags (color/transform/spawngameobj), collidertype) for this tile.
    /// </summary>
    /// <param name="position">The position of the tile to read.</param>
    /// <param name="tilemap">The tilemap to read from.</param>
    /// <param name="tileData">Output param, returns the tile data.</param>
    public override void GetTileData( Vector3Int position, ITilemap tilemap, ref TileData tileData )
    {
        tileData.gameObject = instance;
        tileData.sprite = sprite;
        tileData.color = color;
        tileData.transform = tileData.transform;
        tileData.colliderType = colliderType;
        tileData.flags = flags;
        //base.GetTileData( position, tilemap, ref tileData );
    }

    /// <summary>
    /// Determine whether the tile is animated or not.
    /// </summary>
    /// <param name="position">The position of the tile.</param>
    /// <param name="tilemap">The tilemap the tile is in.</param>
    /// <param name="tileAnimationData">Output param, returns animation data.</param>
    /// <returns>True if the tile is animated, false otherwise.</returns>
    public override bool GetTileAnimationData( Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData )
    {
        return base.GetTileAnimationData( position, tilemap, ref tileAnimationData );
    }

    /// <summary>
    /// Called when the tilemap updates for the first time.
    /// </summary>
    /// <param name="position">The position of the tile.</param>
    /// <param name="tilemap">The tilemap the tile is in.</param>
    /// <param name="go">The gameobject instantiated for the tile (unused, typically passed in as null)</param>
    /// <returns></returns>
    public override bool StartUp( Vector3Int position, ITilemap tilemap, GameObject go )
    {
        instance = InstantiateGameObject( position, tilemap ); // Always making real gameobjects for this is bad for performance.
        return true;
        //return base.StartUp( position, tilemap, go );
    }

    // Instantiates the prefab.
    private GameObject InstantiateGameObject( Vector3Int position, ITilemap tilemap )
    {
        if ( ! Application.isPlaying ) { return null; } // Only do this in "play" mode, not edit mode. (tilebase code calls startup in edit mode)
        if ( prefab == null )
        {
            Debug.LogError( "This custom tile is missing an assigned prefab." );
            return null;
        }

        // 1) Unity decided to only provide the ITilemap interface to StartUp, which means no Tilemap.GetCellCenterWorld.
        // 2) It also doesn't have an accessible grid property or component.
        //    Grid gridLayout = tilemap.GetComponent<Grid>();
        // 3) I don't want to hack it by hardcoding.

        // So we grab the grid from the scene on awake, and make it accessible.
        Grid grid = Referencer.instance.tilemap_grid;
        if ( grid == null ) { return null; }

        // Startup is called by each tile in the tilemap once AND by each tile in the tile palette once. 
        // Why? IDK. Another problem we need to solve, though.
        // We need to figure out what is calling this. So we compare the passed tilemap (could be tilepalette) with the main tilemap from the scene.
        Tilemap geometry_tilemap = grid.GetComponentInChildren<Tilemap>();
        if ( geometry_tilemap == null ) { return null; }

        // If called from tile palette, ignore. 
        // There IS an edge case if the tile palette matches these properties, 
        // so I gave the tile palette a negative origin to prevent this from duplicating the entire tile palette into the level.
        if ( tilemap.origin != geometry_tilemap.origin || tilemap.size != geometry_tilemap.size) { return null; }

        // Enforce positive origin for the main geometry tilemap (to prevent the tile palette from ever being confused with the geometry tilemap).
        if ( position.x < 0 || position.y < 0 )
        {
            #if UNITY_EDITOR
            Debug.LogError( "A tile in the main geometry tilemap was placed at cell position: " + position 
                + ". Tiles are not allowed to have negative x/y cell coordinates by a convention that prevents a horrible horrible bug. Delete this tile or shift the tiles in the tilemap." );
            #endif
            return null;
        }

        // Place the prefab to store the tile's data!
        Vector3 placementPosition = grid.GetCellCenterWorld( position );
        GameObject game_object = Instantiate( prefab, placementPosition, Quaternion.identity, geometry_tilemap.gameObject.transform );
        game_object.name = "Tile " + position;
        return game_object;
    }
}

#if UNITY_EDITOR
/// <summary>
/// A custom editor is needed for our useful custom tiles, to be able to manipulate them in useful ways. Usefully.
/// </summary>
[CustomEditor( typeof( CustomTile ) )]
public class MyTileEditor : Editor
{
    private CustomTile tile { get { return (CustomTile) target; } }

    // Controls the editor UI.
    public override void OnInspectorGUI()
    {
        // Allow editing these properties
        EditorGUI.BeginChangeCheck();

        tile.sprite = (Sprite) EditorGUILayout.ObjectField( "Sprite", tile.sprite, typeof( Sprite ), true );
        tile.color = EditorGUILayout.ColorField( "Color", tile.color );
        tile.colliderType = (Tile.ColliderType) EditorGUILayout.EnumPopup( "Collider Type", tile.colliderType ); // Should typically be set to "grid" for our purposes.
        // This is the big custom change.
        tile.prefab = (GameObject) EditorGUILayout.ObjectField( "Prefab", tile.prefab, typeof( GameObject ), true );

        // An earlier approach had fields for collision data here, too.

        // If it was changed
        if ( EditorGUI.EndChangeCheck() )
        {
            EditorUtility.SetDirty( tile );
        }
    }

    #region sprite preview
    // Renders a preview.
    public override Texture2D RenderStaticPreview( string assetPath, Object[] subAssets, int width, int height )
    {
        if ( GetEditorPreviewSprite() != null )
        {
            Type type = GetType( "UnityEditor.SpriteUtility" );
            if ( type != null )
            {
                MethodInfo method = type.GetMethod( "RenderStaticPreview", new[] { typeof( Sprite ), typeof( Color ), typeof( int ), typeof( int ) } );
                if ( method != null )
                {
                    object ret = method.Invoke("RenderStaticPreview", new object[] { GetEditorPreviewSprite(), Color.white, width, height });
                    if ( ret is Texture2D )
                    {
                        return ret as Texture2D;
                    }
                }
            }
        }
        return base.RenderStaticPreview( assetPath, subAssets, width, height );
    }

    // Uses assembly information to get the type of the type with the given name.
    private static Type GetType( string typeName )
    {
        Type type = Type.GetType( typeName );
        if ( type != null ) { return type; }

        if ( typeName.Contains( "." ) )
        {
            string assemblyName = typeName.Substring( 0, typeName.IndexOf( '.' ) );
            Assembly assembly = Assembly.Load(assemblyName);
            if ( assembly == null ) { return null; }
            type = assembly.GetType( typeName );
            if ( type != null ) { return type; }
        }

        Assembly currentAssembly = Assembly.GetExecutingAssembly();
        AssemblyName[] referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach ( AssemblyName assemblyName in referencedAssemblies )
        {
            Assembly assembly = Assembly.Load(assemblyName);
            if ( assembly != null )
            {
                type = assembly.GetType( typeName );
                if ( type != null ) { return type; }
            }
        }
        return null;
    }

    // Gets the sprite to use in the preview (used in this custom editor and the TilePalette).
    private Sprite GetEditorPreviewSprite()
    {
        return tile.sprite;
    }
    #endregion
}
#endif
