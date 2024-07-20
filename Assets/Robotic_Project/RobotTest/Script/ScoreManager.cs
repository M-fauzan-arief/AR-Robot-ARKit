using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;    // UI Text for displaying score
    public TextMeshProUGUI starText;     // UI Text for displaying stars
    public GameObject ScoreCanvas;       // Canvas to show score

    [SerializeField]
    private int totalObjects = 3;        // Total number of objects to place
    private int score = 0;               // Score to track correct placements
    private int stars = 0;               // Number of stars earned
    private int objectPlace = 0;         // Number of objects placed

    void Start()
    {
        scoreText.text = "Score: 0";
        starText.text = "";
        ScoreCanvas.SetActive(false);    // Ensure the canvas is initially hidden
    }

    public void UpdateScore()
    {
        score++; // Increase score when called
        scoreText.text = "Score: " + score;
        Debug.Log("Your Score is: " + score);
        CheckWinCondition();
    }

    public void UpdateObjectPlace()
    {
        objectPlace++;
        Debug.Log("Object Placed: " + objectPlace);
        CheckWinCondition(); // Ensure win condition is checked after updating objectPlace
    }

    private void CheckWinCondition()
    {
        if (objectPlace >= totalObjects)
        {
            // Player successfully placed all correct objects
            if (score == totalObjects)
            {
                stars = 3;
                starText.text = "You win! All objects placed correctly! 3 Stars!";
                ScoreCanvas.SetActive(true);
                Debug.Log("You win! All objects placed correctly! 3 Stars!");
            }
            else if (score == totalObjects - 1)
            {
                stars = 2;
                starText.text = "You win! Almost all objects placed correctly! 2 Stars!";
                ScoreCanvas.SetActive(true);
                Debug.Log("You win! Almost all objects placed correctly! 2 Stars!");
            }
            else if (score == totalObjects - 2)
            {
                stars = 1;
                starText.text = "You win! Some objects placed correctly! 1 Star!";
                ScoreCanvas.SetActive(true);
                Debug.Log("You win! Some objects placed correctly! 1 Star!");
            }
            else
            {
                stars = 0;
                starText.text = "Not all objects placed correctly. 0 Stars.!";
                ScoreCanvas.SetActive(true);
                Debug.Log("Not all objects placed correctly. 0 Stars.");
            }
        }
    }
}
