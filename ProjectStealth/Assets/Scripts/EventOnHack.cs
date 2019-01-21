using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Triggers specified code when this object is hacked.
/// </summary>
public class EventOnHack : MonoBehaviour
{
    #region vars
    [SerializeField]
    private UnityEvent on_hack; // Set up callbacks in the editor. NEVER, EVER use EventOnHack.OnHack!
    #endregion

    /// <summary>
    /// Use this for early initialization.
    /// </summary>
    private void Awake()
    {
        if ( on_hack.GetPersistentEventCount() <= 0 )
        {
            #if UNITY_EDITOR
            Debug.LogError( this.gameObject + " has an EventOnHack script, but the event list is empty. Removing EventOnHack." );
            #endif
            Destroy( this );
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// Hook to call when this object is successfully hacked. Triggers the callbacks set up in the editor.
    /// </summary>
    public void OnHack()
    {
        on_hack.Invoke();
    }

    /// <summary>
    /// Placeholder callback provided for testing purposes.
    /// </summary>
    public void Test()
    {
        #if UNITY_EDITOR
        Debug.Log( "Hacked " + this.gameObject );
        #endif
    }
}
