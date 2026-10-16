using System.Collections.Generic;

using UnityEngine;

public class AntNest : MonoBehaviour
{
    [SerializeField]
    private List<Ant> ants = new List<Ant>();

    [SerializeField]
    private int maxAnts = 10;

    [SerializeField]
    private float spawnRate = 1.0f;

    [Header("\t\t Ant Settings")]

    [SerializeField]
    private float minMovementSpeed = 6f;

    [SerializeField]
    private float maxMovementSpeed = 20f;

    [SerializeField]
    [Tooltip("The min amount in degrees we can rotate while searching for food.")]
    private float minRandomRotation = 10f;

    [SerializeField]
    [Tooltip("The max amount in degrees we can rotate while searching for food.")]
    private float maxRandomRotation = 45f;

    [SerializeField]
    [Tooltip("The number of blocks around the ant's sensor that are gonna be searched when searching for pheromones.")]
    private int sensorRadius = 2;

    [SerializeField]
    [Tooltip("How far the sensors sit from the ant")]
    private int sensorDistance = 3;

    [SerializeField]
    [Tooltip("The travelled distance from a point of interest after which the ant will trust its estimate, of where it is, the least.")]
    private float maxReliableDistance = 1000f;

    [SerializeField]
    [Tooltip("The random deviation on the ant when the estimate confidence is low.")]
    private float maxEstimateDeviation = 0.0002f;

    [SerializeField]
    [Tooltip(".")]
    private bool followWeakPheromones = false;

    [SerializeField]
    [Tooltip("How fast will the ant turn when we do not find the nest where we thought it is.")]
    private float nestSearchTurnSpeed = 3f;

    [SerializeField]
    [Tooltip("How fast will the searching radius expand when we do not find the nest where we thought it is.")]
    private float nestSearchExpansionSpeed = 3f;

    private float lastSpawn = 0;

    public List<Ant> Ants { get { return ants; } }
    public int MaxAnts { get { return maxAnts; } set { maxAnts = value; } }
    public float SpawnRate { get { return spawnRate; } set { spawnRate = value; } }


    private void Start()
    {
        lastSpawn = spawnRate;
    }

    private void Update()
    {
        if (MaxAnts == ants.Count)
        {
            return;
        }

        if (Time.time - lastSpawn < SpawnRate)
        {
            return;
        }

        lastSpawn = Time.time;
        Ant newAnt = new Ant(GameManager.Instance.GetAntID(), transform.position,
            new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized, Random.Range(minMovementSpeed, maxMovementSpeed), 
            maxEstimateDeviation, minRandomRotation, maxRandomRotation, sensorDistance, sensorRadius, followWeakPheromones, 
            nestSearchTurnSpeed, nestSearchExpansionSpeed);

        SpawnAnt(newAnt);
    }

    private void OnValidate()
    {
        for (int i = 0; i < ants.Count; i++)
        {
            Ant ant = Ants[i];

            ant.sensorRadius = sensorRadius;
            ant.sensorDistance = sensorDistance;
            ant.maxReliableDistance = maxReliableDistance;
            ant.movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);
            ant.maxEstimateDeviation = maxEstimateDeviation;
            ant.minRandomRotation = minRandomRotation;
            ant.maxRandomRotation = maxRandomRotation;

            ant.sensors[0] = new AntSensor(ant.position, ant.orientation, 35, sensorDistance, sensorRadius, followWeakPheromones);
            ant.sensors[1] = new AntSensor(ant.position, ant.orientation, 0, sensorDistance, sensorRadius, followWeakPheromones);
            ant.sensors[2] = new AntSensor(ant.position, ant.orientation, -35, sensorDistance, sensorRadius, followWeakPheromones);

            ant.nestSearchTurnSpeed = nestSearchTurnSpeed;
            ant.nestSearchExpansionSpeed = nestSearchExpansionSpeed;

            Ants[i] = ant;
        }
    }

    public void SpawnAnt(Ant ant)
    {
        ants.Add(ant);
    }

    public void RemoveAnt(int antIndex)
    {
        if (ants.Count >= antIndex)
        {
            throw new System.Exception("Ant index does not exists!");
        }

        ants.RemoveAt(antIndex);
    }
}
