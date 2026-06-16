using NUnit.Framework.Constraints;

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
    CarryingFoodToNest = 4,
    /// <summary>
    /// The ant does not follow established path to the nest. Its "lost".
    /// </summary>
    SearchingForNest = 5,
}

public struct Ant
{
    public Vector2 position;
    public float orientation;
    public Directions nestDirection;
    public AntState state;

    // Metrics
    public int id;
    public int age;
    public int tripsCompleted;
    public int distanceTraveled;
}
