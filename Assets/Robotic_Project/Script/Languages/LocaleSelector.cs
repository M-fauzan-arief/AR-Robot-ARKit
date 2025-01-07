using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI; // Required for Dropdown

public class LocaleSelector : MonoBehaviour
{
    private bool active = false;
    public Dropdown localeDropdown; // Assign your dropdown in the Unity Editor

    void Start()
    {
        // Populate dropdown with locale names
        if (localeDropdown != null)
        {
            localeDropdown.options.Clear();
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                localeDropdown.options.Add(new Dropdown.OptionData(locale.LocaleName));
            }
            localeDropdown.onValueChanged.AddListener(ChangeLocale);
            localeDropdown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale); // Set default
        }
    }

    public void ChangeLocale(int localeID)
    {
        if (active)
            return;
        StartCoroutine(SetLocale(localeID));
    }

    IEnumerator SetLocale(int _localeID)
    {
        active = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeID];
        active = false;
    }
}
