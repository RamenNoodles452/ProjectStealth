using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    #region vars
    [SerializeField]
    private Transform focal_target;
    [SerializeField]
    private float damp_time = 0.15f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 accumulated_position;   // accumulate fractional x, y coordinates. camera position will always be whole number.
    private Camera cam;
    private BoxCollider2D boundingBox;
    #endregion

    private void Awake()
    {
        focal_target = GameObject.Find( "PlayerCharacter/CameraFocalPoint" ).transform;
    }

    // Use this for initialization
    void Start()
    {
        cam = GetComponent<Camera>();

        boundingBox = GameObject.Find( "BoundingBox" ).GetComponent<BoxCollider2D>(); //bad
        accumulated_position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = focal_target.position;
        if ( focal_target )
        {
            #region clamp
            Vector3 clamped_focal_target = focal_target.position;

            if ( !boundingBox )
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

            //smoothly move the camera to the focal point
            Vector3 point = cam.WorldToViewportPoint( clamped_focal_target );
            Vector3 delta = clamped_focal_target - cam.ViewportToWorldPoint( new Vector3( 0.5f, 0.5f, point.z ) );
            Vector3 destination = transform.position + delta;
            accumulated_position = Vector3.SmoothDamp( accumulated_position, destination, ref velocity, damp_time );
            // Disabled. Making it less smooth looks bad. We should either go all-in on low res before enabling this, or don't do it.
            transform.position = new Vector3( (int) accumulated_position.x, (int) accumulated_position.y, accumulated_position.z );
            //transform.position = accumulated_position;
        }

    }

    /// <summary>
    /// Instantly centers the camera on the focus target
    /// </summary>
    public void SnapToFocalPoint()
    {
        accumulated_position = new Vector3( (int) focal_target.position.x, (int) focal_target.position.y, focal_target.position.z );
        transform.position = accumulated_position;
    }
}
