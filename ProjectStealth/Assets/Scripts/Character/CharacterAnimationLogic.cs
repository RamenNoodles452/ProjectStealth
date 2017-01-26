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
        AnimationTriggers();

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

    void AnimationTriggers()
    {
        CoverTrigger();
    }

    void CoverTrigger()
    {
        if (charStats.IsTakingCover == false)
            Anim.SetBool("taking_cover", false);
        else
            Anim.SetBool("taking_cover", true);
    }
}

