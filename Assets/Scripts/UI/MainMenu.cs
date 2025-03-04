using Core;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : Menu
    {
        [SerializeField] Button AiVSAiButton;  // обычная игра
        [SerializeField] Button HvsHButton;
        
        private IGameManager  gameManager;
       
        
        
        public override void Initialize()
        {
            base.Initialize();
            canvasGroup.ignoreParentGroups = true;
            gameManager = ServiceLocator.Get<IGameManager>();
           
            AiVSAiButton.onClick.AddListener(StartGame);
            HvsHButton.onClick.AddListener(StartGameHvsH);
        }

        private void StartGame()
        {
            gameManager.CurrentGame.StartGame(GameType.AiVsAi);
        }
        
        private void StartGameHvsH()
        {
            gameManager.CurrentGame.StartGame(GameType.HumanVsHuman);
        }
        
        
    }
}