using System.Collections.Generic;

using UnityEngine;

public class Visualization : MonoBehaviour
{
    private static Visualization instance = null;
    public static Visualization Instance { get { return instance; } }
    private AntNest[] antNests = new AntNest[0];
    public FoodSource[] foodSources = new FoodSource[0];

    private void Awake()
    {
        instance = FindAnyObjectByType<Visualization>();

        if (instance.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
    }

    public void OnSimulationInitialize()
    {
        UpdateNests();
        AntManager.Instance.OnNestAdded += UpdateNests;
        UpdateFoodSources();
        FoodSourceManager.Instance.OnFoodSourceAdded += UpdateFoodSources;
    }

    private void OnDrawGizmos()
    {
        Draw();
    }

    private void UpdateNests()
    {
        antNests = AntManager.Instance.Nests.ToArray();
    }

    private void UpdateFoodSources()
    {
        foodSources = FoodSourceManager.Instance.FoodSources.ToArray();
    }

    private void Draw()
    {
        (Vector2 position, float intensity)[] pheromones = PheromoneManager.Instance.GetActivePheromones();
        foreach (var pheromone in pheromones)
        {
            if (pheromone.intensity < 5)
            {
                Gizmos.color = Color.red;
            }
            else if (pheromone.intensity < 15)
            {
                Gizmos.color = Color.orange;
            }
            else
            {
                Gizmos.color = Color.green;
            }

            Gizmos.DrawSphere(pheromone.position, 0.5f);
        }

        foreach (var nest in antNests)
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawSphere(nest.transform.position, 5);

            List<Ant> ants = nest.Ants; 
            foreach (var ant in ants)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(ant.position, 3);

                foreach (var sensor in ant.sensors)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(sensor.Position, sensor.Radius / 2);
                }
            }
        }

        //foreach (var foodSource in foodSources)
        //{
        //    Gizmos.color = Color.brown;
        //    Debug.Log(foodSource.Position);
        //    Debug.Log(foodSource.Radius);
        //    Gizmos.DrawSphere(foodSource.Position, foodSource.Radius);
        //}

        (Vector2 pos, float amount)[] foodCells = FoodSourceManager.Instance.GetFood();
        foreach (var foodCell in foodCells)
        {
            if (foodCell.amount == 0)
            {
                continue;
            }

            if (foodCell.amount < 1)
            {
                Gizmos.color = Color.gray1;
            }
            else if (foodCell.amount < 2)
            {
                Gizmos.color = Color.gray3;
            }
            else if (foodCell.amount < 3)
            {
                Gizmos.color = Color.gray5;
            }
            else if (foodCell.amount < 3)
            {
                Gizmos.color = Color.gray7;
            }
            else
            {
                Gizmos.color = Color.gray9;
            }

            Gizmos.DrawSphere(foodCell.pos, 0.5f);
        }
    }
}
