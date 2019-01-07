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
    /// <summary>
    /// ?
    /// </summary>
    public UnityEvent TogglePause { get; set; }
    #endregion Events

    #region properties
    /// <summary>
    /// If set to true, all input will be ignored.
    /// </summary>
    public bool IgnoreInput { get; set; }

    /// <summary>
    /// Amount that the analog movement stick is held left/right [-1,1]
    /// </summary>
    public float HorizontalAxis { get; set; }
    /// <summary>
    /// Amount that the analog movement stick is held up/down [-1,1]
    /// </summary>
    public float VerticalAxis { get; set; }
    /// <summary>
    /// Value of HorizontalAxis from last frame.
    /// </summary>
    public float PreviousHorizontalAxis { get; set; }
    /// <summary>
    /// Value of VerticalAxis from last frame.
    /// </summary>
    public float PreviousVerticalAxis { get; set; }
    #region aim
    /// <summary>
    /// Amount that the aim analog stick is held left/right [-1,1]
    /// </summary>
    public float HorizontalAimAxis { get; set; }
    /// <summary>
    /// Amount that the aim analog stick is held up/down [-1,1]
    /// </summary>
    public float VerticalAimAxis { get; set; }
    /// <summary>
    /// The position, in screen-space, of the position that is being aimed at.
    /// </summary>
    public Vector2 AimPosition { get; set; }
    /// <summary>
    /// If the aim mode toggle button went from unpressed to pressed this frame.
    /// </summary>
    public bool AimToggleInputInst { get; set; }
    /// <summary>
    /// If manual aim is currently on.
    /// </summary>
    public bool IsManualAimOn { get; set; }

    public enum ManualAimMode { angle, position };
    /// <summary>
    /// Controls how aim input should be treated.
    /// </summary>
    public ManualAimMode AimMode { get; set; }
    #endregion

    /// <summary>
    /// If the jump button is being held down (holding increases jump's height, to a point).
    /// </summary>
    public bool JumpInput { get; set; }
    /// <summary>
    /// If the jump button went from unpressed to pressed this frame.
    /// </summary>
    public bool JumpInputInst { get; set; }
    /// <summary>
    /// If the evade button is being held down.
    /// </summary>
    public bool EvadeInput { get; set; }
    /// <summary>
    /// If the evade button went from unpressed to pressed this frame.
    /// </summary>
    public bool EvadeInputInst { get; set; }
    /// <summary>
    /// If the attack button is being held down.
    /// </summary>
    public bool AttackInput { get; set; }
    /// <summary>
    /// If the attack button went from unpressed to pressed this frame.
    /// </summary>
    public bool AttackInputInst { get; set; }
    /// <summary>
    /// If the shoot button is being held down. (Holding charges up the charge shot.)
    /// </summary>
    public bool ShootInput { get; set; }
    /// <summary>
    /// If the shoot button went from unpressed to pressed this frame.
    /// </summary>
    public bool ShootInputInst { get; set; }
    /// <summary>
    /// If the shoot button went from pressed to unpressed this frame. (actually fires)
    /// </summary>
    public bool ShootInputReleaseInst { get; set; }
    /// <summary>
    /// If the adrenaline surge button went from unpressed to pressed this frame.
    /// </summary>
    public bool AdrenalineInputInst { get; set; }
    /// <summary>
    /// If the interact button is being held down.
    /// </summary>
    public bool InteractInput { get; set; }
    /// <summary>
    /// If the interact button went from unpressed to pressed this frame.
    /// </summary>
    public bool InteractInputInst { get; set; }

    /// <summary>
    /// If the run button is being held down (how much?)
    /// </summary>
    protected float RunAxis { get; set; }
    /// <summary>
    /// If the run button is being held down.
    /// </summary>
    public bool RunInput { get; set; }
    /// <summary>
    /// If the run button went from unpressed to pressed this frame.
    /// </summary>
    public bool RunInputDownInst { get; set; }
    /// <summary>
    /// If the run button went from pressed to unpressed this frame.
    /// </summary>
    public bool RunInputUpInst { get; set; }

    // TODO: Remove unused stuff.
    /// <summary>
    /// If the assassinate button is being held down. (TODO: remove?)
    /// </summary>
    public bool AssassinateInput { get; set; }
    /// <summary>
    /// If the assassinate button went from unpressed to pressed this frame. (TODO: remove?)
    /// </summary>
    public bool AssassinateInputInst { get; set; }
    /// <summary>
    /// If the cloak button is being held down. (TODO: remove?)
    /// </summary>
    public bool CloakInput { get; set; }
    /// <summary>
    /// If the cloak button went from unpressed to pressed this frame. (TODO: remove?)
    /// </summary>
    public bool CloakInputInst { get; set; }
    /// <summary>
    /// If the "down button" went from unpressed to pressed this frame.
    /// </summary>
    public bool CrouchInputInst { get; set; }
    /// <summary>
    /// If the "up button" went from unpressed to pressed this frame. I know uncrouch isn't a word, but it's brief and clear.
    /// </summary>
    public bool UnCrouchInputInst { get; set; }
    #endregion
}
