using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    public float damp_time = 0.15f;
    private Vector3 velocity = Vector3.zero;
    public Transform focal_target;
    private Camera cam;

    private BoxCollider2D boundingBox;

    private void Awake()
    {
        focal_target = GameObject.Find("CameraFocalPoint").transform;
    }

    // Use this for initialization
    void Start ()
    {
        Screen.SetResolution(640, 480, false, 60);
        cam = GetComponent<Camera>();

        boundingBox = GameObject.Find("BoundingBox").GetComponent<BoxCollider2D>(); //bad
    }

    // Update is called once per frame
    void Update ()
    {
        //transform.position = focal_target.position;

        if (focal_target)
        {
            #region clamp
            Vector3 clamped_focal_target = focal_target.position;

            if (!boundingBox)
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
                if (focal_target.position.x + half_width_in_world > boundingBox.gameObject.transform.position.x + boundingBox.offset.x + boundingBox.size.x / 2.0f)
                {
                    // don't go too far right
					clamped_focal_target.x = (boundingBox.gameObject.transform.position.x + boundingBox.offset.x + boundingBox.size.x / 2.0f) - half_width_in_world;
                }
                else if (focal_target.position.x - half_width_in_world < boundingBox.gameObject.transform.position.x + boundingBox.offset.x - boundingBox.size.x / 2.0f)
                {
                    // don't go too far left
					clamped_focal_target.x = (boundingBox.gameObject.transform.position.x + boundingBox.offset.x - boundingBox.size.x / 2.0f) + half_width_in_world;
                }

                if (focal_target.position.y + half_height_in_world > boundingBox.gameObject.transform.position.y + boundingBox.offset.y + boundingBox.size.y / 2.0f)
                {
                    // don't go too far up/down
					clamped_focal_target.y = (boundingBox.gameObject.transform.position.y + boundingBox.offset.y + boundingBox.size.y / 2.0f) - half_height_in_world;
                }
                else if (focal_target.position.y - half_height_in_world < boundingBox.gameObject.transform.position.y + boundingBox.offset.y - boundingBox.size.y / 2.0f)
                {
                    // don't go too far up/down
					clamped_focal_target.y = (boundingBox.gameObject.transform.position.y + boundingBox.offset.y - boundingBox.size.y / 2.0f) + half_height_in_world;
                }
            }
            #endregion

			Vector3 point = cam.WorldToViewportPoint( clamped_focal_target );
			Vector3 delta = clamped_focal_target - cam.ViewportToWorldPoint( new Vector3( 0.5f, 0.5f, point.z ) );
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp( transform.position, destination, ref velocity, damp_time );
        }
        
    }

    /// <summary>
    /// Instantly centers the camera on the focus target
    /// </summary>
    public void SnapToFocalPoint()
    {
        transform.position = focal_target.position;
    }
}
