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
    public TMP_InputField nameInputField;

    public Button startButton;
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
        if (startButton != null) startButton.onClick.AddListener(LoginOrRegisterWithCustomID);
        if (messageText != null) messageText.text = "Please enter your username.";
    }

    void CheckLoginStatus()
    {
        if (PlayFabSettings.staticPlayer != null && PlayFabSettings.staticPlayer.IsClientLoggedIn())
        {
            loginPanel.SetActive(false);
            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
                result =>
                {
                    string playerId = result.AccountInfo.PlayFabId;
                    var request = new GetPlayerProfileRequest { PlayFabId = playerId };
                    PlayFabClientAPI.GetPlayerProfile(request, profileResult =>
                    {
                        string playerName = profileResult.PlayerProfile?.DisplayName;
                        Debug.Log("Player name: " + playerName);
                    }, error => Debug.LogError("Profile error: " + error.GenerateErrorReport()));
                },
                error =>
                {
                    Debug.LogError("Session invalid. Logging out...");
                    PlayFabSettings.staticPlayer.ForgetAllCredentials();
                    loginPanel.SetActive(true);
                });
        }
        else
        {
            loginPanel.SetActive(true);
        }
    }

    public void LoginOrRegisterWithCustomID()
    {
        string customId = usernameInputField.text;
        if (string.IsNullOrEmpty(customId))
        {
            messageText.text = "Username is required.";
            return;
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = customId,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
                GetPlayerStatistics = true
            }
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess_CustomID, OnError);
    }

    void OnLoginSuccess_CustomID(LoginResult result)
    {
        playerID = result.PlayFabId;
        playerName = result.InfoResultPayload?.PlayerProfile?.DisplayName;

        if (string.IsNullOrEmpty(playerName))
        {
            string newDisplayName = usernameInputField.text;
            var request = new UpdateUserTitleDisplayNameRequest { DisplayName = newDisplayName };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, displayNameResult =>
            {
                playerName = newDisplayName;
                LoadMenu();
            }, OnError);
        }
        else
        {
            LoadMenu();
        }

        LoadPlayerStats();
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Intro");
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
            if (stat.StatisticName == "Level") level = stat.Value;
            else if (stat.StatisticName == "XP") xp = stat.Value;
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
            MaxResultsCount = 5
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult result)
    {
        foreach (Transform child in rowsParent) Destroy(child.gameObject);

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

    void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.GenerateErrorReport());
        if (messageText != null)
            messageText.text = "Error: " + error.ErrorMessage;
    }
}
