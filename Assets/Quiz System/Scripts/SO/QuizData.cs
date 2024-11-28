using QuizSystem.Custom;
using System.Collections.Generic;
using UnityEngine;

namespace QuizSystem.SO
{
    public enum QuestionType
    {
        Text,
        Image
    }
    public enum AnswerType
    {
        Text,
        Image
    }

    [CreateAssetMenu(fileName ="Quiz Data",menuName ="Quiz System/Data")]
    public class QuizData : ScriptableObject
    {
        public List<Question> questions;
    }

    [System.Serializable]
    public class Question
    {
        public QuestionType questionType;

        [ConditionalProperty("questionType",(int)QuestionType.Text)]
        public string questionText;

        [ConditionalProperty("questionType", (int)QuestionType.Image)]
        public Sprite questionImage;

        [Space]
        public AnswerType answerType;
        [Header("Make sure you have 4 answers or less")]

        [ConditionalProperty("answerType", (int)AnswerType.Text)]
        public TextAnswerList textAnswer;

        [ConditionalProperty("answerType", (int)AnswerType.Image)]
        public ImageAnswerList imageAnswer;

        public bool hasTime;
        [ConditionalProperty("hasTime")]
        public QuestionTimerData questionTimerData;
        
    }

    [System.Serializable]
    public class TextAnswerList
    {
        public List<TextAnswer> answers;
    }

    [System.Serializable]
    public class ImageAnswerList
    {
        public List<SpriteAnswer> answers;
    }

    [System.Serializable]
    public class Answer
    {
        public bool isCorrect;
    }

    [System.Serializable]
    public class TextAnswer : Answer
    {
        public string answerValue;
    }

    [System.Serializable]
    public class SpriteAnswer : Answer
    {
        public Sprite answerValue;
    }

    public enum TimerType
    {
        Hours,
        Minutes,
        Seconds
    }

    [System.Serializable]
    public class QuestionTimerData
    {
        public TimerType timerType;
        public float timerValue;
    }
}
