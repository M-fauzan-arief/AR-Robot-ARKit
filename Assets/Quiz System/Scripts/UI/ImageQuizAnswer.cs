using QuizSystem.Events;
using QuizSystem.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace QuizSystem.UI
{
    public class ImageQuizAnswer : QuizAnswer
    {
        [SerializeField] private Image _answerIMG;

        public Sprite Answer
        {
            get { return _answerIMG.sprite; }
            set { _answerIMG.sprite = value; _answerIMG.SetNativeSize(); }
        }

        private new void OnEnable()
        {
            base.OnEnable();
            _button.onClick.AddListener(
                    () => QuizEvents.OnChooseImageAnswer?.Invoke(this)
                );
        }
        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }
    } 
}
