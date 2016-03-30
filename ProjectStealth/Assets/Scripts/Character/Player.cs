using UnityEngine;
using System.Collections;
using System;

public class Player : SimpleCharacterCore
{
	[HideInInspector]
	public int Health;
	public bool AquiredMagGrip;

	private CharacterStatus Status;

	public bool ledgeClimb; // TODO: private
	private const float WALL_GRAB_DELAY = 0.15f;
	private float wallGrabDelayTimer = 0.15f;
    private const float WALL_CLIMB_SPEED = 2.0f;
    private const float WALL_SLIDE_SPEED = 3.0f;

    //Mag Grip variables
    private Collider2D grabCollider = null;
    private enum ClimbState { notClimb, wallClimb, ceilingClimb };
    private ClimbState currentClimbState = ClimbState.notClimb;
    private ClimbState transitioningToState = ClimbState.notClimb; // used when bzr curving to a new climb state

    void Awake ()
	{
		this.Status = CharacterStatus.CreateInstance<CharacterStatus> ();
	}

	public override void Start ()
	{
		base.Start ();
		if (GameState.Instance.PlayerState != null) {
			this.Status.Clone (GameState.Instance.PlayerState);
			Debug.Log ("Loading from the GameState instance");
		}

		//walk and run vars
		WALK_SPEED = 1.0f; //used for cutscenes with Alice
		SNEAK_SPEED = 2.0f; //Alice's default speed
		RUN_SPEED = 4.5f;
		currentMoveState = moveState.isSneaking;

		// MagGrip vars
		AquiredMagGrip = true;
	}

	void OnDestroy ()
	{
		if (GameState.Instance.PlayerState == null) {
			GameState.Instance.PlayerState = CharacterStatus.CreateInstance<CharacterStatus> ();
			Debug.Log ("Creating a new PlayerState instance in GameState");
		}
		GameState.Instance.PlayerState.Clone (this.Status);
		Debug.Log ("Storing the current player instance in GameState");
	}

	private void LoadInstance ()
	{

	}

	private void StoreInstance ()
	{

	}

	public override void Update ()
	{
		//wall grab delay timer
        if (wallGrabDelayTimer < WALL_GRAB_DELAY)
            wallGrabDelayTimer = wallGrabDelayTimer + Time.deltaTime * TimeScale.timeScale;
        else
            wallGrabDelayTimer = WALL_GRAB_DELAY;

        if (currentClimbState == ClimbState.wallClimb)
        {
            ClimbMovementInput();
        }
        else if (currentClimbState == ClimbState.ceilingClimb)
        {
        }
        else if (currentClimbState == ClimbState.notClimb) // if not in any special movement state
        {
            base.Update();

            // base mag grip checks
            if (AquiredMagGrip)
            {
                // do we want to climb down?
                WallClimbFromLedge();
            }
        }
	}

    public override void LateUpdate()
    {
        base.LateUpdate();
    }

	public override void FixedUpdate()
	{
        if (ledgeClimb)
        {
            transform.position = BezierCurveMovement(bzrDistance, bzrStartPosition, bzrEndPosition, bzrCurvePosition);

            if (bzrDistance < 1.0f)
                bzrDistance = bzrDistance + Time.deltaTime * TimeScale.timeScale * 5;
            else
            {
                ledgeClimb = false;
                currentClimbState = transitioningToState;
                InputManager.InputOverride = false;
                //UnityEditor.EditorApplication.isPaused = true;
            }
        }
        else if (currentClimbState == ClimbState.wallClimb)
        {
            ClimbHorizontalVelocity();
            ClimbVerticalVelocity();
            Collisions();
            ClimbVerticalEdgeDetect();

            //move the character after all calculations have been done
            transform.Translate(Velocity);

            //if you climb down and touch the ground, stop climbing
            if (OnTheGround)
            {
                StopClimbing();
            }
        }
        else if (currentClimbState == ClimbState.ceilingClimb)
        {

        }
		else
			base.FixedUpdate ();
	}

    void ClimbMovementInput()
    {
		LookingOverLedge();

        if (grabCollider)
        {
            // Jump logic.
            if (!lookingOverLedge && InputManager.JumpInputInst) {
                StopClimbing();
                isJumping = true;
                jumpInputTime = 0.0f;
                FacingDirection = -FacingDirection;
                if (FacingDirection == 1)
                    characterAccel = JUMP_ACCEL;
                else
                    characterAccel = -JUMP_ACCEL;

                HorizontalJumpVelNoAccel((JUMP_HORIZONTAL_SPEED_MIN + JUMP_HORIZONTAL_SPEED_MAX) / 2.0f);
                SetFacing ();

                // gotta do a second call of fixed update to make sure we move off the wall
                base.FixedUpdate ();
            } 
            //ledge climb logic
            else if (lookingOverLedge && InputManager.JumpInputInst) 
            {
                // if we're looking below
                if (grabCollider.bounds.min.y == characterCollider.bounds.min.y)
                {
                    StopClimbing();
                }
                // if we're looking above
                else if (grabCollider.bounds.max.y == characterCollider.bounds.max.y)
                {
                    SetupLedgeClimb(currentClimbState);
                }
                else
                    Debug.LogError("The grabCollider object is most likely null. This should never happen");
            }
        }
    }

