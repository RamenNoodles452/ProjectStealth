using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// small utilty script
public class DestroyMe : MonoBehaviour
{
    #region vars
    public  float lifetime = 1.0f;
    private float timer = 0.0f;
    #endregion

    // Use this for initialization
    void Start ()
    {
        timer = 0.0f;
    }
    
    // Update is called once per frame
    void Update ()
    {
        timer += Time.deltaTime * Time.timeScale;
        if ( timer >= lifetime ) { GameObject.Destroy( this.gameObject ); }
    }
}
