using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public Renderer objectRenderer;   // Reference to the Renderer component of the object
    public Color newColor = Color.red; // The new color to change to

    void Start()
    {
        // Ensure the Renderer component is assigned
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }

        Debug.Log("ChangeColorOnTrigger script initialized. Waiting for trigger to change color.");
    }

    void OnTriggerEnter(Collider other)
    {
        // Change the material color only when the object enters the trigger
        if (objectRenderer != null)
        {
            objectRenderer.material.color = newColor;
            Debug.Log("Object color changed to: " + newColor);
        }
        else
        {
            Debug.LogWarning("Renderer component not assigned.");
        }
    }
}
