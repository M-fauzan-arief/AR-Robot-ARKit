using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : MonoBehaviour
{
    public List<QuestionAndAnswer> QnA;
    public GameObject[] options;
    public int currentQuestion;

    public TMP_Text QuestionTxt;
    public TMP_Text ScoreTxt; // TMP Text for displaying the score

    private int score = 0; // Variable to keep track of the score

    private void Start()
    {
        generateQuestion();
        UpdateScoreText();
    }

    public void correct()
    {
        score++; // Increment the score when the answer is correct
        QnA.RemoveAt(currentQuestion);
        generateQuestion();
        UpdateScoreText(); // Update the score display
    }

    public void wrong()
    {
        QnA.RemoveAt(currentQuestion);
        generateQuestion();
    }

    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == null)
            {
                Debug.LogError($"options[{i}] is null");
                continue;
            }

            var answerScript = options[i].GetComponent<AnswerScript>();
            if (answerScript == null)
            {
                Debug.LogError($"options[{i}] does not have an AnswerScript component");
                continue;
            }

            var textComponent = options[i].transform.GetChild(0).GetComponent<TMP_Text>();
            if (textComponent == null)
            {
                Debug.LogError($"Child 0 of options[{i}] does not have a TMP_Text component");
                continue;
            }

            // Set the text of the answer option
            textComponent.text = QnA[currentQuestion].Answers[i];

            // Set whether this option is the correct answer
            answerScript.isCorrect = (QnA[currentQuestion].correctAnswer == i);
        }
    }

    void generateQuestion()
    {
        if (QnA.Count > 0)
        {
            currentQuestion = Random.Range(0, QnA.Count);
            QuestionTxt.text = QnA[currentQuestion].Question;
            SetAnswers();
        }
        else
        {
            Debug.Log("No more questions available!");
        }
    }

    void UpdateScoreText()
    {
        if (ScoreTxt != null)
        {
            ScoreTxt.text = "Score: " + score;
        }
    }
}
