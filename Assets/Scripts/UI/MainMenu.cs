using Core;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : Menu
    {
        [SerializeField] Button AiVSAiButton;  // обычная игра
        [SerializeField] Button HvsHButton;
        [SerializeField] Button HvsAiButton;
        
        private IGameManager  gameManager;
       
        
        
        public override void Initialize()
        {
            base.Initialize();
            canvasGroup.ignoreParentGroups = true;
            gameManager = ServiceLocator.Get<IGameManager>();
           
            AiVSAiButton.onClick.AddListener(AiVSAi);
            HvsHButton.onClick.AddListener(HumanVSHuman);
            HvsAiButton.onClick.AddListener(HumanVsAi);
        }

        private void HumanVSHuman()
        {
            gameManager.CurrentGame.StartGame(GameType.HumanVsHuman);
        }
        
        private void AiVSAi()
        {
            gameManager.CurrentGame.StartGame(GameType.AiVsAi);
        }
        private void HumanVsAi()
        {
            gameManager.CurrentGame.StartGame(GameType.HumanVsAi);
        }
        
        
    }
}