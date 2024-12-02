using QuizSystem.SO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using QuizSystem.Events;
using QuizSystem.Interface;
using QuizSystem.UI.Base;

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

        [Header("Audio Settings")]
        [SerializeField] private AudioClip _finishMusic; // Music to play when the quiz finishes
        private AudioSource _audioSource; // AudioSource to play the music

        private void Awake()
        {
            // Ensure AudioSource exists
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

            QuizEvents.OnQuizIsFinished += Open;
        }

        private void OnDisable()
        {
            _nextButton.onClick.RemoveListener(Close);

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

            PlayFinishMusic(); // Play the music when the quiz is finished
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
    }
}
