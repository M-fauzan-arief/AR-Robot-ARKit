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
    public void ARCodeDobot()
    {
        SceneManager.LoadScene("ARCodeDobot");
        Debug.Log("Scene Transition to Dobot Visual Code");
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

    public void DobotScan(){
        SceneManager.LoadScene("Dobot-ARKIT");
        Debug.Log("Dobot Scanner is loaded");
    }


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


    public void Quiz()
    {
        SceneManager.LoadScene("Robot_Quiz");
        Debug.Log("Quiz is loaded");
    }

    public void Lesson_Select()
    {
        SceneManager.LoadScene("Lesson_Select");
        Debug.Log("Lesson Selection is loaded");
    }

    public void Chapter_1()
    {
        SceneManager.LoadScene("Chapter_1");
        Debug.Log("Lesson Selection is loaded");
    }
    public void Chapter_2()
    {
        SceneManager.LoadScene("Chapter_3");
        Debug.Log("Lesson Selection is loaded");
    }

    public void Chapter_3()
    {
        SceneManager.LoadScene("Chapter_3");
        Debug.Log("Lesson Selection is loaded");
    }




    public void Robot_Quiz_C1()
    {
        SceneManager.LoadScene("Robot_Quiz C1");
        Debug.Log("Lesson Selection is loaded");
    }

    public void Robot_Quiz_C2()
    {
        SceneManager.LoadScene("Robot_Quiz C2");
        Debug.Log("Lesson Selection is loaded");
    }

    public void Robot_Quiz_C3()
    {
        SceneManager.LoadScene("Robot_Quiz C3");
        Debug.Log("Lesson Selection is loaded");
    }



    public void ARCodeAPIRO()
    {
        SceneManager.LoadScene("ARCodeAPIRO");
    }
}
