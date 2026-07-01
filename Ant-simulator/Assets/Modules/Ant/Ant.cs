using NUnit.Framework;

using UnityEngine;


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
    FollowingHomePheromone = 4,
    /// <summary>
    /// The ant does not follow established path to the nest. Its "lost".
    /// </summary>
    SearchingForNest = 5,
}

public struct Ant
{
    public Vector2 position;
    public Vector2 orientation;
    public float movementSpeed;

    public (PheromoneType , PheromoneSetting)[] pheromones = new (PheromoneType, PheromoneSetting)[2];

    /// <summary>
    /// The general direction to the nest.
    /// </summary>
    public Directions nestDirection = Directions.None;
    /// <summary>
    /// The position of the nest. Its used to get the direction of the nest and never directly for path finding.
    /// Its equal to the position in the beginning, because the nest is the thing that "spawns" the ant.
    /// </summary>
    private readonly Vector2 nestPosition;
    /// <summary>
    /// The distance to the nest (not exact distance).
    /// </summary>
    public readonly float NestDistance => (nestPosition - position).sqrMagnitude;
    /// <summary>
    /// How confident the ant is in its estimate of the nest direction.
    /// </summary>
    public float nestDirectionConfidence = 0.5f;

    public AntState state = AntState.None;
    public int foodAmount = 0;
    public float foodMemoryStrength = 0;


    // Metrics
    public int id;
    public int age = 0;
    public int tripsCompleted = 0;
    public int distanceTraveled = 0;

    public Ant(int id, Vector2 pos, Vector2 orientation)
    {
        this.id = id;

        this.position = pos;
        this.orientation = orientation;

        this.nestPosition = pos;

        this.movementSpeed = Random.value;

        // Might add different genome for each ant and so different proterties for each pheromone of each ant in the future.
    }
}
