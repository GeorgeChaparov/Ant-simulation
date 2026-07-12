using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class PheromoneManager : MonoBehaviour
{
    [SerializeField]
    private int width = 10;
    [SerializeField]
    private int height = 10;

    [SerializeField]
    [Tooltip("How often should the pheromones update.")]
    private float updateInterval = 0.5f;


    [SerializeField]
    [Tooltip("How fast should the pheromones for food decay.")]
    private float foodPheromoneDecayRate = 1f;

    [SerializeField]
    [Tooltip("How fast should the pheromones for the nest decay.")]
    private float nestPheromoneDecayRate = 1f;

    private float[] foodIntensity;
    private float[] nestIntensity;

    /// <summary>
    /// Used to check which cells have any pheromones in them. Will be changed with lazy decay as needed.
    /// </summary>
    private HashSet<int> activeCells;

    /// <summary>
    /// Stores the last time each cell was updated. Will be removed in favoure of lazy decay as needed.
    /// </summary>
    private float[] lastUpdate;

    private int gridSize = 0;

    private static PheromoneManager pheromoneManager = null;
    public static PheromoneManager GetPheromoneManager { get { return pheromoneManager; } }

    private void Awake()
    {
        pheromoneManager = FindAnyObjectByType<PheromoneManager>();

        if (pheromoneManager.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gridSize = width * height;
        foodIntensity = new float[gridSize];
        nestIntensity = new float[gridSize];

        activeCells = new HashSet<int>();
        lastUpdate = new float[gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            foodIntensity[i] = 0;
            nestIntensity[i] = 0;

            lastUpdate[i] = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePheromones();
    }

    public void DepositPheromoneOn(Vector2 position, PheromoneSetting pheromoneSettings, PheromoneType pheromoneType)
    {
        int index = GetPosFromVector(position);

        switch (pheromoneType)
        {
            case PheromoneType.None:
                break;
            case PheromoneType.Food:
                foodIntensity[index] += pheromoneSettings.strength;

                lastUpdate[index] = Time.time;
                activeCells.Add(index);
                break;
            case PheromoneType.Nest:
                nestIntensity[index] += pheromoneSettings.strength;

                lastUpdate[index] = Time.time;
                activeCells.Add(index);
                break;
            default:
                break;
        }
    }

    private (Vector2, float) GetStrongestPheromoneInRange(Vector2 position, PheromoneType type, int radius = 3)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);

        int minX = Mathf.Max(0, x - radius);
        int maxX = Mathf.Min(width - 1, x + radius);

        int minY = Mathf.Max(0, y - radius);
        int maxY = Mathf.Min(height - 1, y + radius);

        int radiusSquared = radius * radius;

        Vector2 strongestPos = Vector2.zero;
        float strongestVal = 0;

        for (int iy = minY; iy <= maxY; iy++)
        {
            for (int ix = minX; ix <= maxX; ix++)
            {
                int dx = ix - x;
                int dy = iy - y;

                if (dx * dx + dy * dy > radiusSquared)
                    continue;

                int index = GetIndexFromPos(ix, iy);

                float intensity = GetPheromoneAt(index, type);

                if (strongestVal < intensity)
                {
                    strongestVal = intensity;
                    strongestPos.x = ix;
                    strongestPos.y = iy;
                }
            }
        }

        if (strongestVal == 0)
            return (Vector2.zero, 0);

        return ((strongestPos - position).normalized, strongestVal);
    }


    /// <summary>
    /// Searches for the strongest pheromone around three sensors positioned
    /// relative to the given orientation.
    /// </summary>
    /// <param name="position">Current position</param>
    /// <param name="orientation"></param>
    /// <param name="type">The type of pheremone</param>
    /// <param name="radius">The radius around each sensor that will be searched.</param>
    /// <param name="angle">The angle of the sensors from the forward orientation.</param>
    /// <param name="sensorsDistance">How far are the sensors form the position.</param>
    /// <returns>The position of the stronges pheromone in the given radius around the sensors, as 2D vector. 
    /// If the pheromone is not found, it returns Vector2.zero.</returns>
    public Vector2 GetStrongestPheromonePos(Vector2 position, Vector2 orientation, PheromoneType type, int radius = 3, float angle = 30, float sensorsDistance = 3)
    {
        Vector2[] sensorsPos = new Vector2[3];
        Vector2 forwardLeftSensor = position + Utils.Rotate(orientation, -angle) * sensorsDistance;
        Vector2 forwardSensor = position + orientation * sensorsDistance;
        Vector2 forwardRightSensor = position + Utils.Rotate(orientation, angle) * sensorsDistance;

        (Vector2 strongestPosLeft, float strongestValLeft) = GetStrongestPheromoneInRange(forwardLeftSensor, type, radius);
        (Vector2 strongestPosForward, float strongestValForward) = GetStrongestPheromoneInRange(forwardLeftSensor, type, radius);
        (Vector2 strongestPosRight, float strongestValRight) = GetStrongestPheromoneInRange(forwardLeftSensor, type, radius);

        Vector2 strongestPos = strongestPosLeft;
        float strongestVal = strongestValLeft;

        if (strongestVal < strongestValForward)
        {
            strongestVal = strongestValForward;
            strongestPos.x = strongestPosForward.x;
            strongestPos.y = strongestPosForward.y;
        }
        if (strongestVal < strongestValRight)
        {
            strongestVal = strongestValRight;
            strongestPos.x = strongestPosRight.x;
            strongestPos.y = strongestPosRight.y;
        }

        return strongestPos;
    }

    private float GetPheromoneAt(int index, PheromoneType type)
    {
        float intensity = 0;
        switch (type)
        {
            case PheromoneType.None:
                break;
            case PheromoneType.Food:
                intensity = foodIntensity[index];
                break;
            case PheromoneType.Nest:
                intensity = nestIntensity[index];
                break;
            default:
                break;
        }

        return intensity;
    }

    private void UpdatePheromones()
    {
        List<int> removeList = new List<int>();

        foreach (var cellIndex in activeCells)
        {
            if (Time.deltaTime - lastUpdate[cellIndex] <= updateInterval) { continue; }

            float newFoodIntensity = DecayPheromone(foodPheromoneDecayRate, foodIntensity[cellIndex]);
            float newNestIntensity = DecayPheromone(nestPheromoneDecayRate, nestIntensity[cellIndex]);

            foodIntensity[cellIndex] = Math.Max(0, newFoodIntensity);
            nestIntensity[cellIndex] = Math.Max(0, newFoodIntensity);

            if (newFoodIntensity + newNestIntensity <= 0)
            {
                removeList.Add(cellIndex);
            }

            lastUpdate[cellIndex] = Time.time;
        }

        for (int i = removeList.Count - 1; i >= 0; i--)
        {
            int index = removeList[i];
            activeCells.Remove(index);
            removeList.Remove(index);
        }
    }

    private int GetPosFromVector(Vector2 position)
    {
        return Mathf.FloorToInt(position.y) * width + Mathf.FloorToInt(position.x);
    }
    private int GetIndexFromPos(int x, int y)
    {
        return x * width + y;
    }

    private float DecayPheromone(float baseDecayRate, float currIntensity)
    {
        float k = 1f; // Shows how much the intensity plays a role in the decay rate.
        float decayRate = baseDecayRate / (1 + currIntensity * k);

        currIntensity *= (1f - decayRate * Time.deltaTime);

        return currIntensity;
    }
}
