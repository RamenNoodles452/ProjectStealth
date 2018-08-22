using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOffset : MonoBehaviour
{
    public Vector3 offset;
    public CharacterStats char_stats;

    public void MoveByOffset()
    {
        float sign = 1.0f;
        if ( char_stats.IsFacingLeft() ) { sign = -1.0f; }
        transform.position += new Vector3( offset.x * sign, offset.y, offset.z );
    }

    private void Awake()
    {
        char_stats = GetComponent<CharacterStats>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
