using QuizSystem.SO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using QuizSystem.Events;
using QuizSystem.Interface;
using QuizSystem.UI.Base;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement; // Required for scene loading

namespace QuizSystem.UI
{
    public class QuizResultPanel : QuizResultUI
    {
        [SerializeField] private QuizResultData _resultData;

        [Space]
        [SerializeField] private TextMeshProUGUI _correctAnswersTMP;
        [SerializeField] private TextMeshProUGUI _incorrectAnswersTMP;

        [Space]
        [SerializeField] private TextMeshProUGUI _timeTakenTMP;
        [SerializeField] private TextMeshProUGUI _percentageTMP;

        [SerializeField] private Button _nextButton;

        [Space]
        [SerializeField] private Canvas _resultCanvas;

        [Header("Navigation Buttons")]
        [SerializeField] private Button _backToMainMenuButton; // New button to return to main menu

        [Header("Audio Settings")]
        [SerializeField] private AudioClip _finishMusic;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            _nextButton.onClick.AddListener(Close);
            _backToMainMenuButton.onClick.AddListener(BackToMainMenu);

            QuizEvents.OnQuizIsFinished += Open;
        }

        private void OnDisable()
        {
            _nextButton.onClick.RemoveListener(Close);
            _backToMainMenuButton.onClick.RemoveListener(BackToMainMenu);

            QuizEvents.OnQuizIsFinished -= Open;
        }

        private void SetResultData()
        {
            _correctAnswersTMP.text = $"{_resultData.numberOfCorrectAnswers}<#b3bedb>/ {_resultData.totalNumberOfQuestions}";
            _incorrectAnswersTMP.text = $"{_resultData.numberOfIncorrectAnswers}<#b3bedb>/ {_resultData.totalNumberOfQuestions}";
            _percentageTMP.text = $"{_resultData.correctAnswersPercentage} % Correct";
            _timeTakenTMP.text = $"{_resultData.totalTimeTakenToFinishQuiz} Seconds";
        }

        public override void Open()
        {
            SetResultData();
            _resultCanvas.enabled = true;
            _resultCanvas.gameObject.SetActive(true);
            PlayFinishMusic();
            SendScoreToPlayFab();
        }

        public override void Close()
        {
            _resultCanvas.gameObject.SetActive(false);
            _resultCanvas.enabled = false;
            QuizEvents.OnCloseQuiz?.Invoke();
        }

        private void PlayFinishMusic()
        {
            if (_finishMusic != null && _audioSource != null)
            {
                _audioSource.clip = _finishMusic;
                _audioSource.Play();
            }
            else
            {
                Debug.LogWarning("Finish music or AudioSource is missing!");
            }
        }

        private void SendScoreToPlayFab()
        {
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                var playerScore = _resultData.correctAnswersPercentage;
                var request = new UpdatePlayerStatisticsRequest
                {
                    Statistics = new List<StatisticUpdate>
                    {
                        new StatisticUpdate
                        {
                            StatisticName = "QuizLeaderboard",
                            Value = (int)playerScore
                        }
                    }
                };

                PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
                {
                    Debug.Log("Score sent to PlayFab successfully.");

                    if (AchievementManager.Instance != null)
                    {
                        AchievementManager.Instance.UnlockAchievement("complete_quiz");
                        Debug.Log("Quiz achievement unlocked.");
                    }
                    else
                    {
                        Debug.LogWarning("AchievementManager.Instance is null. Make sure it's in the scene.");
                    }

                }, OnError);
            }
            else
            {
                Debug.LogWarning("User is not logged in to PlayFab. Cannot send score.");
            }
        }

        private void OnError(PlayFabError error)
        {
            Debug.LogError("Error sending score to PlayFab: " + error.GenerateErrorReport());
        }

        private void BackToMainMenu()
        {
            Destroy(gameObject);
            SceneManager.LoadScene("Menu"); // Replace "MainMenu" with your main menu scene name
        }
    }
}
