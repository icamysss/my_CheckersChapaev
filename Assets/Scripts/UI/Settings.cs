using Services;
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
        private string language = "ru";
        private float volume;
       
        
        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            uiManager = manager;
            
            okButton.onClick.AddListener(PressOk);
            leftButton.onClick.AddListener(NextLanguage);
            rightButton.onClick.AddListener(NextLanguage);
            // громкость
            volume = uiManager.GetVolume();
            //  язык
            language = uiManager.GetLanguage();
            lang.sprite = language == "ru" ? rusSprite : engSprite;
        }

        private void PressOk()
        {
            uiManager.SetLanguage(language);
            uiManager.SetVolume(volume);
            uiManager.CloseMenu("Settings");
        }

        private void NextLanguage()
        {
            language = language == "ru" ? "en" : "ru";
            lang.sprite = language == "ru" ? rusSprite : engSprite;
            uiManager.SetLanguage(language);
        }

        // public void PreviousLanguage()
        // {
        //     
        // }
    }
}