using System;
using System.Collections.Generic;

using UnityEngine;

public class PheromoneManager : MonoBehaviour
{
    private int width = 10;
    private int height = 10;
    private int gridSize = 0;

    [SerializeField]
    [Tooltip("How often should the pheromones update.")]
    private float updateInterval = 0.5f;


    [SerializeField]
    [Tooltip("How fast should the pheromones for food decay.")]
    private float foodPheromoneDecayRate = 1f;

    [SerializeField]
    [Tooltip("The maximum intensity a cell can have.")]
    private float maxIntensity = 100;

    private float[] foodIntensity;

    /// <summary>
    /// Used to check which cells have any pheromones in them. Will be changed with lazy decay as needed.
    /// </summary>
    private HashSet<int> activeCells;

    /// <summary>
    /// Stores the last time each cell was updated. Will be removed in favorer of lazy decay as needed.
    /// </summary>
    private float[] lastUpdate;

    /// <summary>
    /// The ID of the last ant that updated the intensity of the cell.
    /// </summary>
    private int[] lastAntID;

    private static PheromoneManager instance = null;
    public static PheromoneManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = FindAnyObjectByType<PheromoneManager>();

        if (instance.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePheromones();
    }

    public void Init(int gridWidth, int gridHeight)
    { 
        width = gridWidth; 
        height = gridHeight;

        gridSize = width * height;
        foodIntensity = new float[gridSize];

        activeCells = new HashSet<int>();
        lastUpdate = new float[gridSize];
        lastAntID = new int[gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            foodIntensity[i] = 0;
            lastUpdate[i] = 0;
            lastAntID[i] = 0;
        }
    }

    public void DepositPheromoneOn(int antID, Vector2 position, PheromoneSetting pheromoneSettings, PheromoneType pheromoneType)
    {
        int index = Utils.GetIndexFromVector(position);

        if (antID == lastAntID[index])
        {
            return;
        }

        lastAntID[index] = antID;

        switch (pheromoneType)
        {
            case PheromoneType.None:
                break;
            case PheromoneType.Food:


                if (foodIntensity[index] >= maxIntensity)
                {
                    break;
                }

                foodIntensity[index] += Math.Min(pheromoneSettings.strength, maxIntensity);

                lastUpdate[index] = Time.time;
                activeCells.Add(index);
                break;
            default:
                break;
        }
    }
    /// <param name="intensityOffset">An offset between 0.00 and 1.00, applied towards the sensor with the most pheromone cells</param>
    /// <param name="weakestOffset">An offset between 0.00 and 1.00 towards the cell with the smallest valid intensity</param>
    /// <returns>The average position of each cell with valid intensity, from the sensor with the smallest intensity, 
    /// with an offset towards the average position of each cell with a valid intensity, 
    /// from the sensor that have the most valid cells.
    /// If the pheromone is not found, it returns Vector2.zero.</returns>
    public Vector2 GetWeakestWeightedPheromonePos(AntSensor[] sensors, float intensityOffset, float weakestOffset)
    {
        (Vector2 pos, float val, int pheromoneCount)[] foundPheromones = new (Vector2 weakestPosLeft, float weakestValLeft, int pheromoneCount)[sensors.Length];

        intensityOffset = Math.Clamp(intensityOffset, 0f, 1f);
        weakestOffset = Math.Clamp(weakestOffset, 0f, 1f);

        for (int i = 0; i < sensors.Length; i++)
        {
            AntSensor sensor = sensors[i];
            
            foundPheromones[i] = GetAveragePheromonPosInRagne(sensor.Position, PheromoneType.Food, sensor.Radius, weakestOffset);
        }

        int mostPheromones = foundPheromones[0].pheromoneCount;
        int sensorIndexWithMostPheromones = 0;
        float weakestVal = float.MaxValue;
        Vector2 weakestPos = Vector2.zero;

        for (int i = 0; i < sensors.Length; i++)
        {
            if (foundPheromones[i].pheromoneCount == 0)
            {
                continue;
            }

            if (mostPheromones < foundPheromones[i].pheromoneCount)
            {
                mostPheromones = foundPheromones[i].pheromoneCount;
                sensorIndexWithMostPheromones = i;
            }

            if (weakestVal > foundPheromones[i].val)
            {
                weakestVal = foundPheromones[i].val;
                weakestPos.x = foundPheromones[i].pos.x;
                weakestPos.y = foundPheromones[i].pos.y;
            }
        }

        if (weakestPos != foundPheromones[sensorIndexWithMostPheromones].pos)
        {
            weakestPos = Vector2.Lerp(weakestPos, foundPheromones[sensorIndexWithMostPheromones].pos, intensityOffset);
        }

        return weakestPos;
    }