    void ClimbHorizontalVelocity()
    {
        if (Mathf.Approximately(Velocity.x - 1000000, -1000000))
            Velocity.x = 0;
    }

    void ClimbVerticalVelocity()
    {
        if (currentMoveState == moveState.isSneaking)
        {
            if (InputManager.VerticalAxis > 0.0f)
                Velocity.y = WALL_CLIMB_SPEED * InputManager.VerticalAxis;
            else if (InputManager.VerticalAxis < 0.0f)
                Velocity.y = -WALL_SLIDE_SPEED;
            else
                Velocity.y = 0.0f;
        }
        else
            Debug.LogError("Character must be in SNEAK movement type when climbing");
    }

    /// <summary>
    /// sets everything that needs to be done when player stops climbing
    /// </summary>
    void StopClimbing()
    {
        currentClimbState = ClimbState.notClimb;
        //no grab, no collider
        grabCollider = null;
        //reset the delay before we can wall grab again
        wallGrabDelayTimer = 0.0f;
    }

    /// <summary>
    /// This function detects if the player is touching the ledge when climbing vertically against a wall
    /// </summary>
    void ClimbVerticalEdgeDetect()
    {
        // raycast to find the climbing object
        RaycastHit2D hit = Physics2D.Raycast(characterCollider.bounds.center, new Vector2(FacingDirection, 0), Mathf.Infinity, CollisionMasks.WallGrabMask);
        if (hit.collider != null)
        {
            float colliderTop = hit.collider.bounds.max.y;
            float colliderBottom = hit.collider.bounds.min.y;
            float characterTop = characterCollider.bounds.max.y;
            float characterBottom = characterCollider.bounds.min.y;

            // stop at the edges of the surface
            float ledgeDistanceTop = colliderTop - characterTop;
            if (Velocity.y > 0.0f && ledgeDistanceTop <= Mathf.Abs(Velocity.y))
                Velocity.y = ledgeDistanceTop;

            float ledgeDistanceBottom = characterBottom - colliderBottom;
            if (Velocity.y < 0.0f && ledgeDistanceBottom <= Mathf.Abs(Velocity.y))
                Velocity.y = -ledgeDistanceBottom;

            // set if you're against the ledge
            if (ledgeDistanceTop == 0.0f || ledgeDistanceBottom == 0.0f)
                againstTheLedge = true;
            else
                againstTheLedge = false;
        }
        else
            Debug.LogError("Can't find collider when climbing against a wall. This shouldn't happen");
    }

    void SetupLedgeClimb(ClimbState startingState, Collider2D climbObject = null)
    {
        InputManager.JumpInputInst = false;
        InputManager.InputOverride = true;
        // translate body to on the ledge
        bzrDistance = 0.0f;
        bzrStartPosition = characterCollider.bounds.center;

        // variable sterilazation
        isJumping = false;

        if (startingState == ClimbState.notClimb)
        {
            currentMoveState = moveState.isSneaking;
            if (FacingDirection == 1)
            {
                bzrEndPosition = new Vector2(climbObject.bounds.max.x + characterCollider.bounds.extents.x + 0.01f, climbObject.bounds.max.y - characterCollider.bounds.extents.y);
                bzrCurvePosition = new Vector2(characterCollider.bounds.center.x + characterCollider.bounds.extents.x * 2, characterCollider.bounds.center.y + characterCollider.bounds.size.y);
            }
            else
            {
                bzrEndPosition = new Vector2(climbObject.bounds.min.x - characterCollider.bounds.extents.x - 0.01f, climbObject.bounds.max.y - characterCollider.bounds.extents.y);
                bzrCurvePosition = new Vector2(characterCollider.bounds.center.x - characterCollider.bounds.extents.x * 2, characterCollider.bounds.center.y + characterCollider.bounds.size.y);
            }
            transitioningToState = ClimbState.wallClimb;
            FacingDirection = -FacingDirection;
            SetFacing();
        }
        else if (startingState == ClimbState.wallClimb)
        {
            if (FacingDirection == 1)
            {
                bzrEndPosition = characterCollider.bounds.center + characterCollider.bounds.size;
                bzrCurvePosition = new Vector2(characterCollider.bounds.center.x - characterCollider.bounds.extents.x, characterCollider.bounds.center.y + characterCollider.bounds.size.y * 2);
            }
            else
            {
                bzrEndPosition = new Vector2(characterCollider.bounds.center.x - characterCollider.bounds.size.x, characterCollider.bounds.center.y + characterCollider.bounds.size.y);
                bzrCurvePosition = new Vector2(characterCollider.bounds.center.x + characterCollider.bounds.extents.x, characterCollider.bounds.center.y + characterCollider.bounds.size.y * 2);
            }
            transitioningToState = ClimbState.notClimb;
        }
        ledgeClimb = true;
    }

