using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizSystem.SO
{
    [CreateAssetMenu(fileName ="QuizDataRef",menuName ="Quiz System/QuizData Ref")]
    public class QuizDataRef : ScriptableObject
    {
        public QuizData chosenQuiz;
    } 
}
