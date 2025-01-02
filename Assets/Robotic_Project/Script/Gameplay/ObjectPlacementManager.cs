using UnityEngine;
using TMPro;

public class ObjectPlacementManager : MonoBehaviour
{
    public GameObject assignedObject;      // The correct object for this trigger
    public Transform target;               // Target position for the assigned object
    public TextMeshProUGUI feedbackText;   // UI Text for feedback
    public ScoreManager scoreManager;      // Reference to ScoreManager script
    public int objectPlaced = 0;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip correctSound; // Sound for correct placement
    [SerializeField] private AudioClip wrongSound;   // Sound for incorrect placement
    private AudioSource audioSource;                // AudioSource for playing sounds

    private void Start()
    {
        feedbackText.text = "";
        scoreManager = FindObjectOfType<ScoreManager>(); // Find ScoreManager script in the scene

        // Ensure AudioSource exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == assignedObject)
        {
            // Correct object entered the trigger
            PlaySound(correctSound);
            TeleportAndColorObject(other.gameObject, target.position, Color.green);
            feedbackText.text = assignedObject.name + " placed correctly!";
            Debug.Log(assignedObject.name + " placed correctly!");
            scoreManager.UpdateScore(); // Notify ScoreManager to update score
            scoreManager.UpdateObjectPlace();
        }
        else
        {
            // Incorrect object entered the trigger
            PlaySound(wrongSound);
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

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioClip or AudioSource is missing!");
        }
    }
}
