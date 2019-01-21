using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scrambles objects in a radius.
/// </summary>
public class EMP : MonoBehaviour
{
    #region vars
    public float radius = 32.0f; // pixels
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if ( ! TriggerBuildValidator.Validate( this.gameObject ) ) { return; }
        CircleCollider2D circle_collider = GetComponent<CircleCollider2D>();
        circle_collider.radius = radius;
    }

    /// <summary>
    /// Triggered when another collider enters this object's trigger radius.
    /// </summary>
    /// <param name="collision">The collider2D of the other object, within this EMP's radius.</param>
    private void OnTriggerEnter2D( Collider2D collision )
    {
        ElectroMagneticPulse( collision );
    }

    /// <summary>
    /// Checks if a collider is EMP-able, and if so, scrambles it (whatever that means for the object).
    /// </summary>
    /// <param name="collision">The collider2D of the other object, within this EMP's radius.</param>
    private void ElectroMagneticPulse( Collider2D collision )
    {
        EventOnEMP event_on_EMP = collision.gameObject.GetComponent<EventOnEMP>();
        if ( event_on_EMP != null )
        {
            event_on_EMP.OnEMP();
        }
    }
}
