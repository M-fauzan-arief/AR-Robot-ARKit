using UnityEngine;
using TMPro;

public class ObjectPlacementManager : MonoBehaviour
{
    public GameObject assignedObject;      // The correct object for this trigger
    public Transform target;               // Target position for the assigned object
    public TextMeshProUGUI feedbackText;   // UI Text for feedback
    public ScoreManager scoreManager;      // Reference to ScoreManager script
    public int objectPlaced = 0;

    private void Start()
    {
        feedbackText.text = "";
        scoreManager = FindObjectOfType<ScoreManager>(); // Find ScoreManager script in the scene
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == assignedObject)
        {
            // Correct object entered the trigger
            TeleportAndColorObject(other.gameObject, target.position, Color.green);
            feedbackText.text = assignedObject.name + " placed correctly!";
            Debug.Log(assignedObject.name + " placed correctly!");
            scoreManager.UpdateScore(); // Notify ScoreManager to update score
            scoreManager.UpdateObjectPlace();
        }
        else
        {
            // Incorrect object entered the trigger
            TeleportAndColorObject(other.gameObject, target.position, Color.red);
            feedbackText.text = other.gameObject.name + " placed incorrectly!";
            Debug.Log(other.gameObject.name + " placed incorrectly!");
            scoreManager.UpdateObjectPlace();
        }
    }

    private void TeleportAndColorObject(GameObject obj, Vector3 position, Color color)
    {
        obj.transform.position = position;
        obj.GetComponent<Renderer>().material.color = color;
    }
}
