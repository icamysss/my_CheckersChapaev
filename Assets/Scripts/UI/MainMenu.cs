using Core;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : Menu
    {
        [SerializeField] Button startGameButton;  // обычная игра
        
        private IGameManager  gameManager;
       
        
        
        public override void Initialize()
        {
            base.Initialize();
            canvasGroup.ignoreParentGroups = true;
            gameManager = ServiceLocator.Get<IGameManager>();
           
            startGameButton.onClick.AddListener(StartGame);
        }

        private void StartGame()
        {
            gameManager.CurrentGame.StartGame(GameType.AiVsAi);
        }
    }
}