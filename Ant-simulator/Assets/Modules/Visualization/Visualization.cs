using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;

public class Visualization : MonoBehaviour
{
    private GameManager gameManager = null;
    private static Visualization visualization = null;
    public static Visualization GetVisualization { get { return visualization; } }
    private AntNest[] antNests = new AntNest[0];
        
    private void Awake()
    {
        visualization = FindAnyObjectByType<Visualization>();

        if (visualization.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        UpdateNests();
        gameManager.AntManager.OnNestAdded += UpdateNests;
    }

    void FixedUpdate()
    {
        
    }

    private void OnDrawGizmos()
    {
        Draw();
    }

    private void UpdateNests()
    {
        antNests = gameManager.AntManager.Nests.ToArray();
    }

    private void Draw()
    {
        foreach (var nest in antNests)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(nest.transform.position, 5);

            List<Ant> ants = nest.Ants; 
            foreach (var ant in ants)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(ant.position, 1);
            }
        }
    }
}
