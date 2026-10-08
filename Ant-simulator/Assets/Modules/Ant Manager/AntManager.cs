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
        Vector2 movement = (ant.position - lastPos);
        ant.distanceTravelledFromNest += movement.magnitude;
        ant.nestVector -= movement + new Vector2(Random.Range(0f, 0.0002f), Random.Range(0f, 0.0002f));

        if (ant.state == AntState.SearchingForNest)
        {
            return ant;
        }

        ant.CalculateConfidence();
        return ant;
    }

    private Ant ChooseAntState(Ant ant)
    {
        // We have food.
        if (ant.haveFood)
        {
            PheromoneManager.Instance.DepositPheromoneOn(ant.id, ant.position, ant.foodPheromoneSettings, PheromoneType.Food);

            if (ant.state == AntState.GoingToTheNest)
            {
                if (ant.NestDistance <= 3)
                {
                    ant.haveFood = false;
                    ant.orientation = -ant.orientation;
                    ant.ResetNestInfo();

                    ant.state = AntState.SearchingForFood;
                }
            }
            else
            {
                ant.state = AntState.SearchingForNest;

                if (ant.NestDistance <= 100)
                {
                    ant.state = AntState.GoingToTheNest;
                }
            }
        }
        // We do not have food yet.
        else
        {
            if (ant.state == AntState.GoingTowardsFood)
            {
                if ((ant.position - ant.foodSourcePosition).sqrMagnitude < 1 )
                {
                    if (FoodSourceManager.Instance.RemoveFoodAt(ant.foodSourcePosition, 1))
                    {
                        ant.haveFood = true;
                    }
                    else
                    {
                        ant.state = AntState.SearchingForFood;
                    }
                }
                return ant;
            }

            ant.CheckSensors();

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
             case AntState.SearchingForNest:
                float randomAmount = 1f - ant.nestDirectionConfidence;
                ant.orientation = (ant.nestVector + new Vector2(-randomAmount, randomAmount)).normalized;
                break;
            case AntState.GoingToTheNest:
                ant.orientation = ant.NestDirection;
                break;
            case AntState.FollowingFoodPheromone:
                Vector2 pheromoneDirection = ant.targetPosition - ant.position;
                ant.orientation = pheromoneDirection.normalized;
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
