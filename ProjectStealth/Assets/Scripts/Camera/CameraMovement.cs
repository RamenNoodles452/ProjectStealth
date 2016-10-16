using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    public float DampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    public Transform FocalTarget;
    private Camera cam;

    // Use this for initialization
    void Start ()
    {
        Screen.SetResolution(640, 480, false, 60);
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update ()
    {
        //transform.position = FocalTarget.position;
        
        if (FocalTarget)
        {
            Vector3 point = cam.WorldToViewportPoint(FocalTarget.position);
            Vector3 delta = FocalTarget.position - cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, DampTime);
        }
        
    }
}
