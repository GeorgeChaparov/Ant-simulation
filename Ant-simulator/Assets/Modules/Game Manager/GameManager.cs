using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static PheromoneManager pheromoneManager = null;
    public PheromoneManager PheromoneManager { get { return pheromoneManager; } }

    private static AntManager antManager = null;
    public AntManager AntManager { get { return antManager; } }

    private static GameManager gameManager = null;
    public static GameManager GetGameManager { get { return gameManager; } }

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        pheromoneManager = FindAnyObjectByType<PheromoneManager>();
        antManager = FindAnyObjectByType<AntManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void UpdateVizualizerNestList()
    { 
        
    }
}
