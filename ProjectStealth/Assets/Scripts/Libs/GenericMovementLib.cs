using UnityEngine;
using System.Collections;

public class GenericMovementLib : MonoBehaviour {

    public Vector2 BezierCurveMovement(float distance, Vector2 start, Vector2 end, Vector2 curvePoint)
    {
        Vector2 ab = Vector2.Lerp(start, curvePoint, distance);
        Vector2 bc = Vector2.Lerp(curvePoint, end, distance);
        return Vector2.Lerp(ab, bc, distance);
    }
}
