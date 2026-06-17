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
    private bool[] dirtyCells;

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
        float currIntensity;

        switch (pheromoneType)
        {
            case PheromoneType.None:
                break;
            case PheromoneType.Food:
                currIntensity = foodIntensity[index];

                foodIntensity[index] = currIntensity == 0 ? 
                    pheromoneSettings.strength :
                    DepositPheromone(pheromoneSettings.strength, currIntensity);

                lastUpdate[index] = Time.time;
                dirtyCells[index] = true;
                break;
            case PheromoneType.Nest:
                currIntensity = nestIntensity[index];

                nestIntensity[index] = currIntensity == 0 ?
                    pheromoneSettings.strength :
                    DepositPheromone(pheromoneSettings.strength, currIntensity);

                lastUpdate[index] = Time.time;
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
            if (!dirtyCells[i]) { continue; }
            if (Time.deltaTime - lastUpdate[i] <= updateInterval) { continue; }

            float newFoodIntensity = DecayPheromone(foodPheromoneDecayRate, foodIntensity[i]);
            float newNestIntensity = DecayPheromone(nestPheromoneDecayRate, nestIntensity[i]);

            foodIntensity[i] = newFoodIntensity <= 0 ? 0 : newFoodIntensity;
            nestIntensity[i] = newNestIntensity <= 0 ? 0 : newNestIntensity;

            if (newFoodIntensity + newNestIntensity <= 0)
            {
                dirtyCells[i] = false;
            }

            lastUpdate[i] = Time.time;
        }
    }

    private int GetPosFromVector(Vector2 position)
    {
        return Mathf.FloorToInt(position.y) * width + Mathf.FloorToInt(position.x);
    }

    public float DecayPheromone(float baseDecayRate, float currIntensity)
    {
        float k = 1f; // Shows how much the intensity plays a role in the decay rate.
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
