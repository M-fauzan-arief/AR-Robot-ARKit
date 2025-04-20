using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    // Reference to the PlayerProfileManager to update UI
    public PlayerProfileManager playerProfileManager;

    // Achievement statuses
    private bool isCompleteLessonUnlocked = false;
    private bool isFinishRobotUnlocked = false;
    private bool isCompleteQuizUnlocked = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AchievementManager initialized.");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("Duplicate AchievementManager instance destroyed.");
        }

        // Try to find PlayerProfileManager automatically (optional)
        if (playerProfileManager == null)
        {
            playerProfileManager = FindObjectOfType<PlayerProfileManager>();
            if (playerProfileManager != null)
                Debug.Log("PlayerProfileManager found and linked in AchievementManager.");
            else
                Debug.LogWarning("PlayerProfileManager not found! Make sure it's in the scene.");
        }
    }

    // Load achievements from PlayFab and check the unlock status
    public void LoadAchievementsFromPlayFab()
    {
        Debug.Log("Loading achievements from PlayFab...");
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(), OnAchievementsLoaded, OnError);
    }

    private void OnAchievementsLoaded(GetPlayerStatisticsResult result)
    {
        Debug.Log("Achievements loaded from PlayFab.");

        // Check if the player has unlocked each achievement
        foreach (var stat in result.Statistics)
        {
            Debug.Log($"Checking achievement: {stat.StatisticName}, Value: {stat.Value}");

            switch (stat.StatisticName)
            {
                case "complete_lesson":
                    isCompleteLessonUnlocked = stat.Value == 1;
                    Debug.Log($"complete_lesson unlocked: {isCompleteLessonUnlocked}");
                    break;
                case "finish_robot":
                    isFinishRobotUnlocked = stat.Value == 1;
                    Debug.Log($"finish_robot unlocked: {isFinishRobotUnlocked}");
                    break;
                case "complete_quiz":
                    isCompleteQuizUnlocked = stat.Value == 1;
                    Debug.Log($"complete_quiz unlocked: {isCompleteQuizUnlocked}");
                    break;
                default:
                    Debug.Log($"Unknown achievement: {stat.StatisticName}");
                    break;
            }
        }

        // Inform the UI manager that achievements have been loaded
        if (playerProfileManager != null)
        {
            playerProfileManager.OnAchievementsLoaded(result);
        }
        else
        {
            Debug.LogWarning("PlayerProfileManager not assigned in AchievementManager.");
        }
    }

    // Unlock an achievement and update PlayFab
    public void UnlockAchievement(string achievementId)
    {
        Debug.Log($"Unlocking achievement: {achievementId}");

        // Set the status for the specified achievement
        switch (achievementId)
        {
            case "complete_lesson":
                isCompleteLessonUnlocked = true;
                break;
            case "finish_robot":
                isFinishRobotUnlocked = true;
                break;
            case "complete_quiz":
                isCompleteQuizUnlocked = true;
                break;
            default:
                Debug.LogWarning($"Unknown achievement ID: {achievementId}");
                return;
        }

        // Update PlayFab to reflect the achievement
        UpdatePlayFabAchievement(achievementId);
    }

    // Update PlayFab with the achievement unlock
    private void UpdatePlayFabAchievement(string achievementId)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = achievementId, Value = 1 }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
        {
            Debug.Log($"Achievement {achievementId} updated in PlayFab.");
        }, error =>
        {
            Debug.LogError($"Failed to update {achievementId} in PlayFab: {error.GenerateErrorReport()}");
        });
    }

    // Error handler
    private void OnError(PlayFabError error)
    {
        Debug.LogError($"Error: {error.GenerateErrorReport()}");
    }
}
