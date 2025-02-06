using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QuizSystem.UI
{
    public class QuizProgressBar : MonoBehaviour
    {
        [SerializeField] private Slider _progressSlider;
        [Space]
        [SerializeField] private TextMeshProUGUI _progressText;

        private int _totalNumberOfQuestions = 0;

        private int _currentQuestionIndex = 0;

        public void SetupProgressBar(int totalNumberOfQuestions)
        {
            _totalNumberOfQuestions = totalNumberOfQuestions;

            _progressSlider.maxValue = _totalNumberOfQuestions;

            VisualizeProgress();
            
        }

        public void Advance()
        {
            _currentQuestionIndex++;

            VisualizeProgress();
        }

        private void VisualizeProgress()
        {
            _progressSlider.value = _currentQuestionIndex; 
            _progressText.text = $"{_currentQuestionIndex} / {_totalNumberOfQuestions}";
        }

        public void ResetProgressState()
        {
            _totalNumberOfQuestions = 0;
            _currentQuestionIndex = 0;
            _progressSlider.value = 0;
        }
    } 
}
