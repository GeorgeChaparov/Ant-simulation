using System;
using System.Collections.Generic;

using UnityEngine;

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

    public float GetPheromone(Vector2 position, PheromoneType type)
    {
        int index = GetPosFromVector(position);

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

    private float DecayPheromone(float baseDecayRate, float currIntensity)
    {
        float k = 1f; // Shows how much the intensity plays a role in the decay rate.
        float decayRate = baseDecayRate / (1 + currIntensity * k);

        currIntensity *= (1f - decayRate * Time.deltaTime);

        return currIntensity;
    }
}
