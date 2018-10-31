using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates a 2D object
/// </summary>
public class Spin : MonoBehaviour
{
    #region vars
    public float speed; // degrees per second.
    #endregion

    // Update is called once per frame
    void Update ()
    {
        transform.rotation = Quaternion.Euler( transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + speed * Time.deltaTime * Time.timeScale );
    }
}
