using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGameOnClick : MonoBehaviour
{
    /// <summary>
    /// Quits the game when the player pushes the quit button.
    /// </summary>
    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Exit();
        #endif
    }
}
