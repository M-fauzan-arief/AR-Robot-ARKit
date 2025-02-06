using UnityEngine;
using UnityEngine.UI;

namespace QuizSystem.UI.Base
{
    public class QuizAnswer : MonoBehaviour
    {
        [Space]
        [SerializeField] protected bool _isCorrect = false;

        [Space]
        [SerializeField] protected AnswerEffectBase _answerEffect;

        public bool IsCorrect
        {
            get { return _isCorrect; }
            set { _isCorrect = value; }
        }

        protected Button _button;

        protected void OnEnable()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }

        public void DisplayAsCorrect()
        {
            _answerEffect.CorrectEffect();
        }
        public void DisplayAsIncorrect()
        {
            _answerEffect.IncorrectEffect();
        }
        public void ResetState()
        {
            _isCorrect = false;
            _answerEffect.ResetState();
        }
    } 
}
