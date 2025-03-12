using System;

namespace Services.Interfaces
{
    /// <summary>
    /// Интерфейс для сервиса локализации, наследующий IService.
    /// Определяет методы и события для управления переводами в игре.
    /// </summary>
    public interface ILocalizationService : IService
    {
        /// <summary>
        /// Устанавливает текущий язык для локализации.
        /// </summary>
        /// <param name="languageCode">Код языка (например, "en", "ru").</param>
        void SetLanguage(string languageCode);

        /// <summary>
        /// Возвращает переведенную строку по заданному ключу.
        /// </summary>
        /// <param name="key">Ключ перевода.</param>
        /// <returns>Переведенная строка или ключ, если перевод не найден.</returns>
        string GetLocalizedString(string key);

        /// <summary>
        /// Событие, вызываемое при смене языка.
        /// </summary>
        event Action LanguageChanged;
    }
}