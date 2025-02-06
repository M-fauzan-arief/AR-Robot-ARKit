using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonMenuManager : MonoBehaviour
{
    // Main Menu Buttons
    [SerializeField] private Button userButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button loginButton; // Login Button

    // Pop-up Panels
    [SerializeField] private GameObject userPanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject loginPanel; // Login Panel

    // Close Buttons for each panel
    [SerializeField] private Button userCloseButton;
    [SerializeField] private Button leaderboardCloseButton;
    [SerializeField] private Button settingsCloseButton;
    [SerializeField] private Button loginCloseButton; // Login Close Button

    // Backdrop GameObject
    [SerializeField] private GameObject backdrop;

    // Animation duration
    [SerializeField] private float animationDuration = 0.5f;

    // Start scale for the pop-up animation
    [SerializeField] private Vector3 startScale = new Vector3(0.5f, 0.5f, 0.5f);

    // Sound Effects
    [SerializeField] private AudioClip buttonClickSound; // Sound for button clicks
    [SerializeField] private float soundPitch = 1f; // Adjustable pitch for sound effects
    private AudioSource audioSource;

    private void Start()
    {
        // Initialize panels and backdrop to be inactive
        DeactivateAllPanels();
        backdrop.SetActive(false); // Ensure backdrop is deactivated initially

        // Initialize AudioSource for sound effects
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.pitch = soundPitch; // Set initial pitch <button class="citation-flag" data-index="1">

        // Add listeners to buttons with responsive sound effects
        AddResponsiveButtonListener(userButton, () => ShowPanel(userPanel));
        AddResponsiveButtonListener(leaderboardButton, () => ShowPanel(leaderboardPanel));
        AddResponsiveButtonListener(settingsButton, () => ShowPanel(settingsPanel));
        AddResponsiveButtonListener(loginButton, () => ShowPanel(loginPanel));

        // Add listeners to close buttons with responsive sound effects
        AddResponsiveButtonListener(userCloseButton, () => HidePanel(userPanel));
        AddResponsiveButtonListener(leaderboardCloseButton, () => HidePanel(leaderboardPanel));
        AddResponsiveButtonListener(settingsCloseButton, () => HidePanel(settingsPanel));
        AddResponsiveButtonListener(loginCloseButton, () => HidePanel(loginPanel));
    }

    private void AddResponsiveButtonListener(Button button, System.Action action)
    {
        button.onClick.AddListener(() =>
        {
            // Play the sound immediately when the button is clicked
            if (buttonClickSound != null && audioSource != null)
            {
                audioSource.pitch = soundPitch; // Update pitch dynamically <button class="citation-flag" data-index="1">
                audioSource.PlayOneShot(buttonClickSound); // Play the sound instantly
            }

            // Delay the action slightly to ensure the sound plays first
            StartCoroutine(DelayedAction(action, 0.05f)); // Optional: Small delay for better responsiveness
        });
    }

    private System.Collections.IEnumerator DelayedAction(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke(); // Execute the original action after a slight delay
    }

    private void DeactivateAllPanels()
    {
        userPanel.SetActive(false);
        leaderboardPanel.SetActive(false);
        settingsPanel.SetActive(false);
        loginPanel.SetActive(false);
    }

    private void ShowPanel(GameObject panel)
    {
        // Deactivate all menu buttons
        userButton.gameObject.SetActive(false);
        leaderboardButton.gameObject.SetActive(false);
        settingsButton.gameObject.SetActive(false);
        loginButton.gameObject.SetActive(false);

        // Activate the backdrop and fade it in
        backdrop.SetActive(true);
        Image backdropImage = backdrop.GetComponent<Image>();
        if (backdropImage != null)
        {
            backdropImage.color = new Color(backdropImage.color.r, backdropImage.color.g, backdropImage.color.b, 0f); // Set alpha to 0
            backdropImage.DOFade(0.75f, animationDuration).SetEase(Ease.Linear); // Fade in to 75% opacity
        }

        // Activate the selected panel
        panel.SetActive(true);

        // Play pop-up animation
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.localScale = startScale; // Start with a smaller scale
            panelRect.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack); // Scale up to normal size
        }
    }

    private void HidePanel(GameObject panel)
    {
        // Play close animation for the panel
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.DOScale(startScale, animationDuration).SetEase(Ease.InBack).OnComplete(() =>
            {
                // Deactivate the panel after animation
                panel.SetActive(false);

                // Reactivate all menu buttons
                userButton.gameObject.SetActive(true);
                leaderboardButton.gameObject.SetActive(true);
                settingsButton.gameObject.SetActive(true);
                loginButton.gameObject.SetActive(true);

                // Fade out the backdrop
                Image backdropImage = backdrop.GetComponent<Image>();
                if (backdropImage != null)
                {
                    backdropImage.DOFade(0f, animationDuration).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        backdrop.SetActive(false); // Deactivate backdrop after fading out
                    });
                }
            });
        }
    }
}