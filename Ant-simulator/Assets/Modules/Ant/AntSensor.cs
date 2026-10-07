using UnityEngine;
using UnityEngine.UIElements;

public class AntSensor
{
    Vector2 position;

    public Vector2 Position => position;
    Vector2 orientation;
    int radius = 3;
    public int Radius => radius;
    float angle = 30;
    float sensorsDistance = 3;

    public AntSensor(Vector2 antPos, Vector2 antOrientation, float angle, float sensorsDistance, int radius = 3) 
    {
        orientation = Utils.Rotate(antOrientation, angle);
        position = antPos + orientation * sensorsDistance;

        this.angle = angle;
        this.sensorsDistance = sensorsDistance;
        this.radius = radius;
    }

    public void Update(Vector2 antPos, Vector2 antOrientation)
    {
        orientation = Utils.Rotate(antOrientation, angle);
        position = antPos + orientation * sensorsDistance;
    }
}
