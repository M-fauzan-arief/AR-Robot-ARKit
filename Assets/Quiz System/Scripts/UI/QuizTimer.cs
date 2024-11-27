using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using QuizSystem.SO;

namespace QuizSystem.UI
{
    public class QuizTimer : MonoBehaviour
    {
        [SerializeField] private GameObject _timerUI;


        [Space]
        [SerializeField] private TextMeshProUGUI _timerTMP;

        [Space]
        [SerializeField] private QuizResultData _resultData;

        //Timer
        private float _totalTime;
        private float _initialTime;
        private float _timeCounter;

        private QuestionTimerData _currentQuestionTimeData;
        private IEnumerator _currentCountDownTimeRoutine;

        public void Open()
        {
            _timerUI.SetActive(true);
        }
        public void Close()
        {
            _timerUI.SetActive(false);

            //StopAllCoroutines();
        }

        public void CountTotalTime()
        {
            _totalTime += _initialTime - _timeCounter;

            _resultData.totalTimeTakenToFinishQuiz = _totalTime;
        }


        public void StartCountDownTimer(QuestionTimerData data)
        {
            _initialTime = data.timerValue;
            _timeCounter = _initialTime;

            _currentCountDownTimeRoutine = CountDownTimerRoutine(); 
            StartCoroutine(_currentCountDownTimeRoutine);   
        }
        public void StopCountDownTimer()
        {
            if (_currentCountDownTimeRoutine != null)
            {
                StopCoroutine(_currentCountDownTimeRoutine);
            }
            CountTotalTime();
        }


        IEnumerator CountDownTimerRoutine()
        {
            _timerTMP.text = $"{_timeCounter}";

            while(_timeCounter > 0)
            {
                yield return new WaitForSecondsRealtime(1);
                _timeCounter--;

                _timerTMP.text = $"{_timeCounter}";
            }
        }
    } 
}
