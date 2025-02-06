using UnityEngine;

namespace QuizSystem.UI.Base
{
    public abstract class AnswerEffectBase : MonoBehaviour
    {
        public abstract void ResetState();
        public abstract void CorrectEffect();
        public abstract void IncorrectEffect();

    } 
}
