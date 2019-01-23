using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For rotating UI elements over time.
/// </summary>
public class SpinUIElement : MonoBehaviour
{
    #region vars
    [SerializeField]
    private float speed = 360.0f; // degrees per second.
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RectTransform rect_transform = GetComponent<RectTransform>();
        rect_transform.Rotate( new Vector3( 0.0f, 0.0f, Time.unscaledDeltaTime * speed ) );
    }
}
