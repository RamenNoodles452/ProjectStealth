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
	    if (charStats.OnTheGround == true)
        {
            if (charStats.currentMoveState == CharEnums.MoveState.isSneaking)
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
}

