using System;
using System.Collections.Generic;

using UnityEngine;

public class Visualization : MonoBehaviour
{
    private static Visualization instance = null;
    public static Visualization Instance { get { return instance; } }
    private AntNest[] antNests = new AntNest[0];

    public bool drawAnts = true;
    public bool drawPheromones = true;
    public bool drawObstacles = true;


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
        AntManager.Instance.OnNestsUpdated += UpdateNests;
    }

    private void OnDrawGizmos()
    {
        Draw();
    }

    private void UpdateNests()
    {
        antNests = AntManager.Instance.Nests.ToArray();
    }


    private void Draw()
    {
        DrawObstacles();
        DrawPheromones();
        DrawAntsAndNests();

        (Vector2 pos, float amount)[] foodCells = FoodSourceManager.Instance.GetFood();
        float maxFoodAmount = FoodSourceManager.Instance.MaxFoodAmount;
        foreach (var foodCell in foodCells)
        {
            if (foodCell.amount == 0)
            {
                continue;
            }


            float maxAmount20 = (maxFoodAmount * 20) / 100;
            float maxAmount40 = (maxFoodAmount * 40) / 100;
            float maxAmount60 = (maxFoodAmount * 60) / 100;
            float maxAmount80 = (maxFoodAmount * 80) / 100;

            if (foodCell.amount <= maxAmount20)
            {
                Gizmos.color = Color.gray1;
            }
            else if (foodCell.amount <= maxAmount40)
            {
                Gizmos.color = Color.gray3;
            }
            else if (foodCell.amount <= maxAmount60)
            {
                Gizmos.color = Color.gray5;
            }
            else if (foodCell.amount <= maxAmount80)
            {
                Gizmos.color = Color.gray7;
            }
            else
            {
                Gizmos.color = Color.gray9;
            }

            Gizmos.DrawSphere(foodCell.pos, 0.5f);
        }

        void DrawObstacles()
        {
            if (!drawObstacles) return;
            Vector2[] obstaclesPos = ObstacleManager.Instance.GetObstacles();
            Gizmos.color = Color.brown;
            foreach (var obstaclePos in obstaclesPos)
            {
                Gizmos.DrawCube(obstaclePos, new Vector3(1, 1, 1));
            }
        }

        void DrawPheromones()
        {
            if (!drawPheromones) return;

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
        }

        void DrawAntsAndNests()
        {
            foreach (var nest in antNests)
            {
                Gizmos.color = Color.grey;
                Gizmos.DrawSphere(nest.transform.position, 5);

                if (!drawAnts) return;

                List<Ant> ants = nest.Ants;
                foreach (var ant in ants)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(ant.position, 3);

                    foreach (var sensor in ant.sensors)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawSphere(sensor.Position, sensor.Radius);
                    }
                }
            }
        }
    }
}
