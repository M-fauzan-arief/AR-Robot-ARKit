using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class ButtonData
{
    public GameObject buttonObject; // The button GameObject
    public Sprite defaultSprite;    // Default sprite for the button
    public Sprite clickedSprite;    // Sprite when the button is clicked
    public AudioClip clickSound;    // Sound to play when the button is clicked
    [Range(0f, 1f)] public float soundVolume = 1f; // Volume of the click sound
}

public class AnimatedButton : MonoBehaviour
{
    [Header("Button Settings")]
    public float pressHeight = 0.1f; // How much the button moves up when pressed
    public float animationDuration = 0.1f; // Duration of the animation

    [Header("Buttons List")]
    public ButtonData[] buttons; // Array of buttons to manage

    void Start()
    {
        // Initialize all buttons
        foreach (var button in buttons)
        {
            if (button.buttonObject != null)
            {
                InitializeButton(button);
            }
        }
    }

    void InitializeButton(ButtonData button)
    {
        // Cache the original position of the button
        Vector3 originalPosition = button.buttonObject.transform.position;

        // Get the SpriteRenderer component
        SpriteRenderer spriteRenderer = button.buttonObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer not found on {button.buttonObject.name}. Please add one.");
            return;
        }

        // Set up the AudioSource for sound effects
        AudioSource audioSource = button.buttonObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = button.soundVolume;

        // Reset the button to its default state
        spriteRenderer.sprite = button.defaultSprite;

        // Add interaction logic to the button
        button.buttonObject.AddComponent<ButtonInteraction>().Initialize(
            originalPosition,
            pressHeight,
            animationDuration,
            spriteRenderer,
            button.clickedSprite,
            audioSource,
            button.clickSound
        );
    }
}