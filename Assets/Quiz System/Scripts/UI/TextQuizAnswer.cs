using UnityEngine;
using TMPro;
using UnityEngine.UI;
using QuizSystem.Events;
using QuizSystem.UI.Base;

namespace QuizSystem.UI
{
    public class TextQuizAnswer : QuizAnswer
    {
        [SerializeField] private TextMeshProUGUI _answerTMP;

        public string Answer
        {
            get { return _answerTMP.text; }
            set { _answerTMP.text = value; }
        }

        private new void OnEnable()
        {
            base.OnEnable();
            _button.onClick.AddListener(
                    () => QuizEvents.OnChooseTextAnswer?.Invoke(this)
                );
        }
        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}