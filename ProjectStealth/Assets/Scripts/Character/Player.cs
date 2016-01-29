using UnityEngine;
using System.Collections;

public class Player : SimpleCharacterCore
{
	[HideInInspector]
	public int Health;
	public bool AquiredMagGrip;

	private CharacterStatus Status;

	public bool grabbingWall; // TODO: private
	public bool ledgeClimb; // TODO: private
	private const float WALL_GRAB_DELAY = 0.15f;
	private float wallGrabDelayTimer = 0.15f;

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
		SNEAK_SPEED = 1.5f; //Alice's default speed
		RUN_SPEED = 4.0f;
		currentMoveState = moveState.isSneaking;

		// MagGrip vars
		grabbingWall = false;

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

        if (grabbingWall)
        {
            ClimbMovementInput();
        }
        else // if not in any special movement state
        {
            base.Update();

            // check for wall grab!
            if (AquiredMagGrip)
            {
                if (touchingGrabSurface && !OnTheGround && !grabbingWall && wallGrabDelayTimer == WALL_GRAB_DELAY)
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
                            grabbingWall = true;
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

    public override void LateUpdate()
    {
        base.LateUpdate();
    }

	public override void FixedUpdate ()
	{
        if (ledgeClimb)
        {
            transform.position = BezierCurveMovement(bzrDistance, bzrStartPosition, bzrEndPosition, bzrCurvePosition);

            if (bzrDistance < 1.0f)
                bzrDistance = bzrDistance + Time.deltaTime * TimeScale.timeScale * 5;
            else
            {
                ledgeClimb = false;
                grabbingWall = false;
                InputManager.InputOverride = false;
            }
		}
		else if (grabbingWall) 
		{
			ClimbHorizontalVelocity ();
			ClimbVerticalVelocity ();
			Collisions ();
			ClimbVerticalEdgeDetect ();

			//move the character after all calculations have been done
			transform.Translate (Velocity);
		}
		else
			base.FixedUpdate ();
	}

    void ClimbMovementInput()
    {
		LookingOverLedge (InputManager.VerticalAxis);

        //special look over ledge specifically when climbing
        if (lookingOverLedge == false && againstTheLedge && 
            ((InputManager.HorizontalAxis > 0.0f && characterCollider.bounds.center.x < grabCollider.bounds.center.x) ||
             (InputManager.HorizontalAxis < 0.0f && characterCollider.bounds.center.x > grabCollider.bounds.center.x)))
            lookingOverLedge = true;

        // Jump logic.
		if (!lookingOverLedge && InputManager.JumpInputInst) {
			grabbingWall = false;
			isJumping = true;
			jumpInputTime = 0.0f;
			wallGrabDelayTimer = 0.0f;
			FacingDirection = -FacingDirection;
			if (FacingDirection == 1)
				characterAccel = ACCELERATION;
			else
				characterAccel = -ACCELERATION;

            HorizontalJumpVel (JUMP_HORIZONTAL_SPEED);
			SetFacing ();

            // gotta do a second call of fixed update to make sure we move off the wall
			base.FixedUpdate ();
		} 
        else if (lookingOverLedge && InputManager.JumpInputInst) 
        {
            // if we're looking below
            if (grabCollider.bounds.min.y == characterCollider.bounds.min.y)
            {
                grabbingWall = false;
            }
            // if we're looking above
            else if (grabCollider.bounds.max.y == characterCollider.bounds.max.y)
            {
                InputManager.JumpInputInst = false;
                InputManager.InputOverride = true;
                // translate body to on the ledge
				bzrDistance = 0.0f;
				bzrStartPosition = characterCollider.bounds.center;

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
                ledgeClimb = true;
			}
            else
                Debug.LogError("This should never happen");
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
            Velocity.y = SNEAK_SPEED * InputManager.VerticalAxis;
        else
            Debug.LogError("Character must be in SNEAK movement type when climbing");
    }

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
}
