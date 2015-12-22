using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour {

    public int LevelNumber;

    public void LoadScene()
    {
        Application.LoadLevel(LevelNumber);
    }
}
