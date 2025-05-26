using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClawController : MonoBehaviour
{
    public Button grabButton;
    public Transform snapPosition;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI gripStatusText; // <-- NEW: for showing grip status
    public GripperCollider gripperCollider;

    [HideInInspector]
    public GameObject grabbedObject = null;
    [HideInInspector]
    public bool isHolding = false;

    private Arm_Controller armController;
    private MQTT_Client mqttClient;

    void Start()
    {
        armController = FindObjectOfType<Arm_Controller>();
        mqttClient = FindObjectOfType<MQTT_Client>();

        if (grabButton != null)
        {
            grabButton.onClick.AddListener(HandleGrabButton);
        }

        if (buttonText != null)
        {
            buttonText.text = "GRAB";
        }

        if (gripStatusText != null)
        {
            gripStatusText.text = "GRIP STATUS: RELEASED";
        }

        if (gripperCollider == null)
        {
            Debug.LogWarning("GripperCollider reference not assigned in ClawController.");
        }
    }

    private void HandleGrabButton()
    {
        Debug.Log("Grab button clicked. isHolding = " + isHolding);

        if (isHolding)
        {
            ReleaseObject();
        }
        else
        {
            // Get current object from collider
            if (gripperCollider != null)
            {
                grabbedObject = gripperCollider.currentGrabbable;
            }
            GrabObject();
        }

        if (armController != null)
        {
            armController.SetEndEffectorState(isHolding);
            armController.SendJointValues();
        }
    }

    private void GrabObject()
    {
        isHolding = true;

        if (grabbedObject != null && snapPosition != null)
        {
            grabbedObject.transform.SetParent(snapPosition);
            grabbedObject.transform.localPosition = Vector3.zero;
            grabbedObject.transform.localRotation = Quaternion.identity;

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            Debug.Log("Object grabbed: " + grabbedObject.name);
        }
        else
        {
            Debug.Log("Grabbed (no object).");
        }

        if (buttonText != null)
        {
            buttonText.text = "RELEASE";
        }

        if (gripStatusText != null)
        {
            gripStatusText.text = "GRIP STATUS: GRIPPING";
        }

        mqttClient.PublishGrabStatus(true);
    }

    private void ReleaseObject()
    {
        isHolding = false;

        if (grabbedObject != null)
        {
            grabbedObject.transform.SetParent(null);

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            Debug.Log("Object released: " + grabbedObject.name);
            grabbedObject = null;
        }
        else
        {
            Debug.Log("Released (no object).");
        }

        if (buttonText != null)
        {
            buttonText.text = "GRAB";
        }

        if (gripStatusText != null)
        {
            gripStatusText.text = "GRIP STATUS: RELEASED";
        }

        mqttClient.PublishGrabStatus(false);
        armController.SetEndEffectorState(isHolding);
        armController.SendJointValues();
    }
}
