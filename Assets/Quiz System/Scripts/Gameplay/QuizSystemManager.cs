using QuizSystem.Events;
using QuizSystem.SO;
using QuizSystem.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using QuizSystem.Custom;
using QuizSystem.UI.Base;
using System.Collections;

namespace QuizSystem.Gameplay
{
    public class QuizSystemManager : MonoBehaviour
    {
        [SerializeField] private QuizDataRef _chosenQuizData;

        [Space]
        [SerializeField] private SharableBool _openQuizAutomatically;
        
        [Space]
        [SerializeField] private Canvas _quizCanvas;
        [SerializeField] private Slider _questionsNumberSlider;
        [SerializeField] private Button _nextButton;

        [Space]
        [Header("Text Question")]
        [SerializeField] private GameObject _textQuestionHolder;
        [SerializeField] private TextMeshProUGUI _questionTMP;

        [Space]
        [Header("Image Question")]
        [SerializeField] private GameObject _imageQuestionHolder;
        [SerializeField] private Image _questionIMG;

        [Space]
        [Header("Text Answers")]
        [SerializeField] private GameObject _textAnswersHolder;
        [SerializeField] private List<TextQuizAnswer> _textAnswers;

        [Space]
        [Header("Image Answers")]
        [SerializeField] private GameObject _imageAnswersHolder;
        [SerializeField] private List<ImageQuizAnswer> _imageAnswers;

        [Space]
        [Tooltip("The Time to Wait before moving to the next question")]
        [SerializeField] private float _questionsTransitionTime;

        [Space]
        [SerializeField] private QuizProgressBar _quizProgressBar;

        [Space]
        [SerializeField] private QuizTimer _quizTimer;

        [Space]
        [SerializeField] private bool _isAbleToGoNext;

        [Space]
        [SerializeField] private QuizResultData _resultData;

        [Space]
        [SerializeField] private GameObject _interactionBlocker;


        private bool _isQuizStarted = false;

        private QuizData _quizData;
        private int _currentQuestionIndex = 0;
        private Question _currentQuestion;

        
        public void Open()
        { 
            _quizData = _chosenQuizData.chosenQuiz;
            StartQuiz();
            _quizCanvas.enabled = true; 
            _quizCanvas.gameObject.SetActive(true); 
        }
        public void Close() 
        { 
            _quizCanvas.enabled = false; 
            _quizCanvas.gameObject.SetActive(false); 
        }
        
        
        private void Awake()
        {
            if (_openQuizAutomatically.value)
            {
                Open();
            }
        }
        private void OnEnable()
        {
            QuizEvents.OnChooseTextAnswer += HandleChosenTextAnswer;
            QuizEvents.OnChooseImageAnswer += HandleChoseImageAnswer;

            _nextButton.onClick.AddListener(GoNextQuestion);
        }
        private void OnDisable()
        {
            QuizEvents.OnChooseTextAnswer -= HandleChosenTextAnswer;
            QuizEvents.OnChooseImageAnswer -= HandleChoseImageAnswer;

            _nextButton.onClick.RemoveListener(GoNextQuestion);
        }

        private void StartQuiz()
        {
            _resultData.totalNumberOfQuestions = _chosenQuizData.chosenQuiz.questions.Count;
            _resultData.numberOfCorrectAnswers = 0;
            _resultData.numberOfIncorrectAnswers = 0;

            _quizProgressBar.SetupProgressBar(_chosenQuizData.chosenQuiz.questions.Count);

            ShowQuestion(0);
            _isQuizStarted = true;
        }
        private void HandleFinishedQuiz()
        {
            Debug.Log("Quiz is Finished");

            //Calcualte the correct Answers percentage
            _resultData.correctAnswersPercentage = (float)_resultData.numberOfCorrectAnswers / _resultData.totalNumberOfQuestions * 100;

            QuizEvents.OnQuizIsFinished?.Invoke();
            Close();
            ResetManagerState();
        }
        private void ShowQuestion(int index)
        {
            StartCoroutine(ShowQuestionRoutine(index));
        }

