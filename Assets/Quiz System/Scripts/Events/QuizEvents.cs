using QuizSystem.SO;
using QuizSystem.UI;
using UnityEngine.Events;

namespace QuizSystem.Events
{
    public static class QuizEvents
    {
        public static UnityAction<QuizData> OnOpenQuiz;

        public static UnityAction<TextQuizAnswer> OnChooseTextAnswer;
        public static UnityAction<ImageQuizAnswer> OnChooseImageAnswer;

        public static UnityAction OnQuizIsFinished;
        public static UnityAction OnCloseQuiz;
    }
}
