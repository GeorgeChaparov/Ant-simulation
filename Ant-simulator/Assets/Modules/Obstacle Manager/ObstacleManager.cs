using System.Collections.Generic;

using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    private int width = 10;
    private int height = 10;
    private int gridSize = 0;

    private bool[] obstacles;


    private static ObstacleManager instance = null;
    public static ObstacleManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = FindAnyObjectByType<ObstacleManager>();

        if (instance.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
    }

    public void Init(int gridWidth, int gridHeight)
    {
        width = gridWidth;
        height = gridHeight;

        gridSize = width * height;
        obstacles = new bool[gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            obstacles[i] = false;
        }
    }

    private void AddObstacleInRadius(Vector2 position, int radius)
    {
        Utils.DoForeachInRadius(position, radius, index =>
        {
            obstacles[index] = true;
        });
    }

    public void AddObstacleAt(Vector2 position, int radius)
    {
        AddObstacleInRadius(position, radius);
    }

    public void AddObstacleAt(int index)
    {
        obstacles[index] = true;
    }

    public void RemoveObstacleAt(Vector2 position)
    {
        int index = Utils.GetIndexFromVector(position);
        obstacles[index] = false;
    }

    public void DeleteObstacleAt(Vector2 position, int radius)
    {
        Utils.DoForeachInRadius(position, radius, index =>
        {
            // There is food at this position therefor we need to remove the food first which will automatically remove the obstacle as well.
            if (!FoodSourceManager.Instance.HaveFoodAtIndex(index))
            {
                obstacles[index] = false;
            }
        });
    }

    public void RemoveObstacleAt(int index)
    {
        obstacles[index] = false;
    }

    public Vector2[] GetObstacles()
    {
        List<Vector2> obstaclesList = new List<Vector2>();

        for (int i = 0; i < obstacles.Length; i++)
        {
            if (obstacles[i] != false)
            {
                obstaclesList.Add(Utils.GetVectorFromIndex(i));
            }
        }

        return obstaclesList.ToArray();
    }
}
