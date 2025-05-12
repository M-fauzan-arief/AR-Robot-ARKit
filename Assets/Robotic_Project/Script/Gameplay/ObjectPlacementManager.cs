using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ObjectPlacementManager : MonoBehaviour
{
    public GameObject assignedObject;
    public Transform target;
    public TextMeshProUGUI feedbackText;
    public ScoreManager scoreManager;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    private AudioSource audioSource;

    private Dictionary<GameObject, bool> lockedObjects = new Dictionary<GameObject, bool>();

    private void Start()
    {
        feedbackText.text = "";
        scoreManager = FindObjectOfType<ScoreManager>();

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
            // Check if already placed
            if (IsObjectLocked(assignedObject))
            {
                feedbackText.text = assignedObject.name + " has already been placed!";
                Debug.Log(assignedObject.name + " has already been placed!");
                return;
            }

            // Check if still being held
            if (ClawController.Instance != null && ClawController.Instance.grabbedObject == assignedObject)
            {
                feedbackText.text = assignedObject.name + " is still being held!";
                Debug.Log(assignedObject.name + " is still being held!");
                return;
            }

            // Valid placement
            PlaySound(correctSound);
            TeleportAndColorObject(assignedObject, target.position, Color.green);
            feedbackText.text = assignedObject.name + " placed correctly!";
            Debug.Log(assignedObject.name + " placed correctly!");
            scoreManager.UpdateScore();
            scoreManager.UpdateObjectPlace();

            lockedObjects[assignedObject] = true;
        }
        else
        {
            // Incorrect object
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

    public bool IsObjectLocked(GameObject obj)
    {
        return lockedObjects.ContainsKey(obj) && lockedObjects[obj];
    }
}
