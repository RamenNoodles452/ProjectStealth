using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// turns laser on and off
public class LaserToggle : MonoBehaviour
{
    #region vars
    [SerializeField]
    private bool    begin_on = true; // will begin in the "on" state if true, will begin "off" if false.
    [SerializeField]
    private float[] durations;       // time between toggles, or the duration of each state.
    // ex: 1 duration  = on and off will be the same fixed length.
    // ex: 2 durations = initial state will be duration[0], and opposite state will be duration[1].
    // should really only be a multiple of 2 after that.

    private int index = 0;
    private bool is_on;
    private float timer = 0.0f;
    #endregion

    // Use this for initialization
    void Start ()
    {
        is_on = begin_on;
        if ( durations.Length == 0 )
        {
            Destroy( this ); // remove this component if it is not being utilized.
        }
    }
    
    // Update is called once per frame
    void Update ()
    {
        if ( durations.Length == 0 ) { return; }
        timer += Time.deltaTime * Time.timeScale;

        if ( timer > durations[ index ] )
        {
            is_on = ! is_on;
            GetComponent<LaserBeam>().is_on = is_on;
            timer -= durations[ index ];
            index = ( index + 1 ) % durations.Length;
        }
    }
}
