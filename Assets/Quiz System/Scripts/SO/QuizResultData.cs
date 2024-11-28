using UnityEngine;

namespace QuizSystem.SO
{
    [CreateAssetMenu(fileName = "QuizResultData", menuName = "Quiz System/Result Data")]
    public class QuizResultData : ScriptableObject
    {
        public int totalNumberOfQuestions;

        public int numberOfCorrectAnswers;
        public int numberOfIncorrectAnswers;

        public float correctAnswersPercentage;

        public float totalTimeTakenToFinishQuiz;
    } 
}
