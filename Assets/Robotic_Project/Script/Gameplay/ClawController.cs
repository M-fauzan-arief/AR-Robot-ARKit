using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClawController : MonoBehaviour
{
    public Button grabButton;
    public Transform snapPosition;
    public TextMeshProUGUI buttonText;

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
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable") && !isHolding)
        {
            grabbedObject = other.gameObject;
            Debug.Log("Grabbable object entered: " + grabbedObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (grabbedObject != null && other.gameObject == grabbedObject && !isHolding)
        {
            grabbedObject = null;
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
            GrabObject();
        }

        // Update gripper state in Arm_Controller
        if (armController != null)
        {
            armController.SetEndEffectorState(isHolding); // Update gripper state
            armController.SendJointValues(); // Send updated joint and gripper data
        }
    }



    private void GrabObject()
    {
        isHolding = true;

        // If object is in trigger, attach it
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

        if (armController != null)
        {
            armController.SetEndEffectorState(isHolding);
        }

        // Sending the MQTT message is already handled inside PublishGrabStatus
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

        if (armController != null)
        {
            armController.SetEndEffectorState(isHolding);
        }

        // Sending the MQTT message is already handled inside PublishGrabStatus
        mqttClient.PublishGrabStatus(false);
        armController.SetEndEffectorState(isHolding);
        armController.SendJointValues();

    }

}