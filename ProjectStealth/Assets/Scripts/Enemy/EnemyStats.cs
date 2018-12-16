using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Common enemy core script.
// Handles HP, dying, etc.
public class EnemyStats : MonoBehaviour
{
    #region vars
    private float health = 100.0f; // baseline
    #endregion

    // Use this for early initialization (references)
    private void Awake()
    {
        
    }

    // Use this for initialization
    void Start ()
    {
        
    }
    
    // Update is called once per frame
    void Update ()
    {
        
    }

    /// <summary>
    /// Deals damage to the enemy.
    /// </summary>
    /// <param name="damage">The amount of damage to deal.</param>
    public void Hit( float damage )
    {
        health -= damage;

        if ( health < 0.0f )
        {
            // TODO: kill
        }
        else
        {
            // hurt indicator
        }
    }
}
