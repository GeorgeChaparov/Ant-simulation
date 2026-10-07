using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    public int gridWidth = 10;
    [SerializeField]
    public int gridHeight = 10;


    private static GameManager instance = null;
    public static GameManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = FindAnyObjectByType<GameManager>();

        if (instance.gameObject != gameObject)
        {
            Destroy(gameObject);
        }

        PheromoneManager.Instance.Init(gridWidth, gridHeight);
    }

    private void UpdateVizualizerNestList()
    { 
        
    }
}
