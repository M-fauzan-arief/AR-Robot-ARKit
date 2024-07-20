using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClawController : MonoBehaviour
{
    public Button grabButton;           // Reference to the UI Button
    public Transform snapPosition;      // Reference to the Transform for snapping the object
    public TextMeshProUGUI buttonText;  // Reference to the TextMeshPro component for the button text

    [HideInInspector]
    public GameObject grabbedObject = null;
    [HideInInspector]
    public bool isHolding = false;     // Indicates if the claw is holding an object

    private Arm_Controller armController;

    void Start()
    {
        armController = FindObjectOfType<Arm_Controller>();

        // Ensure the button's onClick event calls the HandleGrabButton method
        if (grabButton != null)
        {
            grabButton.onClick.AddListener(HandleGrabButton);
        }
        Debug.Log("ClawController initialized. Waiting for grab button press.");

        if (buttonText != null)
        {
            buttonText.text = "GRAB"; // Initialize the button text
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has the tag "Grabbable"
        if (other.CompareTag("Grabbable") && !isHolding)
        {
            // Assign the object entering the trigger as the object that can be grabbed
            grabbedObject = other.gameObject;
            Debug.Log("Grabbable object entered trigger: " + grabbedObject.name);

            // Verify tag detection
            Debug.Log("Object with 'Grabbable' tag detected: " + grabbedObject.name);
        }
        else
        {
            // If the object does not have the "Grabbable" tag, log a debug message
            Debug.Log("Non-grabbable object entered trigger: " + other.gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the object exiting the trigger is the object that can be grabbed
        if (grabbedObject != null && other.gameObject == grabbedObject && !isHolding)
        {
            Debug.Log("Grabbable object exited trigger: " + grabbedObject.name);
            grabbedObject = null;  // Reset the object that can be grabbed
        }
    }

    private void HandleGrabButton()
    {
        Debug.Log("Grab button pressed.");
        if (isHolding)
        {
            // If an object is being held, release it
            ReleaseObject();
        }
        else if (grabbedObject != null)
        {
            // If there is an object in the trigger, grab it
            GrabObject();
        }
    }

    private void GrabObject()
    {
        // Snap the object's position to the snapPosition Transform
        if (snapPosition != null)
        {
            grabbedObject.transform.SetParent(snapPosition);
            grabbedObject.transform.localPosition = Vector3.zero;
            grabbedObject.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("Snap position not assigned in the editor.");
        }

        // Disable physics so the object follows the snap position
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        isHolding = true; // Update status to indicate the claw is holding an object
        Debug.Log("Object grabbed: " + grabbedObject.name);

        // Update the button text
        if (buttonText != null)
        {
            buttonText.text = "RELEASE";
        }

        // Notify the Arm_Controller
        if (armController != null)
        {
            armController.SetEndEffectorState(isHolding);
        }
    }

    private void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            // Reset the object's parent
            grabbedObject.transform.SetParent(null);
            // Enable physics so the object can be released
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            Debug.Log("Object released: " + grabbedObject.name);
            isHolding = false;  // Update status to indicate the claw is not holding an object anymore
            grabbedObject = null; // Reset the object being held

            // Update the button text
            if (buttonText != null)
            {
                buttonText.text = "Grab";
            }

            // Notify the Arm_Controller
            if (armController != null)
            {
                armController.SetEndEffectorState(isHolding);
            }
        }
    }
}