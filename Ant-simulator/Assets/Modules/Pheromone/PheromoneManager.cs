using System;
using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

using static UnityEditor.PlayerSettings;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

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


                foodIntensity[index] += pheromoneSettings.strength;

                lastUpdate[index] = Time.time;
                activeCells.Add(index);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Searches for the weakest pheromone around three sensors positioned
    /// relative to the given orientation.
    /// </summary>
    /// <returns>The position of the weakest pheromone in the given radius around the sensors, as 2D vector. 
    /// If the pheromone is not found, it returns Vector2.zero.</returns>
    public Vector2 GetWeakestPheromonePos(AntSensor[] sensors)
    {
        (Vector2 pos, float val)[] foundPheromones = new (Vector2 weakestPosLeft, float weakestValLeft)[sensors.Length];

        for (int i = 0; i < sensors.Length; i++)
        {
            AntSensor sensor = sensors[i];

            foundPheromones[i] = GetWeakestPheromoneInRange(sensor.Position, PheromoneType.Food, sensor.Radius);
        }

        Vector2 weakestPos = foundPheromones[0].pos;
        float weakestVal = foundPheromones[0].val;

        for (int i = 1; i < sensors.Length; i++)
        {
            if (weakestVal > foundPheromones[i].val)
            {
                weakestVal = foundPheromones[i].val;
                weakestPos.x = foundPheromones[i].pos.x;
                weakestPos.y = foundPheromones[i].pos.y;
            }
        }

        return weakestPos;
    }

    private (Vector2, float) GetWeakestPheromoneInRange(Vector2 position, PheromoneType type, int radius = 3)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);

        int minX = Mathf.Max(0, x - radius);
        int maxX = Mathf.Min(width - 1, x + radius);

        int minY = Mathf.Max(0, y - radius);
        int maxY = Mathf.Min(height - 1, y + radius);

        int radiusSquared = radius * radius;

        Vector2 weakestPos = Vector2.zero;
        float weakestVal = int.MaxValue;

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

                if (weakestVal > intensity)
                {
                    weakestVal = intensity;
                    weakestPos.x = ix;
                    weakestPos.y = iy;
                }
            }
        }

        if (weakestVal == int.MaxValue)
            return (Vector2.zero, 0);

        return (weakestPos, weakestVal);
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
            if (newFoodIntensity <= 0.1)
            {
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
}
