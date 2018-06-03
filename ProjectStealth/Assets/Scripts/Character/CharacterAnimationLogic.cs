using UnityEngine;
using System.Collections;

public class CharacterAnimationLogic : MonoBehaviour
{
    public Animator Anim;
    protected CharacterStats charStats;

    // Use this for initialization
    void Start ()
    {
        Anim = GetComponent<Animator>();
        charStats = GetComponent<CharacterStats>();
    }

    // Update is called once per frame
    void Update ()
    {
        Anim.SetBool("jumping", !charStats.OnTheGround);
        CoverLogic();
        SneakingLogic();
        CrouchLogic();
        WallClimb();
        WallSlide();

    }

    void SneakingLogic()
    {
        if (charStats.OnTheGround == true)
        {
            
            if (charStats.CurrentMoveState == CharEnums.MoveState.isSneaking)
            {
                if (charStats.Velocity.x != 0.0f)
                {
                    Anim.SetBool("sneaking", true);
                }
                else
                {
                    Anim.SetBool("sneaking", false);
                }
            }
        }
    }

    void CoverLogic()
    {
        if (charStats.IsTakingCover == false)
            Anim.SetBool("taking_cover", false);
        else
            Anim.SetBool("taking_cover", true);
    }

    void CrouchLogic()
    {
        if (charStats.IsCrouching == false)
            Anim.SetBool("crouching", false);
        else
            Anim.SetBool("crouching", true);
    }

    // Triggers are called within the character scripts
    public void JumpTrigger()
    {
        Anim.SetTrigger("jump_ascend");
    }

    // Triggers are called within the character scripts
    public void FallTrigger()
    {
        Anim.SetTrigger("jump_descend");
    }

    // Triggers are called within the character scripts
    public void WallGrabTrigger()
    {
        Anim.SetTrigger("wall_grab_trigger");
    }

    public void WallClimb()
    {
        if (charStats.CurrentMasterState == CharEnums.MasterState.climbState && charStats.Velocity.y > 0)
        {
            Anim.SetBool("wall_climb", true);
        }
        else
        {
            Anim.SetBool("wall_climb", false);
        }
    }
    //TODO: make a wall slide animation
    public void WallSlide()
    {
        if (charStats.CurrentMasterState == CharEnums.MasterState.climbState && charStats.Velocity.y < 0)
        {
            Anim.SetBool("wall_slide", true);
        }
        else
        {
            Anim.SetBool("wall_slide", false);
        }
    }

    //triggers when a character dropps from a wall by moving down to the end of the wall and pressing down + jump
    public void DropFromWallTrigger()
    {
       Anim.SetTrigger("drop_from_wall");
    }

    //triggers when a character climbs up from a wall
    public void WallClimbUpTrigger()
    {
        Anim.SetTrigger("wall_to_ground");
    }
}

