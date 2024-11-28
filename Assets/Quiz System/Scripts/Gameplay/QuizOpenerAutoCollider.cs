using UnityEngine;
using QuizSystem.SO;
using QuizSystem.Events;

namespace QuizSystem.Gameplay
{
    public class QuizOpenerAutoCollider : MonoBehaviour
    {

        [SerializeField]
        private QuizData _quizData;


        private void OnMouseUpAsButton()
        {
            QuizEvents.OnOpenQuiz?.Invoke(_quizData);
        }
    }

}