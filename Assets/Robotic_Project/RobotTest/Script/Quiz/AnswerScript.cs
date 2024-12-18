using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswerScript : MonoBehaviour
{
    public bool isCorrect = false;
    public QuizManager QuizManager;

    public void Answer()
    {
        if (isCorrect)
        {
            Debug.Log("Correct Answer");
            QuizManager.CorrectAnswer();
        }
        else
        {
            Debug.Log("Wrong Answer");
            QuizManager.WrongAnswer();
        }
    }
}
