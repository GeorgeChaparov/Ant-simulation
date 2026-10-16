using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    public int gridWidth = 10;
    [SerializeField]
    public int gridHeight = 10;

    private int currAntID = 1;

    public delegate void Initialized();
    public Initialized OnInitialized;

    private static GameManager instance = null;
    public static GameManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = FindAnyObjectByType<GameManager>();

        if (instance.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        OnInitialized += Visualization.Instance.OnSimulationInitialize;

        // Obstacle needs to be initiated before food.
        ObstacleManager.Instance.Init(gridWidth, gridHeight);
        PheromoneManager.Instance.Init(gridWidth, gridHeight);
        FoodSourceManager.Instance.Init(gridWidth, gridHeight);

        OnInitialized.Invoke();
    }

    public int GetAntID()
    {
        return currAntID++;
    }
}
