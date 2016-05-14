using UnityEngine;
using System.Collections;

public class MagGripUpgrade : MonoBehaviour 
{
    private SpriteRenderer spriteRenderer;

	//this allows us to reference player stuff like their movement state
	//Player player_script; AVOID CIRCULAR REFERENCING
	PlayerStats player_stats;
    CharacterStats char_stats;

	private const float WALL_GRAB_DELAY = 0.15f;
	private float wallGrabDelayTimer = 0.15f;

    //Mag Grip variables
    private Collider2D grabCollider = null;
    private enum ClimbState { notClimb, wallClimb, ceilingClimb };
    private ClimbState currentClimbState = ClimbState.notClimb;
    private ClimbState transitioningToState = ClimbState.notClimb; // used when bzr curving to a new climb state


	// Use this for initialization
	void Start () 
	{
		//player_script = GetComponent<Player> ();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		player_stats = GetComponent<PlayerStats>();
        char_stats = GetComponent<CharacterStats>();
	}
	
	// Update is called once per frame
	void Update () 
	{
        //wall grab delay timer
        if (wallGrabDelayTimer < WALL_GRAB_DELAY)
            wallGrabDelayTimer = wallGrabDelayTimer + Time.deltaTime * TimeScale.timeScale;
        else
            wallGrabDelayTimer = WALL_GRAB_DELAY;
        
        if (char_stats.CurrentMasterState == CharacterStats.MasterState.climbState) 
		{
			

		}
	}

    public void InitiateWallGrab(GameObject collisionObject)
    {
        if (player_stats.AquiredMagGrip)
        {
            if (currentClimbState == ClimbState.notClimb)
            {
                grabCollider = collisionObject.GetComponent<Collider2D>();
                //TODO: check to make sure the wall is climbable (walls should have a component with some public vars. if the object is climbable should be one of the properties

                if (!char_stats.OnTheGround && currentClimbState == ClimbState.notClimb && wallGrabDelayTimer == WALL_GRAB_DELAY)
                {
                    // only grab the wall if we aren't popping out under it or over it
                    if (grabCollider.bounds.min.y <= char_stats.CharCollider.bounds.min.y)
                    {
                        // if character is a bit too above the ledge, bump them down till they're directly under it
                        if (grabCollider.bounds.max.y < char_stats.CharCollider.bounds.max.y)
                        {
                            // check to see if the wall we're gonna be offsetting against is too short.
                            RaycastHit2D predictionCast;
                            float offsetDistance = char_stats.CharCollider.bounds.max.y - grabCollider.bounds.max.y;
                            Vector2 predictionCastOrigin = new Vector2(char_stats.CharCollider.bounds.center.x, char_stats.CharCollider.bounds.min.y - offsetDistance);
                            if (grabCollider.bounds.center.x < char_stats.CharCollider.bounds.center.x)
                                predictionCast = Physics2D.Raycast(predictionCastOrigin, Vector2.left, Mathf.Infinity, CollisionMasks.AllCollisionMask);
                            else
                                predictionCast = Physics2D.Raycast(predictionCastOrigin, Vector2.right, Mathf.Infinity, CollisionMasks.AllCollisionMask);

                            if (predictionCast.collider == grabCollider)
                                transform.Translate(0.0f, -(char_stats.CharCollider.bounds.max.y - grabCollider.bounds.max.y), 0.0f);
                        }
                        // if we're good to grab, get everything in order
                        if (grabCollider.bounds.max.y >= char_stats.CharCollider.bounds.max.y)
                        {
                            char_stats.ResetJump();
                            currentClimbState = ClimbState.wallClimb;
                            char_stats.CurrentMasterState = CharacterStats.MasterState.climbState;
                            // currentMoveState = moveState.isSneaking; potentially not necessary

                            // variable sets to prevent weird turning when grabbing onto a wall
                            // if the wall is to our left
                            if (grabCollider.bounds.center.x < char_stats.CharCollider.bounds.center.x)
                            {
                                char_stats.FacingDirection = -1;
                                spriteRenderer.flipX = true;
                            }
                            // if the wall is to our right
                            else
                            {
                                char_stats.FacingDirection = 1;
                                spriteRenderer.flipX = false;
                            }
                            char_stats.Velocity.x = 0.0f;
                        }
                    }
                }
            }
        } 
    }
}
