using Services;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Settings: Menu
    {
        [Header("Components")]
        [SerializeField] private Button okButton, leftButton, rightButton;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Image lang;
        
        [Header("Sprites")]
        [SerializeField] private Sprite rusSprite;
        [SerializeField] private Sprite engSprite;

        private UIManager uiManager;
        private ILocalizationService localizationService;
        private IAudioService audioService;
        
        private string language = LocalizationService.RUSSIAN;
        private float currentVolume;
       
        
        
        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            
            uiManager = manager;
            localizationService = uiManager.LocalizationService;
            audioService = uiManager.AudioService;
            
            okButton.onClick.AddListener(PressOk);
            leftButton.onClick.AddListener(NextLanguage);
            rightButton.onClick.AddListener(NextLanguage);
            // громкость
            currentVolume = audioService.Volume;
            volumeSlider.value = currentVolume;
            //  язык
            language = localizationService.Language;
            lang.sprite = language == "ru" ? rusSprite : engSprite;
        }

        private void PressOk()
        {
            localizationService.Language = language;
            audioService.Volume = currentVolume;
            uiManager.CloseMenu("Settings");
        }

        private void NextLanguage()
        {
            language = language == "ru" ? "en" : "ru";
            lang.sprite = language == "ru" ? rusSprite : engSprite;
            localizationService.Language = language;
        }
    }
}