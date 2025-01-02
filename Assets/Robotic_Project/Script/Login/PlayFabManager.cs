using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Collections.Generic;

public class PlayFabManager : MonoBehaviour
{
    private static PlayFabManager instance;

    [Header("Leaderboards")]
    public GameObject rowPrefab; // Prefab for leaderboard rows
    public Transform rowsParent; // Parent object to hold leaderboard rows

    [Header("Username")]
    public GameObject nameWindow;
    public GameObject leaderboardWindow;

    [Header("Login")]
    // Input fields for Email and Password
    public GameObject loginPanel;
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField nameInputField;

    // Buttons for Login, Register, and Reset Password
    public Button loginButton;
    public Button registerButton;
    public Button resetPasswordButton;

    // Text to display feedback messages
    public TextMeshProUGUI messageText;

    void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist this object across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
    }

    void Start()
    {
        CheckLoginStatus();

        // Add listeners for button clicks
        if (loginButton != null) loginButton.onClick.AddListener(Login);
        if (registerButton != null) registerButton.onClick.AddListener(Register);
        if (resetPasswordButton != null) resetPasswordButton.onClick.AddListener(ResetPassword);

        // Display default messages
        if (messageText != null)
            messageText.text = "Please enter your email and password.";
    }



    void CheckLoginStatus()
    {
        if (PlayFabSettings.staticPlayer != null && PlayFabSettings.staticPlayer.IsClientLoggedIn())
        {
            Debug.Log("Player IS logged in.");
            if (loginPanel != null)
            {
                loginPanel.SetActive(false);
            }

            // Optional: Verify session ticket validity
            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
                result => Debug.Log("Session is valid. Player ID: " + result.AccountInfo.PlayFabId),
                error =>
                {
                    Debug.LogError("Session is invalid. Logging out...");
                    PlayFabSettings.staticPlayer.ForgetAllCredentials();
                    if (loginPanel != null)
                    {
                        loginPanel.SetActive(true);
                    }
                });
        }
        else
        {
            Debug.Log("Player is NOT logged in.");
            if (loginPanel != null)
            {
                loginPanel.SetActive(true);
            }
        }
    }




    // Login function
    public void Login()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = emailInputField.text,
            Password = passwordInputField.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");

        string playerName = result.InfoResultPayload?.PlayerProfile?.DisplayName;

        // Check if a username exists; if not, prompt the user to set one
        if (string.IsNullOrEmpty(playerName))
        {
            nameWindow.SetActive(true); // Prompt for username
            if (messageText != null)
                messageText.text = "Login successful! Please set your username.";
        }
        else
        {
            if (messageText != null)
                messageText.text = $"Login successful! Welcome, {playerName}.";
        }
    }


    public void SubmitNameButton()
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nameInputField.text,
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnError);
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Updated display name!");
        leaderboardWindow.SetActive(true);
    }

    // Register function
    public void Register()
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = emailInputField.text,
            Password = passwordInputField.text,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Registration successful!");
        if (messageText != null)
            messageText.text = "Registration successful! You can now log in.";
    }

    // Reset password function
    public void ResetPassword()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = emailInputField.text,
            TitleId = PlayFabSettings.staticSettings.TitleId
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordResetSuccess, OnError);
    }

    void OnPasswordResetSuccess(SendAccountRecoveryEmailResult result)
    {
        Debug.Log("Password reset email sent successfully!");
        if (messageText != null)
            messageText.text = "Password reset email sent! Please check your inbox.";
    }

    // Error handling
    void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.GenerateErrorReport());
        if (messageText != null)
            messageText.text = "Error: " + error.ErrorMessage;
    }

    public void SendLeaderboard(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Level1",
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successfully sent leaderboard score.");
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "Level1",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult result)
    {
        // Clear existing leaderboard rows
        foreach (Transform child in rowsParent)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"Received {result.Leaderboard.Count} leaderboard entries.");

        foreach (var item in result.Leaderboard)
        {
            GameObject newRow = Instantiate(rowPrefab, rowsParent);

            // Retrieve all TextMeshProUGUI components in the row
            TextMeshProUGUI[] textFields = newRow.GetComponentsInChildren<TextMeshProUGUI>(true);

            if (textFields.Length < 3)
            {
                Debug.LogError($"Row prefab does not have enough TextMeshProUGUI components! Expected 3, but found {textFields.Length}.");
                continue; // Skip this row to avoid further errors
            }

            // Update the text fields with leaderboard data
            textFields[0].text = (item.Position +1) .ToString(); // Position
            textFields[1].text = item.DisplayName;           // Player ID
            textFields[2].text = item.StatValue.ToString(); // Score

            Debug.Log($"Created leaderboard row: Position: {item.Position}, ID: {item.PlayFabId}, Score: {item.StatValue}");
        }

        // Force UI layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate(rowsParent.GetComponent<RectTransform>());
    }

}
