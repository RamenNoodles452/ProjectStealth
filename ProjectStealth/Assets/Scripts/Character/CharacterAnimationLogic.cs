using UnityEngine;
using System.Collections;

// Reads in character state data from char_stats, and sets/executes animator triggers accordingly.
public class CharacterAnimationLogic : MonoBehaviour
{
    public Animator anim;
    protected CharacterStats char_stats;

    // Use this for initialization
    void Start ()
    {
        anim = GetComponent<Animator>();
        char_stats = GetComponent<CharacterStats>();
    }

    // Update is called once per frame
    void Update ()
    {
		anim.SetBool("jumping", char_stats.IsInMidair);
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
                    anim.SetBool("sneaking", true);
                }
                else
                {
                    anim.SetBool("sneaking", false);
                }
            }
        }
    }

    private void CoverLogic()
    {
		if (char_stats.is_taking_cover == false) 
		{
			anim.SetBool ("taking_cover", false);
		}
        else
		{
            anim.SetBool("taking_cover", true);
		}
    }

    private void CrouchLogic()
    {
		if (char_stats.is_crouching == false) 
		{
			anim.SetBool ("crouching", false);
		}
        else
		{
            anim.SetBool("crouching", true);
		}
    }

    public void JumpTrigger()
    {
        anim.SetTrigger("jump_ascend");
    }

    public void FallTrigger()
    {
        anim.SetTrigger("jump_descend");
    }
}

