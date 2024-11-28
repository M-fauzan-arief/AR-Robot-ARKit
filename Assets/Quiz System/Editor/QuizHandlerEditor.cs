using UnityEngine;
using UnityEditor;
using QuizSystem.Gameplay;

[CustomEditor(typeof(QuizSystemHandler))]
public class QuizHandlerEditor : Editor
{
    private QuizLoadMode previousLoadMode;

    public override void OnInspectorGUI()
    {
        QuizSystemHandler quizHandler = (QuizSystemHandler)target;

        DrawDefaultInspector();

        if (quizHandler.LoadMode != previousLoadMode)
        {
            previousLoadMode = quizHandler.LoadMode;
            quizHandler.HandleLoadModeChange();
        }
    }
}
