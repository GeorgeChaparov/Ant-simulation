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
        foreach (var nest in antNests)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(nest.transform.position, 5);

            List<Ant> ants = nest.Ants; 
            foreach (var ant in ants)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(ant.position, 1);

                foreach (var sensor in ant.sensors)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(sensor.Position, sensor.Radius);
                }
            }
        }

        foreach (var foodSource in foodSources)
        {
            Gizmos.color = Color.orange;
            Debug.Log(foodSource.Position);
            Debug.Log(foodSource.Radius);
            Gizmos.DrawSphere(foodSource.Position, foodSource.Radius);
        }
    }
}
