using System.Collections.Generic;
using System;
using UnityEngine;

public class AntNest : MonoBehaviour
{
    private readonly List<Ant> ants;

    private int maxAnts = 10;

    private float spawnRate = 1.0f;

    private float lastSpawn = 0;

    private bool canSpawn = true;

    public List<Ant> Ants { get { return ants; } }
    public bool CanSpawn { get { return canSpawn; } }

    public int MaxAnts { get { return maxAnts; } set { maxAnts = value; } }
    public float SpawnRate { get { return spawnRate; } set { spawnRate = value; } }


    private void Update()
    {
        UpdateCanSpawn();
    }

    private void UpdateCanSpawn()
    {
        if (!canSpawn) { return; }

        float currTime = Time.deltaTime;

        if (currTime - lastSpawn < spawnRate) { return; }

        lastSpawn = currTime;
    }

    public void SpawnAnt()
    {
        ants.Add(new Ant());
    }

    public void RemoveAnt(int antIndex)
    {
        if (ants.Count >= antIndex)
        {
            throw new Exception("Ant index does not exists!");
        }

        ants.RemoveAt(antIndex);
        canSpawn = true;
    }
}
