using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;    // UI Text for displaying score
    public GameObject ScoreCanvas;       // Canvas to show score

    [SerializeField]
    private int totalObjects = 3;        // Total number of objects to place
    private int score = 0;               // Score to track correct placements
    private int stars = 0;               // Number of stars earned
    private int objectPlace = 0;         // Number of objects placed

    // References to the star objects assigned in the Unity editor
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    void Start()
    {
        scoreText.text = "Score: 0";

        ScoreCanvas.SetActive(false);    // Ensure the canvas is initially hidden

        // Initially hide all stars
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
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
            if (score == 3)
            {
                stars = 3;
                star1.SetActive(true);
                star2.SetActive(true);
                star3.SetActive(true);
  
            }
            else if (score == 2)
            {
                stars = 2;
                star1.SetActive(true);
                star2.SetActive(true);
                star3.SetActive(false);  // Hide the 3rd star
 
            }
            else if (score == 1)
            {
                stars = 1;
                star1.SetActive(true);
                star2.SetActive(false);  // Hide the 2nd and 3rd star
                star3.SetActive(false);

            }

            ScoreCanvas.SetActive(true); // Show the score canvas when the player wins
            Debug.Log("You win! Score: " + score + " Stars: " + stars);
        }
    }
}