    /// <summary>
    /// Can be used to get an array to visualize the food.
    /// </summary>
    public (Vector2 pos, float intensity)[] GetActivePheromones()
    {
        (Vector2 pos, float intensity)[] activePheromones = new (Vector2 pos, float intensity)[activeCells.Count];

        int iterator = 0;

        foreach (var activeCell in activeCells)
        {
            activePheromones[iterator++] = (Utils.GetVectorFromIndex(activeCell), foodIntensity[activeCell]);
        }

        return activePheromones;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="type"></param>
    /// <param name="radius"></param>
    /// <param name="weakestOffset">An offset between 0.00 and 1.00 towards the cell with the smallest valid intensity</param>
    /// <returns>The average position of all of the pheromones in range with an offset towards the weakest pheromone as well as its intensity.</returns>
    private (Vector2 averagePos, float weakestVal, int pheromoneCount) GetAveragePheromonPosInRagne(Vector2 position, PheromoneType type, int radius, float weakestOffset)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);

        int minX = Mathf.Max(0, x - radius);
        int maxX = Mathf.Min(width - 1, x + radius);

        int minY = Mathf.Max(0, y - radius);
        int maxY = Mathf.Min(height - 1, y + radius);

        int radiusSquared = radius * radius;

        Vector2 averagePos = Vector2.zero;
        int count = 0;

        Vector2 weakestPos = Vector2.zero;
        float weakestVal = float.MaxValue;

        for (int iy = minY; iy <= maxY; iy++)
        {
            for (int ix = minX; ix <= maxX; ix++)
            {
                int dx = ix - x;
                int dy = iy - y;

                if (dx * dx + dy * dy > radiusSquared)
                    continue;

                int index = Utils.GetIndexFromPos(ix, iy);

                float intensity = GetPheromoneAt(index, type);

                if (intensity == 0)
                {
                    continue;
                }

                averagePos += new Vector2(ix, iy);

                if (intensity < weakestVal)
                {
                    weakestVal = intensity;
                    weakestPos = new Vector2(ix, iy);
                }

                count++;
            }
        }

        if (weakestVal == float.MaxValue)
            return (Vector2.zero, 0, 0);

        Vector2 average = averagePos / count;
        Vector2 weigthedAveragePos = Vector2.Lerp(average, weakestPos, weakestOffset);

        return (weigthedAveragePos, weakestVal, count);
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
            if (Time.time - lastUpdate[cellIndex] > updateInterval) { continue; }

            float newFoodIntensity = DecayPheromone(foodPheromoneDecayRate, foodIntensity[cellIndex]);

            foodIntensity[cellIndex] = Math.Max(0, newFoodIntensity);

            // If all intensities combined are effectively zero.
            if (foodIntensity[cellIndex] <= 0.1)
            {
                foodIntensity[cellIndex] = 0;
                removeList.Add(cellIndex);
            }

            lastUpdate[cellIndex] = Time.time;
        }

        for (int i = 0; i < removeList.Count; i++)
        {
            int index = removeList[i];
            activeCells.Remove(index);
        }
    }

    private float DecayPheromone(float baseDecayRate, float currIntensity)
    {
        float k = 1f; // Shows how much the intensity plays a role in the decay rate.
        float decayRate = baseDecayRate / (1 + currIntensity * k);

        currIntensity *= (1f - decayRate * Time.deltaTime);

        return currIntensity;
    }
}
