using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerProfileManager : MonoBehaviour
{
    private PlayFabManager playFabManager;

    // UI Elements for Displaying Player Profile Information
    public TMP_Text displayNameText;
    public TMP_Text playerIdText;
    public TMP_Text createdDateText;

    // Input Field and Button for Setting Display Name
    public TMP_InputField displayNameInputField;
    public Button setDisplayNameButton;

    // Achievement badges
    public GameObject completeLessonBadge;
    public GameObject finishRobotBadge;
    public GameObject completeQuizBadge;

    // Achievement statuses
    private bool isCompleteLessonUnlocked = false;
    private bool isFinishRobotUnlocked = false;
    private bool isCompleteQuizUnlocked = false;

    void Start()
    {
        playFabManager = PlayFabManager.instance;

        if (playFabManager == null)
        {
            Debug.LogError("PlayFabManager instance not found!");
            return;
        }

        FetchPlayerProfile();

        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.LoadAchievementsFromPlayFab();
        }
        else
        {
            Debug.LogWarning("AchievementManager is not initialized yet.");
        }
    }


    public void FetchPlayerProfile()
    {
        var request = new GetPlayerProfileRequest
        {
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true,
                ShowCreated = true
            }
        };

        PlayFabClientAPI.GetPlayerProfile(request, OnProfileFetched, OnError);
    }

    private void OnProfileFetched(GetPlayerProfileResult result)
    {
        var profile = result.PlayerProfile;

        string displayName = profile?.DisplayName ?? "Not Set";
        string playerID = profile?.PlayerId ?? "Unknown";
        string createdDate = profile?.Created?.ToString("yyyy-MM-dd") ?? "Unknown";

        if (displayNameText != null) displayNameText.text = $"{displayName}";
        if (playerIdText != null) playerIdText.text = $" {playerID}";
        if (createdDateText != null) createdDateText.text = $" {createdDate}";

        Debug.Log($"Welcome back, {displayName}.");
        Debug.Log($"Player ID: {playerID}");
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"Error: {error.GenerateErrorReport()}");

        if (displayNameText != null)
        {
            displayNameText.text = $"Error: {error.ErrorMessage}";
        }
    }

    public void UpdateAchievementBadges()
    {
        if (completeLessonBadge != null) completeLessonBadge.SetActive(isCompleteLessonUnlocked);
        if (finishRobotBadge != null) finishRobotBadge.SetActive(isFinishRobotUnlocked);
        if (completeQuizBadge != null) completeQuizBadge.SetActive(isCompleteQuizUnlocked);
    }

    public void OnAchievementsLoaded(GetPlayerStatisticsResult result)
    {
        Debug.Log("Achievements loaded from PlayFab.");

        foreach (var stat in result.Statistics)
        {
            Debug.Log($"Checking achievement: {stat.StatisticName}, Value: {stat.Value}");

            switch (stat.StatisticName)
            {
                case "complete_lesson":
                    isCompleteLessonUnlocked = stat.Value == 1;
                    if (isCompleteLessonUnlocked) Debug.Log("Achievement unlocked: complete_lesson");
                    break;
                case "finish_robot":
                    isFinishRobotUnlocked = stat.Value == 1;
                    if (isFinishRobotUnlocked) Debug.Log("Achievement unlocked: finish_robot");
                    break;
                case "complete_quiz":
                    isCompleteQuizUnlocked = stat.Value == 1;
                    if (isCompleteQuizUnlocked) Debug.Log("Achievement unlocked: complete_quiz");
                    break;
                default:
                    Debug.Log($"Unknown achievement: {stat.StatisticName}");
                    break;
            }
        }

        UpdateAchievementBadges();
    }

    // Optional toggle functions for editor testing
    public void ToggleCompleteLessonBadge()
    {
        if (completeLessonBadge != null) completeLessonBadge.SetActive(true);
    }

    public void ToggleFinishRobotBadge()
    {
        if (finishRobotBadge != null) finishRobotBadge.SetActive(true);
    }

    public void ToggleCompleteQuizBadge()
    {
        if (completeQuizBadge != null) completeQuizBadge.SetActive(true);
    }
}
