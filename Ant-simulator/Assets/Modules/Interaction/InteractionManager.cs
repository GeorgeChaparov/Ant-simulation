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

    private bool isMousePressedDown = false;
    private bool isMousePressed = false;
    private bool isMousePressedUp = false;

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
        GetMousePress();
        PlaceItem();
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

    private void GetMousePress()
    {
        isMousePressedDown = Mouse.current.leftButton.wasPressedThisFrame;
        isMousePressed = Mouse.current.leftButton.isPressed;
        isMousePressedUp = Mouse.current.leftButton.wasReleasedThisFrame;
    }

    private void PlaceItem()
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
            case Placable.Nest:
                PlaceNest();
                break;
            default:
                break;
        }
    }

    private void PlaceFood()
    {
        if (!isMousePressed || Time.time - lastDragPlace < timeBetweenDragPlaceing)
        {
            return;
        }

        lastDragPlace = Time.time;

        Vector2 position = GetMousePosition();
        int amount = 1;
        FoodSourceManager.Instance.AddFoodAt(position, brushRadius, amount);
    }

    private void PlaceWall()
    {
        if (!isMousePressed || Time.time - lastDragPlace < timeBetweenDragPlaceing)
        {
            return;
        }

        lastDragPlace = Time.time;

        Vector2 position = GetMousePosition();
        //ObstacleManager.Instance.AddWallAt(position);
    }

    private void PlaceNest()
    {
        if (!isMousePressedDown)
        {
            return;
        }
        Vector2 position = GetMousePosition();
        AntManager.Instance.AddNestAt(position);
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
