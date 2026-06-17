/// <summary>
/// Shows what the pheromone points to.
/// </summary>
public enum PheromoneType
{
    None = 0, Food = 1, Nest = 2
}


public struct Pheromone
{
    /// <summary>
    /// How strong the scent of the pheromone is.
    /// </summary>
    public int baseStrength;

    public PheromoneType type = PheromoneType.None;

    /// <summary>
    /// When was the pheromone secreted.
    /// </summary>
    public float creationTime;

    /// <summary>
    /// How fast should the scent of the pheromone decay.
    /// </summary>
    public float decaySpeed;

    public Pheromone(int baseStrength, float creationTime, float decaySpeed)
    {
        this.baseStrength = baseStrength;
        this.creationTime = creationTime;
        this.decaySpeed = decaySpeed;
    }
}