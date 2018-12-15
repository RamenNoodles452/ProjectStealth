using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    #region vars
    [SerializeField]
    private Transform focal_target;

    private Vector3 accumulated_position;   // accumulate fractional x, y coordinates. camera position must always be whole number.
    private Camera cam;
    private BoxCollider2D boundingBox;

    // Camera config
    private const float CAMERA_Z = -10.0f;
    private const float MAX_VELOCITY = 640.0f; // pixels / second
    #endregion

    // Early initialization (references)
    private void Awake()
    {
        focal_target = GameObject.Find( "PlayerCharacter/CameraFocalPoint" ).transform; // slow
        boundingBox = GameObject.Find( "BoundingBox" ).GetComponent<BoxCollider2D>();   // slow
        cam = GetComponent<Camera>();
    }

    // Use this for initialization
    void Start()
    {
        transform.position = new Vector3( transform.position.x, transform.position.y, CAMERA_Z ); // lock z
        accumulated_position = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Moved to lateupdate so camera hits the target after motion is applied to the player.
        //BASICALLY: transform.position = focal_target.position;

        if ( focal_target )
        {
            #region clamp
            Vector3 clamped_focal_target = focal_target.position;

            if ( ! boundingBox )
            {
                Debug.LogError( "Someone forgot to put a bounding box around the level / messed it up." );
            }
            else
            {
                // Clamping math
                Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
                Vector3 bottom_right = cam.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, 0.0f));
                float half_width_in_world = bottom_right.x - center.x;
                float half_height_in_world = bottom_right.y - center.y;

                // Implementation ASSUMES a level will never be smaller than 1 screen
                if ( focal_target.position.x + half_width_in_world > boundingBox.gameObject.transform.position.x + boundingBox.offset.x + boundingBox.size.x / 2.0f )
                {
                    // don't go too far right
                    clamped_focal_target.x = ( boundingBox.gameObject.transform.position.x + boundingBox.offset.x + boundingBox.size.x / 2.0f ) - half_width_in_world;
                }
                else if ( focal_target.position.x - half_width_in_world < boundingBox.gameObject.transform.position.x + boundingBox.offset.x - boundingBox.size.x / 2.0f )
                {
                    // don't go too far left
                    clamped_focal_target.x = ( boundingBox.gameObject.transform.position.x + boundingBox.offset.x - boundingBox.size.x / 2.0f ) + half_width_in_world;
                }

                if ( focal_target.position.y + half_height_in_world > boundingBox.gameObject.transform.position.y + boundingBox.offset.y + boundingBox.size.y / 2.0f )
                {
                    // don't go too far up/down
                    clamped_focal_target.y = ( boundingBox.gameObject.transform.position.y + boundingBox.offset.y + boundingBox.size.y / 2.0f ) - half_height_in_world;
                }
                else if ( focal_target.position.y - half_height_in_world < boundingBox.gameObject.transform.position.y + boundingBox.offset.y - boundingBox.size.y / 2.0f )
                {
                    // don't go too far up/down
                    clamped_focal_target.y = ( boundingBox.gameObject.transform.position.y + boundingBox.offset.y - boundingBox.size.y / 2.0f ) + half_height_in_world;
                }
            }
            #endregion

            // smoothly move the camera to the focal point
            Vector3 point = cam.WorldToViewportPoint( clamped_focal_target );
            Vector3 screen_center = cam.ViewportToWorldPoint( new Vector3( 0.5f, 0.5f, point.z ) );
            Vector3 delta = clamped_focal_target - screen_center;
            Vector3 destination = transform.position + delta;

            if ( delta.magnitude <= ( MAX_VELOCITY * Time.deltaTime * Time.timeScale ) ) // arrive without overshooting
            {
                accumulated_position = new Vector3( destination.x, destination.y, CAMERA_Z );
                transform.position = new Vector3( (int) destination.x, (int) destination.y, CAMERA_Z );
            }
            else // just move.
            {
                accumulated_position += new Vector3( delta.normalized.x, delta.normalized.y, 0.0f ) * MAX_VELOCITY * Time.deltaTime * Time.timeScale;
                transform.position = new Vector3( (int) ( accumulated_position.x ), (int) ( accumulated_position.y ) , CAMERA_Z );
            }
        }

    }

    /// <summary>
    /// Instantly centers the camera on the focus target
    /// </summary>
    public void SnapToFocalPoint()
    {
        accumulated_position = new Vector3( (int) focal_target.position.x, (int) focal_target.position.y, CAMERA_Z );
        transform.position = accumulated_position;
    }
}
