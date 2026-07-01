using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PheromoneManager PheromoneManager { get; }

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

    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
