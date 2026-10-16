using System.Collections.Generic;
using UnityEngine;

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

                if (ant.depositFoodPheromone)
                {
                    PheromoneManager.Instance.DepositPheromoneOn(ant.id, ant.position, ant.foodPheromoneSettings, PheromoneType.Food);
                }
                ant = UpdateNestDirAproximatio(ant, lastPos);

                nest.Ants[j] = ant;
            }
        }
    }

    private Ant Move(Ant ant)
    {
        ant = ChooseAntState(ant);
        ant = ChooseOrientation(ant);

        // Calculate the new position.
        Vector2 direction = ant.movementSpeed * Time.deltaTime * ant.orientation;
        Vector2 newPos = ant.position + direction;

        // Check if its out of the bounds of the grid.
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
        // The ant "remembers" where the nest is by updating its known position based on all of the moves it has done up to this point.
        // Small random offset is added. This acts as uncertainty. The further the ant goes, the more "uncertain" it becomes.
        Vector2 movement = (ant.position - lastPos);
        ant.distanceTravelledFromNest += movement.magnitude;

        Vector2 random = Random.insideUnitCircle * ant.maxEstimateDeviation;
        ant.nestVector -= movement + random;
        
        if (ant.state == AntState.SearchingForNest)
        {
            return ant;
        }

        ant.CalculateNestPosConfidence();
        return ant;
    }

    private Ant ChooseAntState(Ant ant)
    {
        // We have food.
        if (ant.haveFood)
        {
            // We "see" the nest.
            if (ant.state == AntState.GoingToTheNest)
            {
                if (ant.NestDistance <= 3)
                {
                    // Leave food and reset nest info.
                    ant.haveFood = false;
                    ant.orientation = -ant.orientation;
                    ant.ResetNestInfo();

                    ant.state = AntState.FollowingFoodPheromone;
                }
            }
            // We dont see the nest yet.
            else
            {
                ant.state = AntState.SearchingForNest;

                // We just saw the nest for the first time.
                if (ant.NestDistance <= 100)
                { 
                    ant.state = AntState.GoingToTheNest;
                }
            }
        }
        // We do not have food yet.
        else
        {
            ant.depositFoodPheromone = false;

            // We "see" the food.
            if (ant.state == AntState.GoingTowardsFood)
            {
                // We are close enough to get it.
                if ((ant.position - ant.foodSourcePosition).sqrMagnitude < 1 )
                {
                    // The food is still there by the time we get there.
                    if (FoodSourceManager.Instance.RemoveFoodAt(ant.foodSourcePosition, 1))
                    {
                        ant.haveFood = true;
                    }
                    // The food is gone
                    else
                    {
                        ant.state = AntState.SearchingForFood;
                    }
                }

                return ant;
            }

            // Check for food or pheromones
            ant.CheckSensors();

            // We just saw the food source for the first time.
            if (ant.foundFoodSource)
            {
                ant.targetPosition = ant.foodSourcePosition;
                ant.state = AntState.GoingTowardsFood;

                return ant;
            }

            // We dont have food source, but we have pheromone path to follow.
            if (ant.foundFoodPheromone)
            {
                ant.targetPosition = ant.foodPheromonePosition;
                ant.state = AntState.FollowingFoodPheromone;

                return ant;
            }

            // We dont have food source or food pheromone to follow
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
                    float rotDeg = Random.Range(ant.minRandomRotation, ant.maxRandomRotation);
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
                ant = SearchForNest(ant);
                break;
            case AntState.GoingToTheNest:
                ant.orientation = ant.NestDirection;
                break;
            case AntState.FollowingFoodPheromone:
                ant = FollowFoodPheromone(ant);
                break;
            default:
                break;
        }

        return ant;
    }

    private Ant FollowFoodPheromone(Ant ant)
    {
        Vector2 pheromoneDirection = (ant.targetPosition - ant.position).normalized;

        if (Vector2.Dot(pheromoneDirection, ant.nestVector.normalized) > 0.9f)
        {
            ant.orientation = -pheromoneDirection;
        }
        else
        {
            ant.orientation = pheromoneDirection;
        }

        return ant;
    }

    private Ant SearchForNest(Ant ant)
    {
        if (ant.nestSearchCenter != Vector2.zero)
        {
            ant.depositFoodPheromone = false; 
            return SearchInASpiral(ant);
        }

        ant.depositFoodPheromone = true;
        // We go in the direction where we think the nest is. Based on our confidence in the direction, we explore around us.
        float randomAmount = 1f - ant.nestDirectionConfidence;
        ant.orientation = (ant.nestVector + new Vector2(Random.Range(-randomAmount, randomAmount), Random.Range(-randomAmount, randomAmount))).normalized;

        if (ant.nestVector.sqrMagnitude < 20)
        {
            ant.nestSearchCenter = ant.position;
        }

        return ant;
    }

    private Ant SearchInASpiral(Ant ant)
    {
        ant.nestSearchAngle += ant.nestSearchTurnSpeed * Time.deltaTime;
        ant.nestSearchRadius += ant.nestSearchExpansionSpeed * Time.deltaTime;

        Vector2 target = ant.nestSearchCenter + new Vector2(
            Mathf.Cos(ant.nestSearchAngle),
            Mathf.Sin(ant.nestSearchAngle)
        ) * ant.nestSearchRadius;

        ant.orientation = (target - ant.position).normalized;

        return ant;
    }

    public void AddNest(AntNest nest)
    {
        nests.Add(nest);
        OnNestAdded();
    }

    public void AddNestAt(Vector2 position)
    {
        GameObject newNest = new GameObject("Nest");
        newNest.transform.position = position;
        AntNest antNest = newNest.AddComponent<AntNest>(); 
        nests.Add(antNest);

        OnNestAdded();
    }

    public void RemoveNest(int nestIndex)
    {
        nests.RemoveAt(nestIndex);
    }
}
