using UnityEngine;

public class FoodSource: MonoBehaviour
{
    Vector2 position;
    public Vector2 Position => position;
    [SerializeField]
    int radius = 2;
    public int Radius => radius;
    [SerializeField]
    int amount = 5;
    public int Amount => amount;

    public FoodSource(Vector2 position, int radius = 2, int amount = 5)
    {
        this.position = position;
        this.radius = radius;
        this.amount = amount;
    }

    private void Start()
    {
        position = transform.position;
    }
}
