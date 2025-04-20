using UnityEngine;

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Achievements/Achievement")]
public class Achievement : ScriptableObject
{
    public string achievementID; // Unique name to use in PlayFab, like "ACH_WIN_LEVEL1"
    public string title;
    public string description;
}
