using System;
using System.Collections.Generic;
using Services;
using Services.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizeString : MonoBehaviour
{
    [BoxGroup("Ключ")]
    [SerializeField] private string key;        // Ключ локализации
    [BoxGroup("Добавление нового значения")]
    [SerializeField] private string language;   // Язык (например, "en", "ru")
    [BoxGroup("Добавление нового значения")]
    [SerializeField] private string value;      // Значение перевода

    private Text textComponent;
    private ILocalizationService localizationService;

    private void OnEnable()
    {
        textComponent = GetComponent<Text>();
        // Получаем сервис локализации (предполагается, что он доступен глобально)
        if (ServiceLocator.AllServicesRegistered)
        {
            localizationService = ServiceLocator.Get<ILocalizationService>();
            localizationService.LanguageChanged += UpdateText;
            UpdateText();
        }
        else
        {
            ServiceLocator.OnAllServicesRegistered += OnServicesRegistered;
        }
        
    }

    private void OnDestroy()
    {
        localizationService.LanguageChanged -= UpdateText;
    }
    
    private void OnServicesRegistered()
    {
        localizationService = ServiceLocator.Get<ILocalizationService>();
        localizationService.LanguageChanged += UpdateText;
        UpdateText();
        ServiceLocator.OnAllServicesRegistered -= OnServicesRegistered;
    }
    
    private void UpdateText()
    {
        textComponent.text = localizationService.GetLocalizedString(key);
    }

    // Метод для сохранения перевода через кнопку в инспекторе
    [BoxGroup("Добавление нового значения")]
    [Button("Save Translation")]
    private void SaveTranslation()
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(language) || string.IsNullOrEmpty(value))
        {
            Debug.LogWarning("Key, language, or value is empty.");
            return;
        }

        // Путь к JSON-файлу локализации
        var jsonPath = $"Assets/Resources/Localization/{language}.json";
        var json = System.IO.File.Exists(jsonPath) ? System.IO.File.ReadAllText(jsonPath) : "{}";
        var data = JsonUtility.FromJson<LocalizationData>(json) 
                   ?? new LocalizationData { entries = new List<LocalizationEntry>() };

        // Проверяем, существует ли запись с таким ключом
        var existingEntry = data.entries.Find(entry => entry.key == key);
        if (existingEntry != null)
        {
            existingEntry.value = value; // Обновляем значение
        }
        else
        {
            data.entries.Add(new LocalizationEntry { key = key, value = value }); // Добавляем новую запись
        }

        // Сохраняем обновленный JSON
        var updatedJson = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(jsonPath, updatedJson);
        Debug.Log($"Translation saved for key '{key}' in language '{language}'.");
      
        language = string.Empty;
        value = string.Empty;
    }
}