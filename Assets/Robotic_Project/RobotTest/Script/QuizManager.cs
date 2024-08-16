using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizManager: MonoBehaviour
{
    public List <QuestionAndAnswer> QnA;
    public gameobject[] options;
    public int currentQuestion;

    public Text QuestionTxt;

    private void Start(){
        generateQuestion();
    }

    public void correct(){
        QnA.RemoveAt(currentQuestion);
        generateQuestion();
    }

    void SetAnswers(){
        for (int = 0; i < options.Length; i++){
            options[i].GetComponent<AnswerScript>().isCorrect = false;
            options[i].transform.GetChild(0).GetComponent<Text>() = QnA[currentQuestion].Answer[i];

            if(QnA[currentQuestion].correctAnswer == i+1){
                options[i]..GetComponent<AnswerScript>().isCorrect = true;
            }

        }
    }

    void generateQuestion(){
        currentQuestion = Random.Range(0, QnA.Count);

        QuestionTxt.text = QnA[currentQuestion].Question;
        SetAnswers();
        
           }
}
