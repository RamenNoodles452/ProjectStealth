using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// The IInputManager class provides a common interface for mapping input messages to action messages.
/// Subclasses may inherit this class an invoke action events according to need
/// </summary>
public abstract class IInputManager : MonoBehaviour
{

    #region Events

    //I think the unityevents are better for something decoupled like a pause button. I don't think they should be used for things like, move/jump/etc
    public UnityEvent TogglePause;

    #endregion Events
    public bool InputOverride;

    public float HorizontalAxis;
    public float VerticalAxis;

    //this instance var isn't used but it might come in handy one day
    public bool HorizontalAxisInstance;
    protected bool horizontalInstanceCheck;

    //public bool HorizontalInput;
    //public bool VerticalInput;

    public bool JumpInput;
    public bool JumpInputInst;

    public bool EvadeInput;
    public bool EvadeInputInst;

    protected float RunAxis;
    public bool RunInput;
    public bool RunInputDownInst;
    public bool RunInputUpInst;

    public bool InteractInput;
    public bool InteractInputInst;
}
