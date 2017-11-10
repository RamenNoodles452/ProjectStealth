using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    public float DampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    public Transform FocalTarget;
    private Camera cam;

    private BoxCollider2D boundingBox;

    private void Awake()
    {
        FocalTarget = GameObject.Find("CameraFocalPoint").transform;
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
        //transform.position = FocalTarget.position;

        if (FocalTarget)
        {
            #region clamp
            Vector3 ClampedFocalTarget = FocalTarget.position;

            if (!boundingBox)
            {
                Debug.LogError( "Someone forgot to put a bounding box around the level / messed it up." );
            }
            else
            {
                // Clamping math
                Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
                Vector3 bottomRight = cam.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, 0.0f));
                float halfWidthInWorld = bottomRight.x - center.x;
                float halfHeightInWorld = bottomRight.y - center.y;

                // Implementation ASSUMES a level will never be smaller than 1 screen
                if (FocalTarget.position.x + halfWidthInWorld > boundingBox.gameObject.transform.position.x + boundingBox.offset.x + boundingBox.size.x / 2.0f)
                {
                    // don't go too far right
                    ClampedFocalTarget.x = (boundingBox.gameObject.transform.position.x + boundingBox.offset.x + boundingBox.size.x / 2.0f) - halfWidthInWorld;
                }
                else if (FocalTarget.position.x - halfWidthInWorld < boundingBox.gameObject.transform.position.x + boundingBox.offset.x - boundingBox.size.x / 2.0f)
                {
                    // don't go too far left
                    ClampedFocalTarget.x = (boundingBox.gameObject.transform.position.x + boundingBox.offset.x - boundingBox.size.x / 2.0f) + halfWidthInWorld;
                }

                if (FocalTarget.position.y + halfHeightInWorld > boundingBox.gameObject.transform.position.y + boundingBox.offset.y + boundingBox.size.y / 2.0f)
                {
                    // don't go too far up/down
                    ClampedFocalTarget.y = (boundingBox.gameObject.transform.position.y + boundingBox.offset.y + boundingBox.size.y / 2.0f) - halfHeightInWorld;
                }
                else if (FocalTarget.position.y - halfHeightInWorld < boundingBox.gameObject.transform.position.y + boundingBox.offset.y - boundingBox.size.y / 2.0f)
                {
                    // don't go too far up/down
                    ClampedFocalTarget.y = (boundingBox.gameObject.transform.position.y + boundingBox.offset.y - boundingBox.size.y / 2.0f) + halfHeightInWorld;
                }
            }
            #endregion

            Vector3 point = cam.WorldToViewportPoint( ClampedFocalTarget );
            Vector3 delta = ClampedFocalTarget - cam.ViewportToWorldPoint( new Vector3( 0.5f, 0.5f, point.z ) );
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp( transform.position, destination, ref velocity, DampTime );
        }
        
    }

    /// <summary>
    /// Instantly centers the camera on the focus target
    /// </summary>
    public void SnapToFocalPoint()
    {
        transform.position = FocalTarget.position;
    }
}
