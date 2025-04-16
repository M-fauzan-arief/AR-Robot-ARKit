using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class PlayerProfileManager : MonoBehaviour
{
    private PlayFabManager playFabManager;

    // UI Elements for Displaying Player Profile Information
    public TMP_Text displayNameText;
    public TMP_Text avatarUrlText;
    public TMP_Text createdDateText;

    // Input Field and Button for Setting Display Name
    public TMP_InputField displayNameInputField;  // ✅ Confirmed as TMP_InputField
    public Button setDisplayNameButton;

    void Start()
    {
        playFabManager = PlayFabManager.instance;

        if (playFabManager == null)
        {
            Debug.LogError("PlayFabManager instance not found!");
            return;
        }

        if (setDisplayNameButton != null)
        {
            setDisplayNameButton.onClick.AddListener(SubmitDisplayName);
        }

        FetchPlayerProfile();
    }

    public void FetchPlayerProfile()
    {
        var request = new GetPlayerProfileRequest
        {
            PlayFabId = null,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true,
                ShowAvatarUrl = true,
                ShowCreated = true
            }
        };

        PlayFabClientAPI.GetPlayerProfile(request, OnProfileFetched, OnError);
    }

    private void OnProfileFetched(GetPlayerProfileResult result)
    {
        string displayName = result.PlayerProfile?.DisplayName ?? "Not Set";
        string avatarUrl = result.PlayerProfile?.AvatarUrl ?? "Not Available";
        string createdDate = result.PlayerProfile?.Created?.ToString("yyyy-MM-dd") ?? "Unknown";

        if (displayNameText != null) displayNameText.text = $"Display Name: {displayName}";
        if (avatarUrlText != null) avatarUrlText.text = $"Avatar URL: {avatarUrl}";
        if (createdDateText != null) createdDateText.text = $"Created: {createdDate}";

        Debug.Log($"Fetched Player Profile - Display Name: {displayName}, Avatar URL: {avatarUrl}, Created: {createdDate}");
    }

    public void SubmitDisplayName()
    {
        if (displayNameInputField == null)
        {
            Debug.LogError("Display name input field is not assigned.");
            return;
        }

        string newDisplayName = displayNameInputField.text.Trim();

        if (string.IsNullOrEmpty(newDisplayName))
        {
            Debug.LogError("Display name cannot be empty.");
            return;
        }

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newDisplayName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdated, OnError);
    }

    private void OnDisplayNameUpdated(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Display name updated successfully!");
        FetchPlayerProfile();

        if (playFabManager != null)
        {
            playFabManager.LoadMenu();
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"Error: {error.GenerateErrorReport()}");

        if (displayNameText != null)
        {
            displayNameText.text = $"Error: {error.ErrorMessage}";
        }
    }
}
