using QuizSystem.UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace QuizSystem.UI
{
    public class AnswerEffect : AnswerEffectBase
    {
        [SerializeField] private Image _answerIMG;

        [Space]
        [SerializeField] private Sprite _normalStateSprite;
        [SerializeField] private Sprite _correctSprite;
        [SerializeField] private Sprite _incorrectSprite;


        public override void CorrectEffect()
        {
            _answerIMG.sprite = _correctSprite;
        }

        public override void IncorrectEffect()
        {
            _answerIMG.sprite = _incorrectSprite;
        }

        public override void ResetState()
        {
            _answerIMG.sprite = _normalStateSprite;
        }
    }
}