using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class AntManager : MonoBehaviour
{

    public delegate void NestAdded();
    public event NestAdded OnNestAdded;
    [SerializeField]
    private List<AntNest> nests = new List<AntNest>();

    public List<AntNest> Nests { get { return nests; } }

    private PheromoneManager pheromoneManager = null;

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
        pheromoneManager = GameManager.GetGameManager.PheromoneManager; 
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnts();
    }

    private void UpdateAnts()
    {
        for (int i = 0; i < nests.Count; i++)
        {
            AntNest nest = nests[i];

            for (int j = 0; j < nest.Ants.Count; j++)
            {
                Ant ant = nest.Ants[j];

                ant = ChooseAntState(ant);
                ant = ChooseOrientation(ant);
                ant.Move();

                nest.Ants[j] = ant;
            }
        }
    }

    private Ant ChooseAntState(Ant ant)
    {
        // We have found food.
        if (ant.haveFood)
        {
            ant.state = AntState.GoingToTheNest;
        }
        //  We have not found food yet.
        else
        {
            ant.targetPheromonePosition = pheromoneManager.GetStrongestPheromonePos(ant.position, ant.orientation, PheromoneType.Food);

            // There is no food pheromon to follow
            if (ant.targetPheromonePosition == Vector2.zero)
            {
                // If we dont know where the food is and there is not pheromone path to follow - AntState.SearchingForFood
                ant.state = AntState.SearchingForFood;
            }
            else
            {
                // If we dont have food but we have pheromone path to follow - AntState.FollowingFoodPheromone
                ant.state = AntState.FollowingFoodPheromone;
            }
        }
        return ant;
    }

    private Ant ChooseOrientation(Ant ant)
    {
        switch (ant.state)
        {
            case AntState.None:
                break;
            case AntState.SearchingForFood:
                // Go in a random direction for a little then change direction with a few degrees and go for a little.
                Vector2 newOrientation = ant.orientation;

                if (Time.time - ant.lastRandomRotation > ant.randomRotationfrequency)
                {
                    ant.lastRandomRotation = Time.time;
                    float rotDeg = Random.Range(10, 45);
                    float ran = Random.value;
                    if (ran >= 0.5)
                    {
                        rotDeg = -rotDeg;
                    }

                    newOrientation = Utils.Rotate(newOrientation, rotDeg);
                }

                ant.orientation = newOrientation;
                break;
            //case AntState.SearchingForNest:
            //    // If we have pheromone path to the nest, we follow it.
            //    // If not, we go for a little in the general direction of the nest.
            //    // After that, if the general direction of the nest is still the same, we change direction with a few degrees and go for a little again.
            //    // If the general direction change, we change direction to match it.
            //    // Repeat until ether we find the nest or a pheromone path that goes to it.
            //    break;
            case AntState.GoingToTheNest:
                
                break;
            case AntState.FollowingFoodPheromone:
                Vector2 pheromoneDirection = ant.targetPheromonePosition - ant.position;
                ant.position = pheromoneDirection * ant.movementSpeed * Time.deltaTime;
                break;
            //case AntState.FollowingHomePheromone:
            //    FollowPheromones(ant);
            //    break;
            default:
                break;
        }

        return ant;
    }

    public void AddNest(AntNest nest)
    {
        nests.Add(nest);
        OnNestAdded();
    }

    public void RemoveNest(int nestIndex)
    {
        nests.RemoveAt(nestIndex);
    }
}
