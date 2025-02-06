using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance;

    [Header("Experience")]
    [SerializeField] AnimationCurve experienceCurve;
    [SerializeField] int experiencePerActivation = 5;
    int currentLevel, totalExperience;
    int previousLevelsExperience, nextLevelsExperience;

    [Header("Interface")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI experienceText;
    [SerializeField] Image experienceFill;

    [Header("Animation Settings")]
    [SerializeField] float fillAnimationDuration = 0.5f;
    [SerializeField] float textAnimationDuration = 0.3f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("ExperienceManager: Initializing...");
        UpdateLevel();
    }

    public void AddExperience()
    {
        int oldTotalExperience = totalExperience;
        totalExperience += experiencePerActivation;
        Debug.Log($"ExperienceManager: Added {experiencePerActivation} experience. Total Experience: {totalExperience}");

        CheckForLevelUp();

        StartCoroutine(AnimateInterface(oldTotalExperience));

        // Save player stats to PlayFab
        PlayFabManager.instance.SavePlayerStats(currentLevel, totalExperience);
    }

    void CheckForLevelUp()
    {
        while (totalExperience >= nextLevelsExperience)
        {
            currentLevel++;
            Debug.Log($"ExperienceManager: Level Up! Current Level: {currentLevel}");
            UpdateLevel();
        }
    }

    void UpdateLevel()
    {
        previousLevelsExperience = (int)experienceCurve.Evaluate(currentLevel);
        nextLevelsExperience = (int)experienceCurve.Evaluate(currentLevel + 1);

        levelText.text = currentLevel.ToString();

        Debug.Log($"ExperienceManager: Updated Level Data - Previous Level Exp: {previousLevelsExperience}, Next Level Exp: {nextLevelsExperience}");
    }

    IEnumerator AnimateInterface(int oldTotalExperience)
    {
        int start = oldTotalExperience - previousLevelsExperience;
        int end = nextLevelsExperience - previousLevelsExperience;

        float oldFillAmount = (float)start / (float)end;
        float newFillAmount = (float)(totalExperience - previousLevelsExperience) / (float)end;

        experienceFill.DOFillAmount(newFillAmount, fillAnimationDuration).SetEase(Ease.OutQuad);

        DOTween.To(() => start, x => start = x, totalExperience - previousLevelsExperience, textAnimationDuration)
            .OnUpdate(() =>
            {
                experienceText.text = start + " exp / " + end + " exp";
            })
            .SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(fillAnimationDuration);
    }

    void UpdateInterface()
    {
        int start = totalExperience - previousLevelsExperience;
        int end = nextLevelsExperience - previousLevelsExperience;

        levelText.text = currentLevel.ToString();
        experienceText.text = start + " exp / " + end + " exp";
        experienceFill.fillAmount = (float)start / (float)end;

        Debug.Log($"ExperienceManager: Updated Interface - Level: {currentLevel}, Progress: {start}/{end}, Fill Amount: {experienceFill.fillAmount}");
    }

    // Method to set level and XP from PlayFab
    public void SetLevelAndXP(int level, int xp)
    {
        currentLevel = level;
        totalExperience = xp;

        UpdateLevel();
        UpdateInterface();
    }
}