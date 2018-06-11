using UnityEngine;
using System.Collections;
using System;


public class GenericMovementLib : MonoBehaviour
{
    protected CharacterStats char_stats;

    void Start()
    {
        char_stats = GetComponent<CharacterStats>();
    }

    public Vector2 BezierCurveMovement(float distance, Vector2 start, Vector2 end, Vector2 curve_point)
    {
        Vector2 ab = Vector2.Lerp(start, curve_point, distance);
        Vector2 bc = Vector2.Lerp(curve_point, end, distance);
        return Vector2.Lerp(ab, bc, distance);
    }

    // this function takes input as a string because animation event calling is rather limited in its capabilities 
    // dating this incase it gets better (June/2018)
    public void MoveOffset(string input)
    {
        Vector3 offset = MoveOffsetStringParse(input);
        transform.position += offset;
    }

    private Vector3 MoveOffsetStringParse(string offset)
    {
        string[] split_string = offset.Split(',');
        int xoffset = Int32.Parse(split_string[0]) * char_stats.GetFacingXComponent();
        int yoffset = Int32.Parse(split_string[1]);
        return new Vector3(xoffset, yoffset, 0);
    }
}
