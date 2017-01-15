using UnityEngine;
using System.Collections;

public class PlayerFocalPoint: MonoBehaviour
{
    private CharacterStats charStats;
    private int hPos = 40;
    private int vPos = 20;
    private int depth = -10;
    private Vector3 rightPos;
    private Vector3 leftPos;
    private float focalPointSlider;

	// Use this for initialization
	void Start ()
    {
        charStats = GetComponentInParent<CharacterStats>();
        rightPos = new Vector3(hPos, vPos, depth);
        leftPos = new Vector3(-hPos, vPos, depth);
        focalPointSlider = 1.0f;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (charStats.CurrentMasterState == CharEnums.MasterState.defaultState && charStats.FacingDirection == 1 ||
            charStats.CurrentMasterState == CharEnums.MasterState.climbState && charStats.FacingDirection == -1)
        {
            if (focalPointSlider < 1.0f)
            {
                focalPointSlider += Time.deltaTime * 2.5f;
            }
        }
        else if (charStats.CurrentMasterState == CharEnums.MasterState.defaultState && charStats.FacingDirection == -1 || 
            charStats.CurrentMasterState == CharEnums.MasterState.climbState && charStats.FacingDirection == 1)
        {
            if (focalPointSlider > 0.0f)
            {
                focalPointSlider -= Time.deltaTime * 2.5f;
            }
        }

        transform.localPosition = Vector3.Lerp(leftPos, rightPos, focalPointSlider);
	}
}
