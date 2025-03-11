using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Services
{
    public class LocalizationService : ILocalizationService
    {
        #region Fields and Properties

        private string _currentLanguage = Languages.RUSSIAN;
        private Dictionary<string, string> _translations = new();
        private Dictionary<string, UnityEngine.Sprite> _localizedSprites = new();
        private HashSet<Action> _uiUpdateCallbacks = new();

        public bool isInitialized { get; private set; }

        #endregion

        #region IService Implementation

        public void Initialize()
        {
            // Автоматическое определение языка (заглушка)
            DetectSystemLanguage();
            
            // Асинхронная загрузка переводов
            PreloadTranslationsAsync().Forget();
            
            isInitialized = true;
        }

        public void Shutdown()
        {
            _translations.Clear();
            _localizedSprites.Clear();
            _uiUpdateCallbacks.Clear();
            isInitialized = false;
        }

        #endregion

        #region ILocalizationService Implementation

        public event Action OnLanguageChanged;

        public string GetTranslation(string key)
        {
            if (_translations.TryGetValue(key, out var value))
                return value;

            Debug.LogWarning($"Translation missing for key: {key}");
            return key; // Возвращаем ключ как fallback
        }

        public void SetLanguage(string language)
        {
            if (_currentLanguage == language) return;
            
            _currentLanguage = language;
            SaveLanguagePreference(); // Заглушка
            ReloadTranslations();
            OnLanguageChanged?.Invoke();
        }

        public async UniTaskVoid PreloadTranslationsAsync()
        {
            try
            {
                // Загрузка JSON (пример для WebGL/Resources)
                var jsonPath = $"Locales/{_currentLanguage}";
                var jsonAsset = await Resources.LoadAsync<TextAsset>(jsonPath).ToUniTask();
                
                if (jsonAsset is TextAsset textAsset)
                {
                    ParseTranslations(textAsset.text);
                }

                // Загрузка спрайтов (пример)
                await LoadLocalizedSpritesAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Localization loading failed: {ex.Message}");
            }
        }

        public UnityEngine.Sprite GetLocalizedSprite(string assetKey)
        {
            return _localizedSprites.TryGetValue(assetKey, out var sprite) 
                ? sprite 
                : null;
        }

        public void RegisterLocalizedUI(Action updateAction)
        {
            _uiUpdateCallbacks.Add(updateAction);
            OnLanguageChanged += updateAction;
        }

        public void UnregisterLocalizedUI(Action updateAction)
        {
            _uiUpdateCallbacks.Remove(updateAction);
            OnLanguageChanged -= updateAction;
        }

        #endregion

        #region Private Methods

        private void DetectSystemLanguage()
        {
            // Заглушка для автоматического определения
            // _currentLanguage = Application.systemLanguage;
        }

        private void SaveLanguagePreference()
        {
            // Заглушка для PlayerPrefs/SaveSystem
        }

        private void ParseTranslations(string json)
        {
            // Пример парсинга (реализовать под свой JSON)
            _translations = JsonUtility.FromJson<TranslationData>(json).ToDictionary();
        }

        private async UniTask LoadLocalizedSpritesAsync()
        {
            // Пример загрузки спрайтов через Addressables/Resources
            var spriteKeys = new[] { "icon_play", "icon_settings" };
            foreach (var key in spriteKeys)
            {
                var path = $"Sprites/{_currentLanguage}/{key}";
                var sprite = await Resources.LoadAsync<UnityEngine.Sprite>(path).ToUniTask() as UnityEngine.Sprite;
                if (sprite != null)
                    _localizedSprites[key] = sprite;
            }
        }

        private void ReloadTranslations()
        {
            PreloadTranslationsAsync().Forget();
            foreach (var callback in _uiUpdateCallbacks)
                callback?.Invoke();
        }

        #endregion

        #region Editor Integration

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Open Localization Editor")]
        private static void OpenEditorWindow()
        {
            // Пример вызова кастомного редактора
            LocalizationEditorWindow.Open();
        }
#endif

        #endregion
    }

    // Вспомогательный класс для парсинга JSON
    [Serializable]
    public class TranslationData
    {
        public List<TranslationEntry> entries;

        public Dictionary<string, string> ToDictionary() => 
            entries.ToDictionary(e => e.key, e => e.value);
    }

    [Serializable]
    public class TranslationEntry
    {
        public string key;
        public string value;
    }
}