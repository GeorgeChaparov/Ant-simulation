using NUnit.Framework.Internal;

using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UIElements;

public class AntManager : MonoBehaviour
{

    public delegate void NestAdded();
    public event NestAdded OnNestAdded;
    [SerializeField]
    private List<AntNest> nests = new List<AntNest>();

    public List<AntNest> Nests { get { return nests; } }

    private static AntManager instance = null;
    public static AntManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = FindAnyObjectByType<AntManager>();

        if (instance.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
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
                Vector2 lastPos = ant.position;

                ant = Move(ant);
                ant = UpdateNestDirAproximatio(ant, lastPos);

                nest.Ants[j] = ant;
            }
        }
    }

    private Ant Move(Ant ant)
    {
        ant = ChooseAntState(ant);
        ant = ChooseOrientation(ant);

        Vector2 direction = ant.movementSpeed * Time.deltaTime * ant.orientation;
        Vector2 newPos = ant.position + direction;

        if (Utils.IsOutOfBounds(newPos))
        {
            newPos = ant.position - direction;
            ant.orientation = -ant.orientation;
        }

        ant.Move(newPos);

        return ant;
    }

    private Ant UpdateNestDirAproximatio(Ant ant, Vector2 lastPos)
    {
        if (ant.state == AntState.GoingToTheNest)
        {
            return ant;
        }

        Vector2 movement = (lastPos - ant.position);
        ant.distanceTraveledFromNest += movement.magnitude;
        ant.nestVector -= movement;

        return ant;
    }

    private Ant ChackSensors(Ant ant)
    {
        /// Check for food source
        ant.foodSourcePosition = FoodSourceManager.Instance.GetFoodInRadius(ant.sensors);
        ant.foundFoodSource = ant.foodSourcePosition != Vector2.zero;

        /// Check for food pheromones
        ant.foodPheromonePosition = PheromoneManager.Instance.GetStrongestPheromonePos(ant.sensors);
        ant.foundFoodPheromone = ant.foodPheromonePosition != Vector2.zero;

        return ant;
    }

    private Ant ChooseAntState(Ant ant)
    {
        // We have food.
        if (ant.haveFood)
        {
            ant.state = AntState.GoingToTheNest;
        }
        //  We do not have food yet.
        else
        {
            if (ant.state == AntState.GoingTowardsFood)
            {
                if ((ant.position - ant.foodSourcePosition).sqrMagnitude < 2 )
                {
                    ant.haveFood = true;
                    FoodSourceManager.Instance.RemoveFoodAt(ant.foodSourcePosition, 1);
                }
                return ant;
            }

            ant = ChackSensors(ant);

            if (ant.foundFoodSource)
            {
                ant.targetPosition = ant.foodSourcePosition;
                ant.state = AntState.GoingTowardsFood;

                return ant;
            }

            if (ant.foundFoodPheromone)
            {
                // We dont have food source, but we have pheromone path to follow - AntState.FollowingFoodPheromone
                ant.targetPosition = ant.foodPheromonePosition;
                ant.state = AntState.FollowingFoodPheromone;

                return ant;
            }

            // We dont have food source or food pheromone to follow - AntState.SearchingForFood
            ant.state = AntState.SearchingForFood;
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

                if (Time.time - ant.lastRandomRotation > ant.randomRotationFrequency)
                {
                    ant.lastRandomRotation = Time.time;
                    float rotDeg = Random.Range(10, 45);
                    float random = Random.value;
                    if (random >= 0.5)
                    {
                        rotDeg = -rotDeg;
                    }

                    newOrientation = Utils.Rotate(newOrientation, rotDeg);
                }

                ant.orientation = newOrientation;
                break;
            case AntState.GoingTowardsFood:
                Vector2 foodDirection = ant.targetPosition - ant.position;

                ant.orientation = foodDirection.normalized;
                break;
            //case AntState.SearchingForNest:
            //    // If we have pheromone path to the nest, we follow it.
            //    // If not, we go for a little in the general direction of the nest.
            //    // After that, if the general direction of the nest is still the same, we change direction with a few degrees and go for a little again.
            //    // If the general direction change, we change direction to match it.
            //    // Repeat until ether we find the nest or a pheromone path that goes to it.
            //    break;
            case AntState.GoingToTheNest:
                int a = 2;
                break;
            case AntState.FollowingFoodPheromone:
                Vector2 pheromoneDirection = ant.targetPosition - ant.position;
                ant.position = ant.movementSpeed * Time.deltaTime * pheromoneDirection;
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
