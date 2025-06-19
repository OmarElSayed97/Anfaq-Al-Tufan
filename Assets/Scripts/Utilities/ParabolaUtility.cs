// ParabolaUtility.cs
using UnityEngine;

public static class ParabolaUtility
{
    // Returns an array of points representing a quadratic Bézier curve
    public static Vector3[] GetParabolaPoints(Vector3 start, Vector3 control, Vector3 end, int resolution)
    {
        Vector3[] points = new Vector3[resolution + 1];

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            points[i] = CalculateQuadraticBezierPoint(t, start, control, end);
        }

        return points;
    }

    private static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);
    }
}
