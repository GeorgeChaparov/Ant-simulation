using System;
using System.Collections.Generic;

using UnityEngine;

public class PheromoneManager : MonoBehaviour
{
    [SerializeField]
    private int width = 10;
    [SerializeField]
    private int height = 10;

    private float[] foodIntensity;
    private float[] nestIntensity;

    private bool[] dirtyCells;
    private float[] lastUpdate;

    private int gridSize = 0;

    private float updateInterval = 0.5f;


    void Start()
    {
        gridSize = width * height;
        foodIntensity = new float[gridSize];
        nestIntensity = new float[gridSize];

        dirtyCells = new bool[gridSize];
        lastUpdate = new float[gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            foodIntensity[i] = 0;
            nestIntensity[i] = 0;

            dirtyCells[i] = false;
            lastUpdate[i] = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePheromones();
    }

    public void AddPheromoneOn(Vector2 position, PheromoneSetting pheromoneSettings, PheromoneType pheromoneType)
    {
        int index = GetPosFromVector(position);
        float currIntensity = 0;

        switch (pheromoneType)
        {
            case PheromoneType.None:
                break;
            case PheromoneType.Food:
                currIntensity = foodIntensity[index];

                foodIntensity[index] = currIntensity == 0 ? 
                    pheromoneSettings.strength :
                    DepositPheromone(pheromoneSettings.strength, currIntensity);

                lastUpdate[index] = Time.deltaTime;
                dirtyCells[index] = true;
                break;
            case PheromoneType.Nest:
                currIntensity = nestIntensity[index];

                nestIntensity[index] = currIntensity == 0 ?
                    pheromoneSettings.strength :
                    DepositPheromone(pheromoneSettings.strength, currIntensity);

                lastUpdate[index] = Time.deltaTime;
                dirtyCells[index] = true;
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
        for (int i = 0; i < gridSize; i++)
        {
            if (!dirtyCells[i]) { return; }
            if (Time.deltaTime - lastUpdate[i] <= updateInterval) { return; }

            float newFoodIntensity = DecayPheromone(1, foodIntensity[i]);
            if (newFoodIntensity <= 0)
            {
                foodIntensity[i] = 0;
                dirtyCells[i] = false;
            }
            else { foodIntensity[i] = newFoodIntensity; }

            float newNestIntensity = DecayPheromone(1, nestIntensity[i]);
            if (newNestIntensity <= 0)
            {
                nestIntensity[i] = 0;
                dirtyCells[i] = false;
            }
            else { nestIntensity[i] = newNestIntensity; }

            lastUpdate[i] = Time.deltaTime;
        }
    }

    private int GetPosFromVector(Vector2 position)
    {
        return Mathf.FloorToInt(position.y) * width + Mathf.FloorToInt(position.x);
    }

    public float DecayPheromone(float baseDecayRate, float currIntensity)
    {
        float k = 1f; // Shows how muche the intensity plays a role in the decay rate.
        float decayRate = baseDecayRate / (1 + currIntensity * k);

        currIntensity *= (1f - decayRate * Time.deltaTime);

        return currIntensity;
    }

    public float DepositPheromone(float amount, float currIntensity)
    {
        currIntensity += amount;

        return currIntensity;
    }
}
