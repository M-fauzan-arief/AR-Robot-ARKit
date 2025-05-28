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

    [Header("Username System")]
    public GameObject nameWindow;
    public GameObject leaderboardWindow;

    [Header("Login")]
    public GameObject loginPanel;
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField nameInputField;

    public Button loginButton;
    public Button registerButton;

    public TextMeshProUGUI messageText;

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

        if (loginButton != null) loginButton.onClick.AddListener(LoginWithUsernameOnly);
        if (registerButton != null) registerButton.onClick.AddListener(RegisterWithUsernameOnly);

        if (messageText != null)
            messageText.text = "Please enter your username and password.";
    }

    void CheckLoginStatus()
    {
        if (PlayFabSettings.staticPlayer != null && PlayFabSettings.staticPlayer.IsClientLoggedIn())
        {
            Debug.Log("Player IS logged in.");
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
                    loginPanel.SetActive(true);
                });
        }
        else
        {
            Debug.Log("Player is NOT logged in.");
            loginPanel.SetActive(true);
        }
    }

    public void LoginWithUsernameOnly()
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = usernameInputField.text,
            Password = passwordInputField.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
                GetPlayerStatistics = true
            }
        };
        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnError);
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
                messageText.text = $"Login successful!\nYour Player ID is: {playerID}\nPlease set your display name.";
        }
        else
        {
            if (messageText != null)
                messageText.text = $"Login successful!\nWelcome, {playerName}!\nYour Player ID is: {playerID}";
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

        playerName = nameInputField.text;

        if (messageText != null)
            messageText.text = $"Welcome, {playerName}!\nYour Player ID is: {playerID}";

        nameWindow.SetActive(false);
        leaderboardWindow.SetActive(true);
        LoadMenu();
    }

    public void RegisterWithUsernameOnly()
    {
        var request = new RegisterPlayFabUserRequest
        {
            Username = usernameInputField.text,
            Password = passwordInputField.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Registration successful!");

        // Automatically set display name to username
        var displayNameRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = usernameInputField.text
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(displayNameRequest, updateResult =>
        {
            Debug.Log("Display name set to username.");
            if (messageText != null)
                messageText.text = "Registration complete! Logging you in...";

            // Proceed to login
            LoginWithUsernameOnly();
        }, error =>
        {
            Debug.LogError("Failed to set display name: " + error.GenerateErrorReport());
            if (messageText != null)
                messageText.text = "Registration succeeded, but setting display name failed.";
        });
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

    public void UnlockAchievement(string achievementId)
    {
        AchievementManager.Instance.UnlockAchievement(achievementId);
    }
}
