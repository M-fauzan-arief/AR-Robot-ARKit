using UnityEngine;
using UnityEngine.UI; // Add this line to include the Button and other UI components
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayerProfileManager : MonoBehaviour
{
    // Reference to the PlayFabManager instance
    private PlayFabManager playFabManager;

    // UI Elements for Displaying Player Profile Information
    public TMP_Text displayNameText; // Text field to display the player's display name
    public TMP_Text avatarUrlText;   // Text field to display the player's avatar URL
    public TMP_Text createdDateText; // Text field to display the account creation date

    // Input Field and Button for Setting Display Name
    public TMP_InputField displayNameInputField; // Input field for entering a new display name
    public Button setDisplayNameButton;          // Button to submit the display name

    void Start()
    {
        // Get the PlayFabManager instance
        playFabManager = PlayFabManager.instance;

        if (playFabManager == null)
        {
            Debug.LogError("PlayFabManager instance not found!");
            return;
        }

        // Assign button click event
        if (setDisplayNameButton != null)
        {
            setDisplayNameButton.onClick.AddListener(SubmitDisplayName);
        }

        // Fetch the player's profile when the game starts
        FetchPlayerProfile();
    }

    /// <summary>
    /// Fetches the player's profile from PlayFab.
    /// </summary>
    public void FetchPlayerProfile()
    {
        var request = new GetPlayerProfileRequest
        {
            PlayFabId = null, // Use the current player's ID
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true,
                ShowAvatarUrl = true,
                ShowCreated = true
            }
        };

        PlayFabClientAPI.GetPlayerProfile(request, OnProfileFetched, OnError);
    }

    /// <summary>
    /// Handles the successful retrieval of the player's profile.
    /// </summary>
    private void OnProfileFetched(GetPlayerProfileResult result)
    {
        string displayName = result.PlayerProfile?.DisplayName ?? "Not Set";
        string avatarUrl = result.PlayerProfile?.AvatarUrl ?? "Not Available";
        string createdDate = result.PlayerProfile?.Created.HasValue == true
            ? result.PlayerProfile.Created.Value.ToString("yyyy-MM-dd")
            : "Unknown";

        // Update the UI with the fetched profile data
        if (displayNameText != null) displayNameText.text = $"Display Name: {displayName}";
        if (avatarUrlText != null) avatarUrlText.text = $"Avatar URL: {avatarUrl}";
        if (createdDateText != null) createdDateText.text = $"Created: {createdDate}";

        Debug.Log($"Fetched Player Profile - Display Name: {displayName}, Avatar URL: {avatarUrl}, Created: {createdDate}");
    }

    /// <summary>
    /// Submits the display name entered by the player to PlayFab.
    /// </summary>
    public void SubmitDisplayName()
    {
        string newDisplayName = displayNameInputField.text;

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

    /// <summary>
    /// Handles the successful update of the player's display name.
    /// </summary>
    private void OnDisplayNameUpdated(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Display name updated successfully!");

        // Optionally refresh the profile to reflect the changes in the UI
        FetchPlayerProfile();

        // Optionally notify the PlayFabManager that the display name has been set
        if (playFabManager != null)
        {
            playFabManager.LoadMenu(); // Load the main menu or next scene
        }
    }

    /// <summary>
    /// Handles errors from PlayFab API calls.
    /// </summary>
    private void OnError(PlayFabError error)
    {
        Debug.LogError($"Error: {error.GenerateErrorReport()}");

        // Optionally display the error message in the UI
        if (displayNameText != null)
        {
            displayNameText.text = $"Error: {error.ErrorMessage}";
        }
    }
}