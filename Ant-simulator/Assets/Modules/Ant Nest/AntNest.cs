using System.Collections.Generic;
using UnityEngine;

public class AntNest : MonoBehaviour
{
    [SerializeField]
    private List<Ant> ants = new List<Ant>();

    [SerializeField]
    private int maxAnts = 10;

    [SerializeField]
    private float spawnRate = 1.0f;

    private float lastSpawn = 0;

    public List<Ant> Ants { get { return ants; } }
    public int MaxAnts { get { return maxAnts; } set { maxAnts = value; } }
    public float SpawnRate { get { return spawnRate; } set { spawnRate = value; } }


    private void Start()
    {
        lastSpawn = spawnRate;
    }

    private void Update()
    {
        if (MaxAnts == ants.Count)
        {
            return;
        }

        if (Time.time - lastSpawn < SpawnRate)
        {
            return;
        }

        lastSpawn = Time.time;
        SpawnAnt(new Ant(0, transform.position, new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized));
    }

    public void SpawnAnt(Ant ant)
    {
        ants.Add(ant);
    }

    public void RemoveAnt(int antIndex)
    {
        if (ants.Count >= antIndex)
        {
            throw new System.Exception("Ant index does not exists!");
        }

        ants.RemoveAt(antIndex);
    }
}
