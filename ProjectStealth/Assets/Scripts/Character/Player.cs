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
		
		base.Update ();

        // check for wall grab!
        if (AquiredMagGrip)
        {
            if (touchingGrabSurface && !OnTheGround && !grabbingWall)
            {
                // only grab the wall if we aren't popping out under it
                if (grabCollider.bounds.min.y < characterCollider.bounds.min.y)
                {
                    grabbingWall = true;

                    // if character is a bit too above the ledge, bump them down till they're directly under it
                    if (grabCollider.bounds.max.y < characterCollider.bounds.max.y)
                    {
                        // check to see if the wall we're gonna be offsetting against is too short.
                        RaycastHit2D predictionCast;
                        float offsetDistance = characterCollider.bounds.max.y - grabCollider.bounds.max.y;
                        Vector2 predictionCastOrigin = new Vector2(characterCollider.bounds.center.x, characterCollider.bounds.min.y - offsetDistance);
                        if (FacingDirection == -1)
                        {
                            predictionCast = Physics2D.Raycast(predictionCastOrigin, Vector2.left, Mathf.Infinity, CollisionMasks.AllCollisionMask);
                        }
                        else
                        {
                            predictionCast = Physics2D.Raycast(predictionCastOrigin, Vector2.right, Mathf.Infinity, CollisionMasks.AllCollisionMask);
                        }

                        if (predictionCast.collider == grabCollider)
                        {
                            transform.Translate(0.0f, -(characterCollider.bounds.max.y - grabCollider.bounds.max.y), 0.0f);
                            Debug.Break();
                        }
                    }
                }



            }
        }
	}

	public override void FixedUpdate ()
	{
		base.FixedUpdate ();
	}

	/*
	public void Run(float horizontal, float Vertical)
    {
        
        if (FacingDirection == 1 && horizontal > 0)
        {
            FacingDirection = -1;
            Anim.SetBool("TurnAround", true);
        }
        else if (FacingDirection == -1 && horizontal < 0)
        {
            FacingDirection = 1;
            Anim.SetBool("TurnAround", true);
        }
    }

    public void SetStopAnim(string stop)
    {
        if(stop == "false")
            Anim.SetBool("Stop", false);
        else if(stop == "true")
            Anim.SetBool("Stop", true);
    }

    public void SetTurningAnim()
    {
        Anim.SetBool("TurnAround", false);
    }

    /*
    public void Jump()
    {
        IsJumping = true;
    }
    */
}
