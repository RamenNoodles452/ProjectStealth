using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// The IInputManager class provides a common interface for mapping input messages to action messages.
/// Subclasses may inherit this class an invoke action events according to need
/// </summary>
public abstract class IInputManager : MonoBehaviour {

    #region Events

    // TRICKY: For some reason, Unity does not serialize parameterized events in the editor
    // Creating a serializable version of the UnityEvent seems to fix it
    [System.Serializable]
    public class OnMove : UnityEvent<float, float> { };

    public OnMove Move;
    public UnityEvent Jump;
    public UnityEvent Attack;
    public UnityEvent Evade;
    public UnityEvent Interact;
    
    // TODO: I think the unityevents are better for something decoupled like a pause button. I don't think they should be used for things like, move/jump/etc
    public UnityEvent TogglePause;

    #endregion Events

    public float HorizontalAxis;
    public float VerticalAxis;
    public bool HorizontalInput;
    public bool VerticalInput;

    public bool JumpInput;
    public bool JumpInputInst;
}
