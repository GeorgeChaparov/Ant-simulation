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

    public static int GetIndexFromPos(int x, int y)
    {
        return y * GameManager.Instance.gridWidth + x;
    }

    public static int GetIndexFromVector(Vector2 position)
    {
        return GetIndexFromPos(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }

    public static Vector2 GetVectorFromIndex(int index)
    {
        int x = index % GameManager.Instance.gridWidth;
        int y = index / GameManager.Instance.gridWidth;

        return new Vector2(x, y);
    }
}