    /// <summary>
    /// Function that initiates a wallclimb from a ledge
    /// </summary>
    void WallClimbFromLedge()
    {
        // if we want to grab down onto the wall from the ledge
        if (lookingOverLedge) // && we're standing on a grabbable surface?
        {
            if (OnTheGround && InputManager.JumpInputInst)
            {
                // This is kinda inefficient as it is redundant code from the collision detection...
                Vector2 verticalBoxSize = new Vector2(characterCollider.bounds.size.x - 0.01f, 0.01f);
                Vector2 downHitOrigin = new Vector2(characterCollider.bounds.center.x, characterCollider.bounds.center.y - characterCollider.bounds.extents.y + 0.01f);
                RaycastHit2D downHit = Physics2D.BoxCast(downHitOrigin, verticalBoxSize, 0.0f, Vector2.down, Mathf.Infinity, CollisionMasks.AllCollisionMask);
                if (downHit.collider != null)
                {
                    SetupLedgeClimb(currentClimbState, downHit.collider);

                }
            }
        }
    }

    /// <summary>
    /// at the player level, we have to take into looking over the ledge from wall climbs as well
    /// </summary>
    public override void LookingOverLedge()
    {
        if (currentClimbState == ClimbState.notClimb)
        {
            base.LookingOverLedge();
        }
        else if (currentClimbState == ClimbState.wallClimb)
        {
            if (againstTheLedge && (Mathf.Abs(InputManager.VerticalAxis) > 0.0f ||
                (InputManager.HorizontalAxis > 0.0f && characterCollider.bounds.center.x < grabCollider.bounds.center.x) ||
                (InputManager.HorizontalAxis < 0.0f && characterCollider.bounds.center.x > grabCollider.bounds.center.x)))
                lookingOverLedge = true;
            else
                lookingOverLedge = false;
        }
        else if (currentClimbState == ClimbState.ceilingClimb)
        {

        }
    }

    public override void TouchedWall(GameObject collisionObject)
    {
        if (AquiredMagGrip)
        {
            if (currentClimbState == ClimbState.notClimb)
            {
                grabCollider = collisionObject.GetComponent<Collider2D>();
                //TODO: check to make sure the wall is climbable (walls should have a component with some public vars. if the object is climbable should be one of the properties

                if (!OnTheGround && currentClimbState == ClimbState.notClimb && wallGrabDelayTimer == WALL_GRAB_DELAY)
                {
                    // only grab the wall if we aren't popping out under it or over it
                    if (grabCollider.bounds.min.y <= characterCollider.bounds.min.y)
                    {
                        // if character is a bit too above the ledge, bump them down till they're directly under it
                        if (grabCollider.bounds.max.y < characterCollider.bounds.max.y)
                        {
                            // check to see if the wall we're gonna be offsetting against is too short.
                            RaycastHit2D predictionCast;
                            float offsetDistance = characterCollider.bounds.max.y - grabCollider.bounds.max.y;
                            Vector2 predictionCastOrigin = new Vector2(characterCollider.bounds.center.x, characterCollider.bounds.min.y - offsetDistance);
                            if (grabCollider.bounds.center.x < characterCollider.bounds.center.x)
                                predictionCast = Physics2D.Raycast(predictionCastOrigin, Vector2.left, Mathf.Infinity, CollisionMasks.AllCollisionMask);
                            else
                                predictionCast = Physics2D.Raycast(predictionCastOrigin, Vector2.right, Mathf.Infinity, CollisionMasks.AllCollisionMask);

                            if (predictionCast.collider == grabCollider)
                                transform.Translate(0.0f, -(characterCollider.bounds.max.y - grabCollider.bounds.max.y), 0.0f);
                        }
                        // if we're good to grab, get everything in order
                        if (grabCollider.bounds.max.y >= characterCollider.bounds.max.y)
                        {
                            jumpTurned = false;
                            isJumping = false;
                            currentClimbState = ClimbState.wallClimb;
                            currentMoveState = moveState.isSneaking;

                            // variable sets to prevent weird turning when grabbing onto a wall
                            // if the wall is to our left
                            if (grabCollider.bounds.center.x < characterCollider.bounds.center.x)
                                FacingDirection = -1;
                            // if the wall is to our right
                            else
                                FacingDirection = 1;
                            SetFacing();
                            Velocity.x = 0.0f;
                        }
                    }
                }
            }
        } 
    }
}