        IEnumerator ShowQuestionRoutine(int index)
        {
            if (_isQuizStarted)
                yield return new WaitForSeconds(_questionsTransitionTime);


            if (index >= _quizData.questions.Count)
            {
                HandleFinishedQuiz();
                yield break;
            }

            _currentQuestion = _quizData.questions[index];

            switch (_currentQuestion.questionType)
            {
                case QuestionType.Text:
                    ShowTextQuestion();
                    break;
                case QuestionType.Image:
                    ShowImageQuestion();
                    break;
            }
            switch (_currentQuestion.answerType)
            {
                case AnswerType.Text:
                    ShowTextAnswers();
                    break;
                case AnswerType.Image:
                    ShowImageAnswers();
                    break;
            }

            if (_currentQuestion.hasTime)
            {
                _quizTimer.Open();
                yield return new WaitUntil(()=> _quizTimer.gameObject.activeInHierarchy == true);
                _quizTimer.StartCountDownTimer(_currentQuestion.questionTimerData);
            }
            else
            {
                _quizTimer.Close();
            }

            _interactionBlocker.SetActive(false);
        }

        private void ShowTextQuestion()
        {
            _questionTMP.text = _currentQuestion.questionText;
            _textQuestionHolder.SetActive(true);
            _imageQuestionHolder.SetActive(false);
        }
        private void ShowImageQuestion()
        {
            _questionIMG.sprite = _currentQuestion.questionImage;
            _questionIMG.SetNativeSize();
            _imageQuestionHolder.SetActive(true);
            _textQuestionHolder.SetActive(false);
        }
        private void ShowTextAnswers()
        {
            for (int i = 0; i < _currentQuestion.textAnswer.answers.Count; i++)
            {
                var currentAnswerData = _currentQuestion.textAnswer.answers[i];
                _textAnswers[i].Answer = currentAnswerData.answerValue;
                _textAnswers[i].ResetState();
                _textAnswers[i].gameObject.SetActive(true);

                _textAnswers[i].IsCorrect = currentAnswerData.isCorrect;
            }

            if(_currentQuestion.textAnswer.answers.Count < _textAnswers.Count)
            {
                for (int i = _currentQuestion.textAnswer.answers.Count; i < _textAnswers.Count ; i++)
                {
                    _textAnswers[i].gameObject.SetActive(false);
                }
            }


            _imageAnswersHolder.SetActive(false);
            _textAnswersHolder.SetActive(true);
        }
        private void ShowImageAnswers()
        {
            for (int i = 0; i < _currentQuestion.imageAnswer.answers.Count; i++)
            {
                var currentAnswerData = _currentQuestion.imageAnswer.answers[i];
                _imageAnswers[i].Answer = currentAnswerData.answerValue;
                _imageAnswers[i].ResetState();
                _imageAnswers[i].gameObject.SetActive(true);

                _imageAnswers[i].IsCorrect = currentAnswerData.isCorrect;
            }

            if (_currentQuestion.imageAnswer.answers.Count < _imageAnswers.Count)
            {
                for (int i = _currentQuestion.imageAnswer.answers.Count; i < _imageAnswers.Count; i++)
                {
                    _imageAnswers[i].gameObject.SetActive(false);
                }
            }

            _textAnswersHolder.SetActive(false);
            _imageAnswersHolder.SetActive(true);
        }
        private void GoNextQuestion()
        {
            //if the user must answer the current answer first
            if (!_isAbleToGoNext)
                return;

            _isAbleToGoNext = false;
            _currentQuestionIndex++;
            ShowQuestion(_currentQuestionIndex);
            _quizProgressBar.Advance();
        }
        
        private void HandleAnswer(QuizAnswer quizAnswer)
        {
            _interactionBlocker.SetActive(true);

            if (_currentQuestion.hasTime)
                _quizTimer.StopCountDownTimer();

            if (quizAnswer.IsCorrect)
            {
                quizAnswer.DisplayAsCorrect();
                Debug.Log("Correct Answer");
                _resultData.numberOfCorrectAnswers++;
            }
            else
            {
                quizAnswer.DisplayAsIncorrect();
                _resultData.numberOfIncorrectAnswers++;
                Debug.Log("Incorrect Answer");
            }

            _isAbleToGoNext = true;
            GoNextQuestion();
        }
        
        private void HandleChosenTextAnswer(TextQuizAnswer answer)
        {
            HandleAnswer(answer);
        }
        private void HandleChoseImageAnswer(ImageQuizAnswer answer)
        {
            HandleAnswer(answer);
        }

        private void ResetManagerState()
        {
            _isQuizStarted = false;
            _currentQuestionIndex = 0;
            _currentQuestion = null;
            _quizData = null;
            _quizProgressBar.ResetProgressState();
        }
    } 
}
