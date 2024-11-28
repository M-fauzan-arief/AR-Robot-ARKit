using QuizSystem.Events;
using QuizSystem.SO;
using UnityEngine;

namespace QuizSystem.Gameplay
{
    public class QuizOpenerManual : MonoBehaviour
    {
        [SerializeField] private QuizData _quizData;


        public void Open()
        {
            QuizEvents.OnOpenQuiz?.Invoke(_quizData);
        }
    } 
}
