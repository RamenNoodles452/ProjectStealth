using UnityEngine;
using System.Collections;

public class Player : SimpleCharacterCore
{
	[HideInInspector]
	public int Health;
	public bool AquiredMagGrip;

	private CharacterStatus Status;

	public bool grabbingWall; // TODO: private

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
                if (touchingGrabSurface && !OnTheGround && !grabbingWall)
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
                            if (FacingDirection == -1)
                                predictionCast = Physics2D.Raycast(predictionCastOrigin, Vector2.left, Mathf.Infinity, CollisionMasks.AllCollisionMask);
                            else
                                predictionCast = Physics2D.Raycast(predictionCastOrigin, Vector2.right, Mathf.Infinity, CollisionMasks.AllCollisionMask);

                            if (predictionCast.collider == grabCollider)
                                transform.Translate(0.0f, -(characterCollider.bounds.max.y - grabCollider.bounds.max.y), 0.0f);
                        }

                        if (grabCollider.bounds.max.y >= characterCollider.bounds.max.y)
                        {
                            grabbingWall = true;
                            currentMoveState = moveState.isSneaking;
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
		if (!grabbingWall)
			base.FixedUpdate ();
		else 
		{
            ClimbHorizontalVelocity();
            ClimbVerticalVelocity();
			Collisions();
            ClimbVerticalEdgeDetect();

			//move the character after all calculations have been done
			transform.Translate(Velocity);
		}
	}

    void ClimbMovementInput()
    {
        /*
        // Jump logic. Keep the Y velocity constant while holding jump for the duration of JUMP_CONTROL_TIME
        if ((jumpGracePeriod || OnTheGround) && InputManager.JumpInputInst)
        {
            isJumping = true;
            jumpGracePeriod = false;
            jumpInputTime = 0.0f;
            jumpTurned = false;
        }
        */
        /*
        // check if you're looking over the ledge
        if (againstTheLedge && Mathf.Abs(InputManager.HorizontalAxis) > 0.0f)
            lookingOverLedge = true;
        else
            lookingOverLedge = false;
        */
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
