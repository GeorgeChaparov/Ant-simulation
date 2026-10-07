using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UIElements;

public class FoodSourceManager : MonoBehaviour
{
    public delegate void FoodSourceAdded();
    public event FoodSourceAdded OnFoodSourceAdded;

    private int width = 10;
    private int height = 10;
    private int gridSize = 0;

    [SerializeField]
    List<FoodSource> foodSources = new List<FoodSource>();
    public List<FoodSource> FoodSources => foodSources;
    private static FoodSourceManager instance = null;
    public static FoodSourceManager Instance { get { return instance; } }

    private int[] foodAmount;

    private void Awake()
    {
        instance = FindAnyObjectByType<FoodSourceManager>();

        if (instance.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
    }

    public void Init(int gridWidth, int gridHeight)
    {
        width = gridWidth;
        height = gridHeight;

        gridSize = width * height;
        foodAmount = new int[gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            foodAmount[i] = 0;
        }

        foreach (var foodSource in foodSources)
        {
            AddFoodSource(foodSource);
        }
    }

    private int GetFoodAt(int index)
    {
        int amount = foodAmount[index];

        return amount;
    }

    private void AddFoodSource(FoodSource foodSource)
    {
        int x = Mathf.FloorToInt(foodSource.Position.x);
        int y = Mathf.FloorToInt(foodSource.Position.y);

        int minX = Mathf.Max(0, x - foodSource.Radius);
        int maxX = Mathf.Min(width - 1, x + foodSource.Radius);

        int minY = Mathf.Max(0, y - foodSource.Radius);
        int maxY = Mathf.Min(height - 1, y + foodSource.Radius);

        int radiusSquared = foodSource.Radius * foodSource.Radius;

        for (int iy = minY; iy <= maxY; iy++)
        {
            for (int ix = minX; ix <= maxX; ix++)
            {
                int dx = ix - x;
                int dy = iy - y;

                if (dx * dx + dy * dy > radiusSquared)
                    continue;

                int index = Utils.GetIndexFromPos(ix, iy);

                foodAmount[index] = foodSource.Amount;
            }
        }
    }

    public void RemoveFoodAt(Vector2 pos, int amount)
    {
        int index = Utils.GetIndexFromVector(pos);
        int currentAmount = foodAmount[index];

        int newAmount = currentAmount - amount;
        if (newAmount < 0)
        {
            Debug.LogError($"Removing more food ({amount}) from the food source then its current amount ({currentAmount}).");
            return;
        }

        foodAmount[index] = newAmount;
    }

    public Vector2 GetFoodInRadius(AntSensor[] antSensors)
    {
        bool foundFood = false;
        Vector2 foodPos = Vector2.zero;
        foreach (var antSensor in antSensors)
        {
            Vector2 pos = antSensor.Position;
            int radius = antSensor.Radius;

            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);

            int minX = Mathf.Max(0, x - radius);
            int maxX = Mathf.Min(width - 1, x + radius);

            int minY = Mathf.Max(0, y - radius);
            int maxY = Mathf.Min(height - 1, y + radius);

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

                    int amount = foodAmount[index];
                    if (amount != 0)
                    {
                        foodPos.x = ix;
                        foodPos.y = iy;

                        foundFood = true;
                        break;
                    }
                }

                if (foundFood)
                {
                    break;
                }
            }

            if (foundFood)
            {
                break;
            }
        }

        return foodPos;
    }
}
