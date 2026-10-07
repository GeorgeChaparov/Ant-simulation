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

    public static bool IsOutOfBounds(Vector2 pos)
    {
        int width = GameManager.Instance.gridWidth;
        int height = GameManager.Instance.gridHeight;

        if (pos.x > width ||
            pos.x < 0 ||
            pos.y > height ||
            pos.y < 0)
        {
            return true;
        }

        return false;
    }

    public static int GetPosFromVector(Vector2 position)
    {
        return Mathf.FloorToInt(position.y) * GameManager.Instance.gridWidth + Mathf.FloorToInt(position.x);
    }

    public static int GetIndexFromPos(int x, int y)
    {
        return x * GameManager.Instance.gridWidth + y;
    }
}
