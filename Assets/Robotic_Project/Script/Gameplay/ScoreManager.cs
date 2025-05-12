using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public GameObject ScoreCanvas;

    public TextMeshProUGUI tctText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI retryText;
    public TextMeshProUGUI efficiencyText;

    [Header("Scoring Settings")]
    [SerializeField] private int totalObjects = 3;
    private int score = 0;
    private int stars = 0;
    private int objectPlace = 0;

    [Header("Star UI")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip successMusic;
    private AudioSource audioSource;

    private PlayFabManager playfabManager;

    // Technical Metrics
    private float startTime;
    private float endTime;
    private float taskCompletionTime;

    private int errorCount = 0;
    private int retryCount = 0;

    private float accuracy;
    private float errorRate;
    private float taskEfficiency;

    void Start()
    {
        scoreText.text = "Score: 0";
        ScoreCanvas.SetActive(false);

        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        playfabManager = FindObjectOfType<PlayFabManager>();
        if (playfabManager == null)
        {
            Debug.LogError("PlayFabManager not found! Please ensure it is in the scene.");
        }

        startTime = Time.time; // Start timer
    }

    public void UpdateScore()
    {
        score++;
        scoreText.text = "Score: " + score;

        // Recalculate Technical Metrics in Real-Time
        RecalculateMetrics();

        // Display the technical metrics on UI
        ShowTechnicalMetrics();

        Debug.Log("Your Score is: " + score);
    }

    public void UpdateObjectPlace()
    {
        objectPlace++;
        Debug.Log("Object Placed: " + objectPlace);

        // Recalculate Technical Metrics in Real-Time
        RecalculateMetrics();

        // Display the technical metrics on UI
        ShowTechnicalMetrics();

        // Check if the player has placed all objects, which will trigger the win condition
        CheckWinCondition();
    }

    private void RecalculateMetrics()
    {
        // Calculate task completion time in real-time
        taskCompletionTime = Time.time - startTime;

        // Calculate accuracy: percentage of correct actions out of total objects
        accuracy = (float)score / totalObjects * 100f;

        // Calculate error rate: ratio of errors over total objects
        errorRate = (float)errorCount / totalObjects;

        // Calculate task efficiency considering time, errors, and retries
        float denominator = taskCompletionTime + errorRate + retryCount;
        taskEfficiency = denominator > 0 ? accuracy / denominator : 0f;
    }

    private void CheckWinCondition()
    {
        // Win condition: Check if all objects are placed
        if (objectPlace >= totalObjects)
        {
            // Once all objects are placed, calculate the final metrics
            endTime = Time.time;
            taskCompletionTime = endTime - startTime;

            // Stars logic based on score
            if (score == 3)
            {
                stars = 3;
                star1.SetActive(true);
                star2.SetActive(true);
                star3.SetActive(true);
            }
            else if (score == 2)
            {
                stars = 2;
                star1.SetActive(true);
                star2.SetActive(true);
                star3.SetActive(false);
            }
            else if (score == 1)
            {
                stars = 1;
                star1.SetActive(true);
                star2.SetActive(false);
                star3.SetActive(false);
            }

            ScoreCanvas.SetActive(true);

            // Send metrics to PlayFab and display final results
            playfabManager.SendLeaderboard(score);
            SendTechnicalMetricsToPlayFab();

            Debug.Log($"You win! Score: {score}, Stars: {stars}");
            Debug.Log($"TCT: {taskCompletionTime:F2}s | Accuracy: {accuracy:F1}% | Error Rate: {errorRate:F2} | Retry Count: {retryCount} | Task Efficiency: {taskEfficiency:F2}");

            PlaySuccessMusic();

            UnlockFinishRobotAchievement();
        }
    }

    private void ShowTechnicalMetrics()
    {
        if (tctText != null) tctText.text = $"TCT: {taskCompletionTime:F2}s";
        if (accuracyText != null) accuracyText.text = $"Accuracy: {accuracy:F1}%";
        if (errorText != null) errorText.text = $"Error Rate: {errorRate:F2}";
        if (retryText != null) retryText.text = $"Retry Count: {retryCount}";
        if (efficiencyText != null) efficiencyText.text = $"Task Efficiency: {taskEfficiency:F2}";
    }

    private void UnlockFinishRobotAchievement()
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = "finish_robot", Value = 1 }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
        {
            Debug.Log("Achievement 'finish_robot' unlocked successfully.");
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.LoadAchievementsFromPlayFab();
            }
        },
        error =>
        {
            Debug.LogError("Failed to unlock 'finish_robot': " + error.GenerateErrorReport());
        });
    }

    private void PlaySuccessMusic()
    {
        if (successMusic != null && audioSource != null)
        {
            audioSource.clip = successMusic;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Success music or AudioSource is missing.");
        }
    }

    private void SendTechnicalMetricsToPlayFab()
    {
        var customData = new Dictionary<string, string>
        {
            { "TCT", taskCompletionTime.ToString("F2") },
            { "Accuracy", accuracy.ToString("F2") },
            { "ErrorRate", errorRate.ToString("F2") },
            { "RetryCount", retryCount.ToString() },
            { "Efficiency", taskEfficiency.ToString("F2") }
        };

        var request = new UpdateUserDataRequest
        {
            Data = customData,
            Permission = UserDataPermission.Private
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log(" Technical metrics uploaded to PlayFab."),
            error => Debug.LogError(" Failed to upload metrics: " + error.GenerateErrorReport()));
    }
}
