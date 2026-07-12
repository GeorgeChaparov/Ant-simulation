using UnityEngine;

public class Utils
{
    public static Vector2 Rotate(Vector2 orientation, float degrees)
    {
        float r = -degrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(r);
        float sin = Mathf.Sin(r);

        return new Vector2(
            orientation.x * cos - orientation.y * sin,
            orientation.x * sin + orientation.y * cos
        );
    }
}
