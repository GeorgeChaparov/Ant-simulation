using System;
using System.Collections.Generic;

using Unity.VisualScripting;

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
    private (int id, float time)[] lastAnt;

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
        lastAnt = new (int id, float time)[gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            foodIntensity[i] = 0;
            lastUpdate[i] = 0;
            lastAnt[i] = (0, 0);
        }
    }

    public void DepositPheromoneOn(int antID, Vector2 position, PheromoneSetting pheromoneSettings, PheromoneType pheromoneType)
    {
        int index = Utils.GetIndexFromVector(position);

        // If its the same ant and at least 2 seconds have not passed 
        if (antID == lastAnt[index].id && 
            Time.time - lastAnt[index].time < 2)
        {

            return;
        }

        lastAnt[index] = (antID, Time.time);

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

    /// <summary>
    /// Finds the average position, between all cells for each sensor, and offsets the vector towards the cell with the smallest/biggest intensity. 
    /// Then it compares all of the vectors and finds, which of the have the smallest/biggest intensity, 
    /// and offsets it towards the vector, that is returned form the sensor, that has detected the most cells with a valid intensity.
    /// </summary>
    /// <param name="intensityOffset">An offset between 0.00 and 1.00, applied towards the sensor with the most pheromone cells</param>
    /// <param name="biasedOffset">An offset between 0.00 and 1.00 towards the cell with the smallest/biggest valid intensity, based on the bias</param>
    /// <param name="weakBias">If true, the position will be biased towards the smallest intensity cell. Else its biased towards the cell with the biggest intensity.</param>
    /// <returns>The average position of each cell with valid intensity, from the sensor with the smallest/biggest intensity, 
    /// with an offset towards the average position of each cell with a valid intensity, 
    /// from the sensor that have the most valid cells.
    /// If the pheromone is not found, it returns Vector2.zero.</returns>
    public Vector2 GetBiasedWeightedPheromonePos(AntSensor[] sensors, float intensityOffset, float biasedOffset)
    {
        bool weakBias = sensors[0].SearchForWeakPheromones;

        intensityOffset = Math.Clamp(intensityOffset, 0f, 1f);
        biasedOffset = Math.Clamp(biasedOffset, 0f, 1f);

        int mostPheromones = 0;
        Vector2 mostPheromonePos = Vector2.zero;

        float selectedVal = weakBias ? float.MaxValue : float.MinValue;
        Vector2 selectedPos = Vector2.zero;

        bool foundPheromone = false;

        for (int i = 0; i < sensors.Length; i++)
        {
            AntSensor sensor = sensors[i];

            (Vector2 currPos, float currVal, int currPheromoneCount) = 
                    GetAveragePheromonPosInRagne( sensor.Position, PheromoneType.Food, sensor.Radius, biasedOffset, weakBias);

            if (currPheromoneCount == 0) continue;

            foundPheromone = true;

            if (currPheromoneCount > mostPheromones)
            {
                mostPheromones = currPheromoneCount;
                mostPheromonePos = currPos;
            }

            bool isBetter = weakBias ? 
                currVal < selectedVal : 
                currVal > selectedVal;

            if (isBetter)
            {
                selectedVal = currVal;
                selectedPos = currPos;
            }
        }

        if (!foundPheromone) return Vector2.zero;

        return Vector2.Lerp(selectedPos, mostPheromonePos, intensityOffset);
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
    /// <param name="biasedOffset">An offset between 0.00 and 1.00 towards the cell with the smallest/biggest valid intensity, based on the bias</param>
    /// <param name="weakBias">If true, the position will be biased towards the smallest intensity cell. Else its biased towards the cell with the biggest intensity.</param>
    /// <returns>The average position of all of the pheromones in range with an offset towards the weakest/strongest pheromone as well as its intensity.</returns>
    private (Vector2 averagePos, float weakestVal, int pheromoneCount) GetAveragePheromonPosInRagne(Vector2 position,PheromoneType type,int radius, float biasedOffset, bool weakBias)
    {
        int averageX = 0;
        int averageY = 0;
        int count = 0;

        (int x, int y) selectedPos = (0, 0);
        float selectedVal = weakBias ? float.MaxValue : float.MinValue;

        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);

        int minX = Mathf.Max(0, x - radius);
        int maxX = Mathf.Min(GameManager.Instance.gridWidth - 1, x + radius);

        int minY = Mathf.Max(0, y - radius);
        int maxY = Mathf.Min(GameManager.Instance.gridHeight - 1, y + radius);

        int radiusSquared = radius * radius;

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

                if (intensity == 0f)
                    continue;

                averageX += ix;
                averageY += iy;
                count++;

                bool isBetter = weakBias ? 
                    intensity < selectedVal : 
                    intensity > selectedVal;

                if (isBetter)
                {
                    selectedVal = intensity;
                    selectedPos = (ix, iy);
                }
            }
        }

        if (count == 0) return (Vector2.zero, 0f, 0);

        Vector2 average = new Vector2(
            (float)averageX / count,
            (float)averageY / count);

        Vector2 weightedAveragePos = Vector2.Lerp(average, new Vector2(selectedPos.x, selectedPos.y), biasedOffset);

        return (weightedAveragePos, selectedVal, count);
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
