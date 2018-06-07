using UnityEngine;
using System.Collections;

// Reads in character state data from char_stats, and sets/executes animator triggers accordingly.
public class CharacterAnimationLogic : MonoBehaviour
{
	#region vars
    public Animator animator;
    protected CharacterStats char_stats;
	#endregion

    // Use this for initialization
    void Start ()
    {
        animator   = GetComponent<Animator>();
        char_stats = GetComponent<CharacterStats>();
    }

    // Update is called once per frame
    void Update ()
    {
		animator.SetBool("jumping", char_stats.IsInMidair);
        CoverLogic();
        SneakingLogic();
        CrouchLogic();
	}

    private void SneakingLogic()
    {
		if (char_stats.IsGrounded)
        {
            
			if (char_stats.current_move_state == CharEnums.MoveState.IsSneaking)
            {
                if (char_stats.velocity.x != 0.0f)
                {
					animator.SetBool("sneaking", true);
                }
                else
                {
					animator.SetBool("sneaking", false);
                }
            }
        }
    }

    private void CoverLogic()
    {
		if (char_stats.is_taking_cover == false) 
		{
			animator.SetBool ("taking_cover", false);
		}
        else
		{
			animator.SetBool("taking_cover", true);
		}
    }

    private void CrouchLogic()
    {
		if (char_stats.is_crouching == false) 
		{
			animator.SetBool ("crouching", false);
		}
        else
		{
			animator.SetBool("crouching", true);
		}
    }

    public void JumpTrigger()
    {
		animator.SetTrigger("jump_ascend");
    }

    public void FallTrigger()
    {
		animator.SetTrigger("jump_descend");
    }

    public void WallClimbTrigger()
    {
		animator.SetTrigger("wall_grab_trigger");
    }

    // TODO: hook up wall climb anim
    public void WallAscend()
    {
        if (char_stats.current_master_state == CharEnums.MasterState.ClimbState && char_stats.velocity.y > 0)
        {
			animator.SetBool("wall_ascend", true);
        }
        else
        {
			animator.SetBool("wall_ascend", false);
        }
    }
    //TODO: make a wall slide animation
    public void WallDescend()
    {

    }

    //triggers when a character dropps from a wall by moving down to the end of the wall and pressing down + jump
    public void DropFromWallTrigger()
    {
		animator.SetTrigger("drop_from_wall");
    }

    //triggers when a character climbs up from a wall
    public void WallClimbUpTrigger()
    {
		animator.SetTrigger("wall_to_ground");
    }
}

