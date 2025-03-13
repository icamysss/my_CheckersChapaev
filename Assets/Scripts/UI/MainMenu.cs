using Core;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : Menu
    {
        [SerializeField] private Button single;  // обычная игра
        [SerializeField] private Button onWeb;
        [SerializeField] private Button two;
        [SerializeField] private Button settings;
        private Game game;
        
        
        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            
            uiManager = manager;
            game = uiManager.GameManager.CurrentGame;
            
            canvasGroup.ignoreParentGroups = true;
            
            single.onClick.AddListener(SingleGame);
            onWeb.onClick.AddListener(OnWebGame);
            two.onClick.AddListener(HumanVSHuman);
            settings.onClick.AddListener(OpenSettings);
        }

        private void HumanVSHuman()
        {
            game.StartGame(GameType.HumanVsHuman);
        }
        private void SingleGame()
        {
            game.StartGame(GameType.HumanVsAi);
        }
        private void OnWebGame()
        {
            game.StartGame(GameType.OnWeb);
        }
        private void OpenSettings()
        {
            uiManager.OpenMenu("Settings");
        }
        
    }
}