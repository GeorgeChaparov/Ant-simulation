using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    public int gridWidth = 10;
    [SerializeField]
    public int gridHeight = 10;

    private static PheromoneManager pheromoneManager = null;
    public PheromoneManager PheromoneManager { get { return pheromoneManager; } }

    private static AntManager antManager = null;
    public AntManager AntManager { get { return antManager; } }

    private static GameManager instance = null;
    public static GameManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = FindAnyObjectByType<GameManager>();

        if (instance.gameObject != gameObject)
        {
            Destroy(gameObject);
        }

        pheromoneManager.Init(gridWidth, gridHeight);
    }

    private void OnValidate()
    {
        pheromoneManager = FindAnyObjectByType<PheromoneManager>();
        antManager = FindAnyObjectByType<AntManager>();
    }


    private void UpdateVizualizerNestList()
    { 
        
    }
}
