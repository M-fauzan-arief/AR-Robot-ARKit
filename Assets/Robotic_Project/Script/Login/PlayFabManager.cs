using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager instance;

    [Header("Leaderboards")]
    public GameObject rowPrefab;
    public Transform rowsParent;

    [Header("Username")]
    public GameObject nameWindow;
    public GameObject leaderboardWindow;

    [Header("Login")]
    public GameObject loginPanel;
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField nameInputField;

    public Button loginButton;
    public Button registerButton;
    public Button resetPasswordButton;

    public TextMeshProUGUI messageText;
    public TextMeshProUGUI PlayerID;
    public TextMeshProUGUI Username;

    private string playerID;
    private string playerName;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CheckLoginStatus();

        if (loginButton != null) loginButton.onClick.AddListener(Login);
        if (registerButton != null) registerButton.onClick.AddListener(Register);
        if (resetPasswordButton != null) resetPasswordButton.onClick.AddListener(ResetPassword);

        if (messageText != null)
            messageText.text = "Please enter your email and password.";
    }

    void CheckLoginStatus()
    {
        if (PlayFabSettings.staticPlayer != null && PlayFabSettings.staticPlayer.IsClientLoggedIn())
        {
            Debug.Log("Player IS logged in.");
            if (loginPanel != null)
                loginPanel.SetActive(false);

            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
                result =>
                {
                    string playerId = result.AccountInfo.PlayFabId;
                    Debug.Log("Session is valid. Player ID: " + playerId);

                    var request = new GetPlayerProfileRequest { PlayFabId = playerId };
                    PlayFabClientAPI.GetPlayerProfile(request, profileResult =>
                    {
                        string playerName = profileResult.PlayerProfile?.DisplayName;
                        if (!string.IsNullOrEmpty(playerName))
                        {
                            Debug.Log("Player name: " + playerName);
                        }
                        else
                        {
                            Debug.LogWarning("DisplayName is empty.");
                        }
                    }, error =>
                    {
                        Debug.LogError("Error retrieving player profile: " + error.GenerateErrorReport());
                    });
                },
                error =>
                {
                    Debug.LogError("Session is invalid. Logging out...");
                    PlayFabSettings.staticPlayer.ForgetAllCredentials();
                    if (loginPanel != null)
                        loginPanel.SetActive(true);
                });
        }
        else
        {
            Debug.Log("Player is NOT logged in.");
            if (loginPanel != null)
                loginPanel.SetActive(true);
        }
    }

    public void Login()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = emailInputField.text,
            Password = passwordInputField.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
                GetPlayerStatistics = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");

        playerID = result.PlayFabId;
        playerName = result.InfoResultPayload?.PlayerProfile?.DisplayName;

        if (string.IsNullOrEmpty(playerName))
        {
            nameWindow.SetActive(true);
            if (messageText != null)
                messageText.text = "Login successful! Please set your username.";
        }
        else
        {
            if (messageText != null)
                messageText.text = $"Login successful! Welcome, {playerName}.";
            LoadMenu();
        }

        LoadPlayerStats();
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Intro");
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

        if (messageText != null)
            messageText.text = $"Welcome, {nameInputField.text}!";

        nameWindow.SetActive(false); // Close name input window
        leaderboardWindow.SetActive(true); // Optional
        LoadMenu(); // Automatically go to intro scene
    }

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
            messageText.text = "Registration successful! Logging you in...";
        Login(); // Automatically login after successful registration
    }

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

    void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.GenerateErrorReport());
        if (messageText != null)
            messageText.text = "Error: " + error.ErrorMessage;
    }

    public void SavePlayerStats(int level, int xp)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "Level", Value = level },
                new StatisticUpdate { StatisticName = "XP", Value = xp }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnStatsSaved, OnError);
    }

    void OnStatsSaved(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Player stats saved.");
    }

    public void LoadPlayerStats()
    {
        var request = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { "Level", "XP" }
        };
        PlayFabClientAPI.GetPlayerStatistics(request, OnStatsLoaded, OnError);
    }

    void OnStatsLoaded(GetPlayerStatisticsResult result)
    {
        int level = 1;
        int xp = 0;

        foreach (var stat in result.Statistics)
        {
            if (stat.StatisticName == "Level")
                level = stat.Value;
            else if (stat.StatisticName == "XP")
                xp = stat.Value;
        }

        Debug.Log($"Loaded player stats: Level = {level}, XP = {xp}");

        ExperienceManager.Instance.SetLevelAndXP(level, xp);
    }

    public void SendLeaderboard(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "Level1", Value = score }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Leaderboard score submitted.");
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "QuizLeaderboard",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult result)
    {
        foreach (Transform child in rowsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in result.Leaderboard)
        {
            GameObject newRow = Instantiate(rowPrefab, rowsParent);
            TextMeshProUGUI[] textFields = newRow.GetComponentsInChildren<TextMeshProUGUI>(true);

            if (textFields.Length >= 3)
            {
                textFields[0].text = (item.Position + 1).ToString();
                textFields[1].text = item.DisplayName;
                textFields[2].text = item.StatValue.ToString();
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rowsParent.GetComponent<RectTransform>());
    }

    // Achievement system handled by AchievementManager
    public void UnlockAchievement(string achievementId)
    {
        AchievementManager.Instance.UnlockAchievement(achievementId);
    }
}


ChatGPT