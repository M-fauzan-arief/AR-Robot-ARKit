using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectGrabber : MonoBehaviour
{
    [SerializeField] private Transform gripper;
    [SerializeField] private Transform objectToGrab;
    [SerializeField] private Button grabButton;
    [SerializeField] private string grabButtonName = "GrabButton";

    private bool isGrabbing = false;
    private bool canGrab = true;
    private bool isInRange = false;

    void Start()
    {
        // Find the grab button by name
        grabButton = GameObject.Find(grabButtonName).GetComponent<Button>();

        // Add listener to the grab button
        grabButton.onClick.AddListener(ToggleGrab);
    }

    void Update()
    {
        // Check if object is in range and grab is enabled
        if (isInRange && canGrab && Input.GetMouseButtonDown(0))
        {
            ToggleGrab();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == objectToGrab)
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == objectToGrab)
        {
            isInRange = false;
        }
    }

    private void ToggleGrab()
    {
        if (!isGrabbing)
        {
            if (isInRange)
            {
                // Check lock status from ObjectPlacementManager
                ObjectPlacementManager placementManager = FindObjectOfType<ObjectPlacementManager>();
                if (placementManager != null && placementManager.IsObjectLocked(objectToGrab.gameObject))
                {
                    Debug.Log(objectToGrab.name + " is locked and cannot be grabbed.");
                    return;
                }

                // Grab the object
                objectToGrab.SetParent(gripper);
                objectToGrab.localPosition = Vector3.zero;
                objectToGrab.localRotation = Quaternion.identity;
                isGrabbing = true;
                canGrab = false; // Disable grabbing until release
            }
        }
        else
        {
            // Release the object
            objectToGrab.SetParent(null);
            isGrabbing = false;
            canGrab = true; // Enable grabbing again
        }
    }
}
