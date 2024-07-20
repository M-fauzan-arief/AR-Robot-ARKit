using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Intro() 
    {
        SceneManager.LoadScene("Intro");
        Debug.Log("Scene transition to Intro");
    }

    public void Lesson()
    {
        SceneManager.LoadScene("Lesson");
        Debug.Log("Scene transition to lesson");    
    }

    public void ARSceneDobot()
    {
        SceneManager.LoadScene("ARSceneDobot");
        Debug.Log("Scene transition to AR");
    }
    
    public void ARSceneAPiro()
    {
        SceneManager.LoadScene("ARSceneAPIRO");
        Debug.Log("Scene Transition to Apiro");
    }

    public void ApiroSelect()
    {
        SceneManager.LoadScene("ApiroSelect");
        Debug.Log("Scene transition to ApiroSelect");
    }
    public void RobotDobotAR() { 
        SceneManager.LoadScene("RobotDobotAR");
        Debug.Log("Scene transition to DobotAR");
    }

    public void RobotSelect()
    {
        SceneManager.LoadScene("RobotSelect");
        Debug.Log("Scene transition to RobotSelect");
    }

    public void exit()
    {
        Application.Quit();
        Debug.Log("Application is quit");
    }


    // Main menu logic
    public void pause()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 0;
    }

    public void Resume()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 1;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
