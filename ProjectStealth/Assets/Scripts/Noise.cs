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
        LineRenderer line_renderer  = gameObject.AddComponent<LineRenderer>();
        line_renderer.material      = new Material( Shader.Find( "Particles/Additive" ) );
        line_renderer.startColor    = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
        line_renderer.endColor      = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
        line_renderer.startWidth    = 1.0f;
        line_renderer.endWidth      = 1.0f;
        line_renderer.positionCount = 101;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // draw
        float x, y;
        LineRenderer line_renderer = gameObject.GetComponent<LineRenderer>();
        for ( int i = 0; i <= 100; i++ )
        {
            float theta = 2.0f * Mathf.PI * ((float) i) / 100.0f;
            x = ( timer / lifetime ) * radius * Mathf.Cos( theta ) + position.x;
            y = ( timer / lifetime ) * radius * Mathf.Sin( theta ) + position.y;

            Vector3 pos = new Vector3( x, y, 0 );
            line_renderer.SetPosition( i, pos );
        }

        // timer
        timer += Time.deltaTime * Time.timeScale;
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
