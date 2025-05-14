using UnityEngine;

public class GripperCollider : MonoBehaviour
{
    [HideInInspector]
    public GameObject currentGrabbable = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            currentGrabbable = other.gameObject;
            Debug.Log("Grabbable entered: " + currentGrabbable.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentGrabbable != null && other.gameObject == currentGrabbable)
        {
            currentGrabbable = null;
            Debug.Log("Grabbable exited: " + other.name);
        }
    }
}
