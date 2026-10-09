using UnityEngine;
public class AntSensor
{
    Vector2 position;
    public Vector2 Position => position;

    Vector2 orientation;

    int radius = 3;
    public int Radius => radius;

    float angle = 30;
    float sensorsDistance = 3;

    /// <summary>
    /// The sensor the ant uses to check for food and pheromones.
    /// </summary>
    /// <param name="antPos"></param>
    /// <param name="antOrientation"></param>
    /// <param name="angle">The angle for the offset on the sensor from the forward direction of the ant.</param>
    /// <param name="sensorsDistance">How far the sensor should be from the ant.</param>
    /// <param name="radius">The radius in which the sensor will detect.</param>
    public AntSensor(Vector2 antPos, Vector2 antOrientation, float angle, float sensorsDistance, int radius = 3) 
    {
        orientation = Utils.Rotate(antOrientation, angle);
        position = antPos + orientation * sensorsDistance;

        this.angle = angle;
        this.sensorsDistance = sensorsDistance;
        this.radius = radius;
    }

    /// <summary>
    /// Updates the position of the sensor.
    /// </summary>
    /// <param name="antPos"></param>
    /// <param name="antOrientation"></param>
    public void Update(Vector2 antPos, Vector2 antOrientation)
    {
        orientation = Utils.Rotate(antOrientation, angle);
        position = antPos + orientation * sensorsDistance;
    }

    public static (Vector2 foodSourcePosition, Vector2 foodPheromonePosition) Check(AntSensor[] sensors)
    {
        Vector2 foodSourcePosition = FoodSourceManager.Instance.GetFoodInRadius(sensors);
        Vector2 foodPheromonePosition = PheromoneManager.Instance.GetWeakestPheromonePos(sensors);

        return (foodSourcePosition, foodPheromonePosition);
    }
}
