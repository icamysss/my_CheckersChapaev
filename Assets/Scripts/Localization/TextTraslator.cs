using Services;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Localization
{
    public class TextTraslator : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private string key;

        private ILocalizationService localizationService;

        #region Unity

        private void Start()
        {
            if (ServiceLocator.AllServicesRegistered)
            {
                localizationService = ServiceLocator.Get<ILocalizationService>();
            }
            else
            {
                ServiceLocator.OnAllServicesRegistered += OnRegistered;
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.OnAllServicesRegistered -= OnRegistered;
            localizationService.LanguageChanged -= OnChangeLanguage;
        }
        
        private void OnValidate()
        {
            if (!text) text = GetComponent<Text>();
            key ??= text.name;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                localizationService.SetLanguage(Languages.RUSSIAN);
            }
        }

        #endregion

        private void OnChangeLanguage()
        {
            text.text = localizationService.GetLocalizedString(key);
        }

        private void OnRegistered()
        {
            localizationService = ServiceLocator.Get<ILocalizationService>();
            localizationService.LanguageChanged += OnChangeLanguage;
        }
    }
}