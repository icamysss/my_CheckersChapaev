using System;

namespace Services.Interfaces
{
    public interface ILocalizationService : IService
    {
        string Language { get; set; }
        string GetLocalizedString(string key);
        event Action LanguageChanged;
    }
}