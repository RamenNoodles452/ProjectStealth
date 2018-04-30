using UnityEngine;
using System.Collections;

// sets the focal point for the camera
public class PlayerFocalPoint: MonoBehaviour
{
    private CharacterStats charStats;
    private int x = 40;
    private int y = 20;
    private int depth = -10;
    private Vector3 right_position;
    private Vector3 left_position;
    private float focal_point_slider;

	// Use this for initialization
	void Start ()
    {
        charStats = GetComponentInParent<CharacterStats>();
        right_position = new Vector3(x, y, depth);
        left_position = new Vector3(-x, y, depth);
        focal_point_slider = 1.0f;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (charStats.current_master_state == CharEnums.MasterState.DefaultState && charStats.facing_direction == CharEnums.FacingDirection.Right ||
			charStats.current_master_state == CharEnums.MasterState.ClimbState && charStats.facing_direction == CharEnums.FacingDirection.Left)
        {
			if (focal_point_slider < 1.0f)
            {
				focal_point_slider += Time.deltaTime * 2.5f;
            }
        }
		else if (charStats.current_master_state == CharEnums.MasterState.DefaultState && charStats.facing_direction == CharEnums.FacingDirection.Left || 
			     charStats.current_master_state == CharEnums.MasterState.ClimbState && charStats.facing_direction == CharEnums.FacingDirection.Right)
        {
			if (focal_point_slider > 0.0f)
            {
				focal_point_slider -= Time.deltaTime * 2.5f;
            }
        }

		transform.localPosition = Vector3.Lerp(left_position, right_position, focal_point_slider);
	}
}
