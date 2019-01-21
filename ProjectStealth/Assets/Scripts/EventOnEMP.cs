using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Triggers specified code when an Electro-Magnetic Pulse hits this object.
/// </summary>
public class EventOnEMP : MonoBehaviour
{
    #region vars
    [SerializeField]
    private UnityEvent on_EMP; // Set up callbacks in the editor. NEVER, EVER use EventOnEMP.OnEMP()!
    #endregion

    /// <summary>
    /// Use this for early initialization.
    /// </summary>
    private void Awake()
    {
        // Validate that this object can be hit by an EMP.

        Collider2D collider2D = GetComponent<Collider2D>();
        if ( collider2D == null )
        {
            #if UNITY_EDITOR
            Debug.LogError( this.gameObject + " has an EventOnEMP script, but no collider2D, so it can't be hit by an EMP. Removing EventOnEMP." );
            #endif
            Destroy( this ); // Remove this component.
        }

        if ( on_EMP.GetPersistentEventCount() <= 0 )
        {
            #if UNITY_EDITOR
            Debug.LogError( this.gameObject + " has an EventOnEMP script, but the event list is empty. Removing EventOnEMP." );
            #endif
            Destroy( this ); // Remove this component.
        }
    }

    /// <summary>
    /// Hook to call when an EMP hits this object. Triggers the callbacks set-up in the editor.
    /// </summary>
    public void OnEMP()
    {
        on_EMP.Invoke();
    }

    /// <summary>
    /// Placeholder callback provided for testing purposes.
    /// </summary>
    public void Test()
    {
        #if UNITY_EDITOR
        Debug.Log( "EMP hit " + this.gameObject );
        #endif
    }
}
