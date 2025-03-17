using System;
using System.Collections.Generic;
using Services.Interfaces;
using UnityEngine;
using YG;

namespace Services
{
    public class LocalizationService : ILocalizationService
    {
    
        private Dictionary<string, string> localizationData = new Dictionary<string, string>();
        private string currentLanguage;
        public event Action LanguageChanged;
    
        public const string ENGLISH = "en";
        public const string RUSSIAN = "ru";
    
        public string Language
        {
            get => currentLanguage;
            set
            {
                currentLanguage = value;
                LoadLocalization(currentLanguage);
                LanguageChanged?.Invoke();
            }
        }
        // Загрузка локализации для указанного языка
        private void LoadLocalization(string language)
        {
            var json = LoadJsonFile(language);
            localizationData = ParseJson(json);
        }

        // Получение переведенной строки по ключу
        public string GetLocalizedString(string key)
        {
            return localizationData.GetValueOrDefault(key, key); // Если перевода нет, возвращаем сам ключ
        }

   

        // Загрузка JSON-файла из Resources
        private string LoadJsonFile(string language)
        {
            var textAsset = Resources.Load<TextAsset>($"Localization/{language}");
            return textAsset != null ? textAsset.text : "{}"; // Возвращаем пустой JSON, если файл не найден
        }

        // Парсинг JSON в словарь
        private Dictionary<string, string> ParseJson(string json)
        {
            var data = JsonUtility.FromJson<LocalizationData>(json);
            var dict = new Dictionary<string, string>();

            if (data?.entries == null) return dict;
        
            foreach (var entry in data.entries)
            {
                dict[entry.key] = entry.value;
            }
            return dict;
        }
        
        public void Initialize()
        {
            Language = YG2.envir.language;
            Debug.Log("Localization Service initialized");
            IsInitialized = true;
        }

        public void Shutdown()
        {
            Debug.Log("Shutting down Localization Service");
        }

        public bool IsInitialized { get; private set; }
    }
    
    // Вспомогательные классы для десериализации JSON
    [System.Serializable]
    public class LocalizationData
    {
        public List<LocalizationEntry> entries;
    }

    [System.Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string value;
    }
}