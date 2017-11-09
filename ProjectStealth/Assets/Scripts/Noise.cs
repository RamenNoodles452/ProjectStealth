using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-GabeV
//Class for "sound" detectable by enemies
public class Noise : MonoBehaviour
{
    #region vars
    public Vector3 position;
    public float radius;
    public float lifetime;

    private float timer;
    #endregion

    // Use this for initialization
    void Start ()
    {
        position = this.gameObject.transform.position;
        Referencer.Instance.AddNoise( this ); // Register, so enemies can interact with it
	}
	
	// Update is called once per frame
	void Update ()
    {
        timer += Time.deltaTime * TimeScale.timeScale;
        if ( timer >= lifetime )
        {
            CleanUp();
        }
	}

    /// <summary>
    /// Unregisters and destroys the noise at the end of its lifetime.
    /// </summary>
    private void CleanUp()
    {
        Referencer.Instance.RemoveNoise( this );
        GameObject.Destroy( this.gameObject );
    }
}
