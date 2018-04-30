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
        Referencer.instance.RegisterNoise( this ); // Register, so enemies can interact with it

        // Initialize line renderer
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material( Shader.Find( "Particles/Additive" ) );
        lineRenderer.startColor = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
        lineRenderer.endColor = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
        lineRenderer.startWidth = 1.0f;
        lineRenderer.endWidth = 1.0f;
        lineRenderer.positionCount = 101;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // draw
        float x, y;
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
        for ( int i = 0; i <= 100; i++ )
        {
            float theta = 2.0f * Mathf.PI * ((float) i) / 100.0f;
            x = ( timer / lifetime ) * radius * Mathf.Cos( theta ) + position.x;
            y = ( timer / lifetime ) * radius * Mathf.Sin( theta ) + position.y;

            Vector3 pos = new Vector3( x, y, 0 );
            lineRenderer.SetPosition( i, pos );
        }

        // timer
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
        Referencer.instance.RemoveNoise( this );
        GameObject.Destroy( this.gameObject );
    }
}
