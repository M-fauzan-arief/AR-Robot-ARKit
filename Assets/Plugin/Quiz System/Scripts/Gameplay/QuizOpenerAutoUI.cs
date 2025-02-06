using UnityEngine;
using QuizSystem.Events;
using QuizSystem.SO;
using UnityEngine.EventSystems;

namespace QuizSystem.Gameplay
{
    public class QuizOpenerAutoUI : MonoBehaviour,IPointerClickHandler
    {
        [SerializeField] private QuizData _quizData;

        public void OnPointerClick(PointerEventData eventData)
        {
            QuizEvents.OnOpenQuiz?.Invoke(_quizData);
        }
    } 
}
