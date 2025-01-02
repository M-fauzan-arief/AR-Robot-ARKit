using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class QuestionAndAnswer
    {
        public string Question;
        public List<string> Answers;
        public int CorrectAnswerIndex;
    }

    public List<QuestionAndAnswer> QnA;
    public GameObject[] options;
    public int currentQuestion;

    public TMP_Text QuestionTxt;
    public TMP_Text ScoreTxt; // TMP Text for displaying the score

    private int score = 0; // Variable to keep track of the score

    private void Start()
    {
        if (QnA == null || QnA.Count == 0)
        {
            Debug.LogError("No questions available in QnA list.");
            return;
        }

        InitializeOptions();
        GenerateQuestion();
        UpdateScoreText();
    }

    void InitializeOptions()
    {
        // Attach button listeners to each option once at the start
        for (int i = 0; i < options.Length; i++)
        {
            int index = i; // Local variable to avoid closure issues in loop
            options[i].GetComponent<Button>().onClick.AddListener(() => OnOptionSelected(index));
        }
    }

    void OnOptionSelected(int index)
    {
        if (index == QnA[currentQuestion].CorrectAnswerIndex)
        {
            CorrectAnswer();
        }
        else
        {
            WrongAnswer();
        }
    }

    public void CorrectAnswer()
    {
        score++; // Increment the score when the answer is correct
        Debug.Log("Correct answer selected!");
        QnA.RemoveAt(currentQuestion);
        GenerateQuestion();
        UpdateScoreText(); // Update the score display
    }

    public void WrongAnswer()
    {
        Debug.Log("Wrong answer selected!");
        QnA.RemoveAt(currentQuestion);
        GenerateQuestion();
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

            var textComponent = options[i].transform.GetChild(0).GetComponent<TMP_Text>();
            if (textComponent == null)
            {
                Debug.LogError($"Child 0 of options[{i}] does not have a TMP_Text component");
                continue;
            }

            // Set the text of the answer option
            textComponent.text = QnA[currentQuestion].Answers[i];
        }
    }

    void GenerateQuestion()
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
            // Optionally, display final score or end the quiz
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
