using UnityEngine.UIElements;

/// <summary>
/// Shows what the pheromone points to.
/// </summary>
public enum PheromoneType
{
    None = 0, Food = 1, Nest = 2
}

public struct PheromoneSetting
{
    /// <summary>
    /// How strong is the scent of the pheromone right now.
    /// </summary>
    public float strength;

    /// <summary>
    /// How fast should the scent of the pheromone decay.
    /// </summary>
    public float decaySpeed;


    public PheromoneSetting(float strength, float decaySpeed)
    {
        this.strength = strength;
        this.decaySpeed = decaySpeed;
    }
}