using System;
using UnityEngine;
using System.Collections.Generic;
using Localization;
using Services.Interfaces;

namespace Services
{
    public class LocalizationService : ILocalizationService
    {
        private Dictionary<string, Dictionary<string, string>> _translations;
        private string currentLanguage = Languages.RUSSIAN;
        private const string FALLBACK_LANGUAGE = Languages.RUSSIAN;

        public event Action LanguageChanged;

        public void Initialize()
        {
            LoadAllTranslations();
            LoadSavedLanguage();
            isInitialized = true;
            Debug.Log("LocalizationService initialized");
        }

        private void LoadAllTranslations()
        {
            _translations = new Dictionary<string, Dictionary<string, string>>();

            var translationFiles = Resources.LoadAll<TextAsset>("Localization");
            foreach (var file in translationFiles)
            {
                string langCode = file.name;
                _translations[langCode] = JsonUtility.FromJson<SerializationDictionary>(file.text).ToDictionary();
            }
        }

        private void LoadSavedLanguage()
        {
            string savedLang = PlayerPrefs.GetString("SelectedLanguage", currentLanguage);
            SetLanguage(savedLang);
        }

        public void SetLanguage(string languageCode)
        {
            if (!_translations.ContainsKey(languageCode))
                languageCode = FALLBACK_LANGUAGE;

            currentLanguage = languageCode;
            // PlayerPrefs.SetString("SelectedLanguage", languageCode);
            LanguageChanged?.Invoke();
        }

        public string GetLocalizedString(string key)
        {
            if (_translations[currentLanguage].TryGetValue(key, out var value))
                return value;

            Debug.LogWarning($"Translation missing: {key}");
            return key;
        }

        public string GetCurrentLanguage() => currentLanguage;

        public void Shutdown()
        {
            Debug.Log("Shutting down LocalizationService");
        }

        public bool isInitialized { get; private set; }

        [System.Serializable]
        private class SerializationDictionary
        {
            public List<string> keys = new List<string>();
            public List<string> values = new List<string>();

            public Dictionary<string, string> ToDictionary()
            {
                var dict = new Dictionary<string, string>();
                for (int i = 0; i < keys.Count; i++)
                    dict[keys[i]] = values[i];
                return dict;
            }
        }
    }
}