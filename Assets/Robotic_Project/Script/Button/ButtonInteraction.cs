using UnityEngine;
using DG.Tweening;

public class ButtonInteraction : MonoBehaviour
{
    private Vector3 originalPosition;
    private float pressHeight;
    private float animationDuration;
    private SpriteRenderer spriteRenderer;
    private Sprite clickedSprite;
    private AudioSource audioSource;
    private AudioClip clickSound;

    private bool isPressed = false;

    public void Initialize(Vector3 originalPos, float pressHeightValue, float animDuration, SpriteRenderer renderer, Sprite clickedSpriteValue, AudioSource audio, AudioClip sound)
    {
        originalPosition = originalPos;
        pressHeight = pressHeightValue;
        animationDuration = animDuration;
        spriteRenderer = renderer;
        clickedSprite = clickedSpriteValue;
        audioSource = audio;
        clickSound = sound;
    }

    void OnMouseDown()
    {
        if (!isPressed)
        {
            isPressed = true;

            // Change the sprite to the clicked state
            if (clickedSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = clickedSprite;
            }

            // Play the click sound
            if (clickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(clickSound);
            }

            // Animate the button moving up using DOTween
            transform.DOMoveY(originalPosition.y + pressHeight, animationDuration).SetEase(Ease.InOutQuad);
        }
    }

    void OnMouseUp()
    {
        if (isPressed)
        {
            isPressed = false;

            // Reset the button to its default state
            ResetButton();
        }
    }

    void ResetButton()
    {
        // Reset the sprite to the default state
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = spriteRenderer.sprite == clickedSprite ? spriteRenderer.sprite : null;
        }

        // Animate the button moving back to its original position using DOTween
        transform.DOMoveY(originalPosition.y, animationDuration).SetEase(Ease.InOutQuad);
    }
}