using System;

using UnityEngine;
using UnityEngine.InputSystem;

using static GameManager;

public enum Placable
{
    None, Food, Wall, Nest
}

public class InteractionManager : MonoBehaviour
{
    private Placable placableItem = Placable.None;

    private float lastDragPlace = 0;

    [SerializeField]
    private float timeBetweenDragPlaceing = 0.002f;

    [SerializeField]
    private int brushRadius = 1;

    private static InteractionManager instance = null;
    public static InteractionManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = FindAnyObjectByType<InteractionManager>();

        if (instance.gameObject != gameObject)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        SelectPlacableItem();
        Interact();
    }

    private void SelectPlacableItem()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            placableItem = Placable.Food;
        }
        else if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            placableItem = Placable.Wall;
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            placableItem = Placable.Nest;
        }
    }

    private void Interact()
    {
        Dragable();
        NonDragable();
    }

    private void Dragable()
    {
        if (Mouse.current.leftButton.isPressed &&
            Time.time - lastDragPlace > timeBetweenDragPlaceing)
        {
            switch (placableItem)
            {
                case Placable.None:
                    break;
                case Placable.Food:
                    PlaceFood();
                    break;
                case Placable.Wall:
                    PlaceWall();
                    break;
                default:
                    break;
            }

            lastDragPlace = Time.time;
        }

        if (Mouse.current.rightButton.isPressed)
        {
            switch (placableItem)
            {
                case Placable.None:
                    break;
                case Placable.Food:
                    DeleteFood();
                    break;
                case Placable.Wall:
                    DeleteWall();
                    break;
                default:
                    break;
            }
        }
    }

    private void NonDragable()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            switch (placableItem)
            {
                case Placable.None:
                    break;
                case Placable.Nest:
                    PlaceNest();
                    break;
                default:
                    break;
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            switch (placableItem)
            {
                case Placable.None:
                    break;
                case Placable.Nest:
                    DeleteNest();
                    break;
                default:
                    break;
            }
        }
    }

    private void PlaceFood()
    {
        Vector2 position = GetMousePosition();
        int amount = 1;
        FoodSourceManager.Instance.AddFoodAt(position, brushRadius, amount);
    }

    private void PlaceWall()
    {
        Vector2 position = GetMousePosition();
        ObstacleManager.Instance.AddObstacleAt(position, brushRadius);
    }

    private void PlaceNest()
    {
        Vector2 position = GetMousePosition();
        AntManager.Instance.AddNestAt(position);
    }

    private void DeleteFood()
    {
        Vector2 position = GetMousePosition();
        FoodSourceManager.Instance.DeleteFoodAt(position, brushRadius);
    }

    private void DeleteWall()
    {
        Vector2 position = GetMousePosition();
        ObstacleManager.Instance.DeleteObstacleAt(position, brushRadius);
    }

    private void DeleteNest()
    {
        Vector2 position = GetMousePosition();
        AntManager.Instance.DeleteNestAt(position, brushRadius);
    }


    private Vector2 GetMousePosition()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z)
        );

        return worldPosition;
    }
}
