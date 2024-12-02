using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AxisControl : MonoBehaviour
{
    // Buttons for controlling movement along each axis
    [SerializeField] private Button increaseXButton;
    [SerializeField] private Button decreaseXButton;
    [SerializeField] private Button increaseYButton;
    [SerializeField] private Button decreaseYButton;
    [SerializeField] private Button increaseZButton;
    [SerializeField] private Button decreaseZButton;

    // The object you want to move
    [SerializeField] private GameObject targetObject;

    // Sensitivity factor to control the speed of movement
    [SerializeField] private float sensitivity = 0.1f;

    // Movement direction flags for each axis (X, Y, Z)
    private bool isMovingX = false;
    private bool isMovingY = false;
    private bool isMovingZ = false;

    void Start()
    {
        // Set up EventTriggers for holding buttons
        AddButtonListeners();
    }

    private void AddButtonListeners()
    {
        // Add listeners for increasing and decreasing the X axis movement
        AddButtonListener(increaseXButton, "X", 1f);
        AddButtonListener(decreaseXButton, "X", -1f);

        // Add listeners for increasing and decreasing the Y axis movement
        AddButtonListener(increaseYButton, "Y", 1f);
        AddButtonListener(decreaseYButton, "Y", -1f);

        // Add listeners for increasing and decreasing the Z axis movement
        AddButtonListener(increaseZButton, "Z", 1f);
        AddButtonListener(decreaseZButton, "Z", -1f);
    }

    private void AddButtonListener(Button button, string axis, float direction)
    {
        if (button == null) return;

        // Use the EventTrigger to detect press and hold
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        // Create a new Entry for Pointer Down (when button is pressed)
        EventTrigger.Entry pointerDown = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        pointerDown.callback.AddListener((data) => OnButtonPressed(axis, direction));

        // Create a new Entry for Pointer Up (when button is released)
        EventTrigger.Entry pointerUp = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        pointerUp.callback.AddListener((data) => OnButtonReleased(axis));

        // Add the entries to the EventTrigger
        trigger.triggers.Add(pointerDown);
        trigger.triggers.Add(pointerUp);
    }

    private void OnButtonPressed(string axis, float direction)
    {
        // Set the appropriate flag based on the axis and direction
        if (axis == "X") isMovingX = true;
        if (axis == "Y") isMovingY = true;
        if (axis == "Z") isMovingZ = true;

        // Start movement when button is pressed down
        MoveObject(axis, direction);
    }

    private void OnButtonReleased(string axis)
    {
        // Stop movement when button is released
        if (axis == "X") isMovingX = false;
        if (axis == "Y") isMovingY = false;
        if (axis == "Z") isMovingZ = false;
    }

    private void Update()
    {
        // Move the object continuously based on the axis being held
        if (isMovingX) MoveObject("X", sensitivity);
        if (isMovingY) MoveObject("Y", sensitivity);
        if (isMovingZ) MoveObject("Z", sensitivity);
    }

    private void MoveObject(string axis, float direction)
    {
        Vector3 position = targetObject.transform.position;

        switch (axis)
        {
            case "X":
                position.x += direction * sensitivity * Time.deltaTime;
                break;
            case "Y":
                position.y += direction * sensitivity * Time.deltaTime;
                break;
            case "Z":
                position.z += direction * sensitivity * Time.deltaTime;
                break;
        }

        targetObject.transform.position = position;

        // Debug: Log when object is moving
        Debug.Log($"Object is moving along {axis} axis: {position}");
    }
}
