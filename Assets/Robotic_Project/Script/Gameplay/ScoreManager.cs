using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public GameObject ScoreCanvas;

    [SerializeField]
    private int totalObjects = 3;
    private int score = 0;
    private int stars = 0;
    private int objectPlace = 0;

    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip successMusic;
    private AudioSource audioSource;

    private PlayFabManager playfabManager;

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
    }

    public void UpdateScore()
    {
        score++;
        scoreText.text = "Score: " + score;
        Debug.Log("Your Score is: " + score);
        CheckWinCondition();
    }

    public void UpdateObjectPlace()
    {
        objectPlace++;
        Debug.Log("Object Placed: " + objectPlace);
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (objectPlace >= totalObjects)
        {
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
            playfabManager.SendLeaderboard(score);

            Debug.Log("You win! Score: " + score + " Stars: " + stars);
            PlaySuccessMusic();

            UnlockFinishRobotAchievement();
        }
    }

    private void UnlockFinishRobotAchievement()
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "finish_robot", // Must match your PlayFab statistic ID
                    Value = 1
                }
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
}
