using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button userButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button profileButton;

    [Header("Panels")]
    [SerializeField] private GameObject userPanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject profilePanel;

    [Header("Close Buttons")]
    [SerializeField] private Button userCloseButton;
    [SerializeField] private Button leaderboardCloseButton;
    [SerializeField] private Button settingsCloseButton;
    [SerializeField] private Button loginCloseButton;
    [SerializeField] private Button profileCloseButton;

    [Header("Backdrop")]
    [SerializeField] private GameObject backdrop;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Vector3 startScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Sound Effects")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private float soundPitch = 1f;
    private AudioSource audioSource;

    private void Start()
    {
        InitUI();
        InitAudio();
        InitListeners();
    }

    private void InitUI()
    {
        DeactivateAllPanels();
        if (backdrop != null) backdrop.SetActive(false);
    }

    private void InitAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.pitch = soundPitch;
    }

    private void InitListeners()
    {
        AddResponsiveButtonListener(userButton, () => ShowPanel(userPanel));
        AddResponsiveButtonListener(leaderboardButton, () => ShowPanel(leaderboardPanel));
        AddResponsiveButtonListener(settingsButton, () => ShowPanel(settingsPanel));
        AddResponsiveButtonListener(loginButton, () => ShowPanel(loginPanel));
        AddResponsiveButtonListener(profileButton, () => ShowPanel(profilePanel));

        AddResponsiveButtonListener(userCloseButton, () => HidePanel(userPanel));
        AddResponsiveButtonListener(leaderboardCloseButton, () => HidePanel(leaderboardPanel));
        AddResponsiveButtonListener(settingsCloseButton, () => HidePanel(settingsPanel));
        AddResponsiveButtonListener(loginCloseButton, () => HidePanel(loginPanel));
        AddResponsiveButtonListener(profileCloseButton, () => HidePanel(profilePanel));
    }

    private void AddResponsiveButtonListener(Button button, System.Action action)
    {
        if (button == null) return;
        button.onClick.AddListener(() =>
        {
            PlayClickSound();
            StartCoroutine(DelayedAction(action, 0.05f));
        });
    }

    private void PlayClickSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.pitch = soundPitch;
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    private System.Collections.IEnumerator DelayedAction(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    private void DeactivateAllPanels()
    {
        userPanel?.SetActive(false);
        leaderboardPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        loginPanel?.SetActive(false);
        profilePanel?.SetActive(false);
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel == null) return;

        SetMenuButtonsActive(false);
        if (backdrop != null)
        {
            backdrop.SetActive(true);
            FadeBackdrop(0.75f);
        }

        panel.SetActive(true);
        AnimatePanelScale(panel.transform as RectTransform, startScale, Vector3.one, Ease.OutBack);
    }

    private void HidePanel(GameObject panel)
    {
        if (panel == null || panel.transform == null) return;

        var rect = panel.transform as RectTransform;
        if (rect == null) return;

        AnimatePanelScale(rect, Vector3.one, startScale, Ease.InBack, () =>
        {
            if (panel != null)
            {
                panel.SetActive(false);
                SetMenuButtonsActive(true);
                if (backdrop != null) FadeBackdrop(0f, () => backdrop.SetActive(false));
            }
        });
    }

    private void AnimatePanelScale(RectTransform rect, Vector3 from, Vector3 to, Ease ease, TweenCallback onComplete = null)
    {
        if (rect == null || rect.gameObject == null) return;
        if (!rect.gameObject.activeInHierarchy) return;

        DOTween.Kill(rect);
        rect.localScale = from;

        var tween = rect.DOScale(to, animationDuration).SetEase(ease);
        if (onComplete != null)
            tween.OnComplete(onComplete);
    }

    private void FadeBackdrop(float targetAlpha, TweenCallback onComplete = null)
    {
        if (backdrop == null) return;

        Image backdropImage = backdrop.GetComponent<Image>();
        if (backdropImage == null) return;

        DOTween.Kill(backdropImage);
        var tween = backdropImage.DOFade(targetAlpha, animationDuration).SetEase(Ease.Linear);
        if (onComplete != null)
            tween.OnComplete(onComplete);
    }

    private void SetMenuButtonsActive(bool isActive)
    {
        userButton?.gameObject.SetActive(isActive);
        leaderboardButton?.gameObject.SetActive(isActive);
        settingsButton?.gameObject.SetActive(isActive);
        loginButton?.gameObject.SetActive(isActive);
        profileButton?.gameObject.SetActive(isActive);
    }
}
