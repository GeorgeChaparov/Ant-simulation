using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class AntManager : MonoBehaviour
{
    private List<AntNest> Nests = new List<AntNest>();

    private GameManager gameManager = null;

    private static AntManager antManager = null;
    public static AntManager GetAntManager { get { return antManager; } }

    private void Awake()
    {
        antManager = FindAnyObjectByType<AntManager>();

        if (antManager.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameManager = GameManager.GetGameManager; 
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAntsPosition();
    }


    private void UpdateAntsPosition()
    {
        for (int i = 0; i < Nests.Count; i++)
        {
            AntNest Nest = Nests[i];

            for (int j = 0; j < Nest.Ants.Count; j++)
            {
                Ant ant = Nest.Ants[j];

                ChooseAntState(ant);
                MoveAnt(ant);
            }
        }
    }

    private void ChooseAntState(Ant ant)
    {
        if (ant.haveFood)
        {
            bool haveFoodPheromone;
        }

        // If we dont know where the food is and there is not pheromone path to follow - AntState.SearchingForFood

        // If we have food but we dont know where the nest is - AntState.SearchingForNest

        // If we dont have food but we have pheromone path to follow - AntState.FollowingFoodPheromone

        // If we have food and have pheromone path to follow - AntState.FollowingHomePheromone

        // If we dont have food, but we have pheromone path to follow and we need to go back to the nest - AntState.GoingToTheNest
    }

    private void MoveAnt(Ant ant)
    {
        switch (ant.state)
        {
            case AntState.None:
                break;
            case AntState.SearchingForFood:
                // Go in a random direction for a little then change direction with a few degrees and go for a little.
                // Continue until a food is found or a pheromone for food is found.
                break;
            case AntState.SearchingForNest:
                // If we have pheromone path to the nest, we follow it.
                // If not, we go for a little in the general direction of the nest.
                // After that, if the general direction of the nest is still the same, we change direction with a few degrees and go for a little again.
                // If the general direction change, we change direction to match it.
                // Repeat until ether we find the nest or a pheromone path that goes to it.
                break;
            case AntState.GoingToTheNest:
                FollowPheromones(ant);
                break;
            case AntState.FollowingFoodPheromone:
                FollowPheromones(ant);
                break;
            case AntState.FollowingHomePheromone:
                FollowPheromones(ant);
                break;
            default:
                break;
        }
    }

    private void FollowPheromones(Ant ant)
    {
        // Follow the pheromones to the objective.
    }

    public void AddNest()
    {
        Nests.Add(new AntNest());
    }

    public void RemoveNest(int nestIndex)
    {
        Nests.RemoveAt(nestIndex);
    }
}
