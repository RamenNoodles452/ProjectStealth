using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameOnClick : MonoBehaviour
{

    /// <summary>
    /// Starts a new game when the player pushes the button.
    /// </summary>
    public void NewGame()
    {
        SceneManager.LoadScene( "GabeTestScene" );
    }
}
