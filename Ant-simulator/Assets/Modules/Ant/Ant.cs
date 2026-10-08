using NUnit.Framework;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


/// <summary>
/// The current state of the ant. It shows the intent of the ant.
/// </summary>
public enum AntState
{
    /// <summary>
    /// State not defined.
    /// </summary>
    None = 0, 
    /// <summary>
    /// The ant does not follow established path.
    /// </summary>
    SearchingForFood = 1, 
    /// <summary>
    /// The ant follows established path to the nest.
    /// </summary>
    GoingToTheNest = 2,
    /// <summary>
    ///  The ant follows established path to a food source.
    /// </summary>
    FollowingFoodPheromone = 3,
    /// <summary>
    /// The ant follows established path to the nest so it can leave the food there.
    /// </summary>
    //FollowingHomePheromone = 4,

    SearchingForNest = 5,
    /// <summary>
    /// Sensors have detected food and we are going towards it.
    /// </summary>
    GoingTowardsFood = 4,
}

public struct Ant
{
    public Vector2 position;
    public Vector2 targetPosition = Vector2.zero;

    public Vector2 orientation;

    /// <summary>
    /// When was the last random rotation while searching for food.
    /// </summary>
    public float lastRandomRotation = 0;
    public readonly float randomRotationFrequency;

    /// <summary>
    /// When was the last time that we checked for pheromone.
    /// </summary>
    public float lastPheromoneCheck = 0;
    public readonly float pheromoneCheckFrequency;

    public float movementSpeed;
    public readonly Vector2 Forward => (position + orientation).normalized;

    /// <summary>
    /// The general direction to the nest.
    /// </summary>
    public Vector2 nestVector = Vector2.zero;
    /// <summary>
    /// The position of the nest. Its used to get the direction of the nest and never directly for path finding except when the ant is rigth next to the nest.
    /// Its equal to the position in the beginning, because the nest is the thing that "spawns" the ant.
    /// </summary>
    public readonly Vector2 nestPosition;
    public readonly Vector2 NestDirection => (nestPosition - position).normalized;
    /// <summary>
    /// The distance to the nest (not exact distance).
    /// </summary>
    public readonly float NestDistance => (nestPosition - position).sqrMagnitude;
    /// <summary>
    /// How confident the ant is in its estimate of the nest direction.
    /// </summary>
    public float nestDirectionConfidence = 1f;
    public float distanceTravelledFromNest = 0f;
    public float maxReliableDistance = 1000f;

    public AntSensor[] sensors;

    public AntState state = AntState.None;
    public bool haveFood = false;
    public float foodMemoryStrength = 0;

    public bool foundFoodSource = false;
    public Vector2 foodSourcePosition = Vector2.zero;

    public bool foundFoodPheromone = false;
    public Vector2 foodPheromonePosition = Vector2.zero;
    public PheromoneSetting foodPheromoneSettings;


    /// <summary>
    /// The number of blocks around the ant's sensor that are gonna be searched when searching for pheromones.
    /// Can be changed to FOV in the future.
    /// </summary>
    public readonly int sensorRadius = 5;

    // Metrics
    public int id;
    public int age = 0;
    public int tripsCompleted = 0;
    public float totalDistanceTraveled = 0;

    public Ant(int id, Vector2 pos, Vector2 orientation)
    {
        this.id = id;

        this.position = pos;
        this.orientation = orientation;

        this.nestPosition = pos;

        movementSpeed = Random.Range(6, 20);
        float halfSpeed = movementSpeed / 2;
        randomRotationFrequency = Random.Range(2 / halfSpeed, 4 / halfSpeed);
        pheromoneCheckFrequency = 0.4f / halfSpeed;

        this.sensors = new AntSensor[2];
        sensors[0] = new AntSensor(pos, orientation, 30, sensorRadius);
        sensors[1] = new AntSensor(pos, orientation, -30, sensorRadius);
        
        foodPheromoneSettings = new PheromoneSetting(10);

        // Might add different genome for each ant and so different properties for each pheromone of each ant in the future.
    }

    public void Move(Vector2 newPos)
    {
        totalDistanceTraveled += (position - newPos).magnitude;
        position = newPos;

        foreach (var sensor in sensors)
        {
            sensor.Update(position, orientation);
        }
    }

    public void CalculateConfidence()
    {
        nestDirectionConfidence = Mathf.Clamp01(1f - distanceTravelledFromNest / maxReliableDistance);
    }

    public void ResetNestInfo()
    {
        nestDirectionConfidence = 1f;
        distanceTravelledFromNest = 0f;
    }

    public void CheckSensors()
    {
        if (Time.time - lastPheromoneCheck < pheromoneCheckFrequency)
        {
            foundFoodPheromone = false;
            return;
        }

        lastPheromoneCheck = Time.time;

        (foodSourcePosition, foodPheromonePosition) = AntSensor.CheckForPheromones(sensors);

        /// Check for food source
        foundFoodSource = foodSourcePosition != Vector2.zero;

        /// Check for food pheromones
        foundFoodPheromone = foodPheromonePosition != Vector2.zero;
    }
}
