using System;
using Core;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EndGame : Menu
    {
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button cancelButton;
        
        [SerializeField] private Text header;
        [SerializeField] private Text mainText;
        
        private Game game;

        private void OnEnable()
        {
            if (mainMenuButton == null ||
                restartButton == null ||
                cancelButton == null  ||
                header == null || 
                mainText == null) throw new NullReferenceException("One or more fields in EndGame ui are null");
        }

        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);
            game = manager.GameManager.CurrentGame;

            header.text = string.Empty;
            mainText.text = string.Empty;
           
            cancelButton.onClick.AddListener(ToMainMenu);
            mainMenuButton.onClick.AddListener(ToMainMenu);
            restartButton.onClick.AddListener(RestartGame);


            UpdateUI();
        }

        private void ToMainMenu()
        {
            uiManager.GameManager.CurrentState = GameState.MainMenu;
        }

        private void RestartGame()
        {
            game.StartGame(game.GameType);
        }

        private void UpdateUI()
        {
            if (uiManager.GameManager.CurrentState != GameState.GameOver) return;
            // заголовок
            header.text = uiManager.LocalizationService.GetLocalizedString("END_GAME");
            if (game.Winner == null)
            {   // ничья
                var result = uiManager.LocalizationService.GetLocalizedString("DRAW");
                mainText.text = result;
            }
            else
            {   // есть победитель
                var winPlayer = uiManager.LocalizationService.GetLocalizedString("WIN_PLAYER"); // = Победил игрок
                mainText.text = $"{winPlayer} : {game.Winner.Name}";
            }
           
        }
    }
}