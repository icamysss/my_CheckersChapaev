using Core;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : Menu
    {
        [SerializeField] Button Single;  // обычная игра
        [SerializeField] Button OnWeb;
        [SerializeField] Button Two;
        [SerializeField] Button Settings;
        
        private UIManager  uiManager;
       
        
        
        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            uiManager = manager;
            
            canvasGroup.ignoreParentGroups = true;
            Single.onClick.AddListener(SingleGame);
            OnWeb.onClick.AddListener(OnWebGame);
            Two.onClick.AddListener(HumanVSHuman);
            Settings.onClick.AddListener(OpenSettings);
        }

        private void HumanVSHuman()
        {
            uiManager.StartGame(GameType.HumanVsHuman);
        }
        private void SingleGame()
        {
            uiManager.StartGame(GameType.HumanVsAi);
        }
        private void OnWebGame()
        {
            uiManager.StartGame(GameType.OnWeb);
        }
        private void OpenSettings()
        {
            uiManager.OpenMenu("Settings");
        }
        
    }
}