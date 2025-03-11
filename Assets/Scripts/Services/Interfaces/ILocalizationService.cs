using System;
using Cysharp.Threading.Tasks;

namespace Services
{
    public interface ILocalizationService : IService
    {
        // Системные события
        event Action OnLanguageChanged;
        
        // Основные функции
        string GetTranslation(string key);
        void SetLanguage(string language);
        UniTaskVoid PreloadTranslationsAsync();
        
        // Локализация ассетов
        UnityEngine.Sprite GetLocalizedSprite(string assetKey);
        
        // Для UI интеграции
        void RegisterLocalizedUI(Action updateAction);
        void UnregisterLocalizedUI(Action updateAction);
    }
}