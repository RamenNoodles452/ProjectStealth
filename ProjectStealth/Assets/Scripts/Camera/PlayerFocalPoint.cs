using UnityEngine;
using System.Collections;

// sets the focal point for the camera
public class PlayerFocalPoint : MonoBehaviour
{
    private CharacterStats charStats;
    private int DEFAULT_X = 40;
    private int DEFAULT_Y = 20;
    private int depth = -10;
    private Vector3 right_position;
    private Vector3 left_position;
    private float focal_point_slider;
    private bool focal_point_lock;

    // Use this for initialization
    void Start()
    {
        charStats = GetComponentInParent<CharacterStats>();
        right_position = new Vector3(  DEFAULT_X, DEFAULT_Y, depth );
        left_position  = new Vector3( -DEFAULT_X, DEFAULT_Y, depth );
        focal_point_slider = 1.0f;
        focal_point_lock = true;
    }

    // Update is called once per frame
    void Update()
    {
        if ( focal_point_lock == true )
        {
            if ( charStats.current_master_state == CharEnums.MasterState.DefaultState && charStats.IsFacingRight() ||
                 charStats.current_master_state == CharEnums.MasterState.ClimbState && charStats.IsFacingLeft() )
            {
                if ( focal_point_slider < 1.0f )
                {
                    focal_point_slider += Time.deltaTime * 2.5f;
                }
            }
            else if ( charStats.current_master_state == CharEnums.MasterState.DefaultState && charStats.IsFacingLeft() ||
                      charStats.current_master_state == CharEnums.MasterState.ClimbState && charStats.IsFacingRight() )
            {
                if ( focal_point_slider > 0.0f )
                {
                    focal_point_slider -= Time.deltaTime * 2.5f;
                }
            }
            transform.localPosition = Vector3.Lerp( left_position, right_position, focal_point_slider );
        }

    }

    // stops the FocalPoint from following the player
    void UnlockFocalPoint()
    {
        focal_point_lock = false;
    }

    // sets the flocal point to follow the player
    void LockFlocalPoint()
    {
        focal_point_lock = true;
    }

    // moves the focal point to a specific location respect to the player
    void SetFocalPoint( int x, int y )
    {
        right_position = new Vector3(  x, y, depth );
        left_position  = new Vector3( -x, y, depth );
    }
}
