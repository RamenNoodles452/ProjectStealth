using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour {

    public string sceneName;

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
}
