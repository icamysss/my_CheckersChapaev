using System;
using Core;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGame : Menu
    {
        [SerializeField] private Text whiteCount, blackCount;
        [SerializeField] private Text whoTurn;
        
        private Game game;
        private IGameManager gameManager;

        public override void Initialize()
        {
            base.Initialize();
           
            whoTurn.text = "";
            
            gameManager = ServiceLocator.Get<IGameManager>();
            game = gameManager.CurrentGame;

            game.OnStartTurn += UpdateUI;
            game.OnGameStart += OnStartTurn;
            game.OnGameEnd += OnGameEnded;
            

            whiteCount.text = string.Empty;
            blackCount.text = string.Empty;
            whoTurn.text = string.Empty;
        }

        private void OnStartTurn()
        {
            whoTurn.text = "Выберите шашку";
        }
        private void UpdateUI()
        {
            whiteCount.text = game.Board.GetPawnsOnBoard(PawnColor.White).Count.ToString();
            blackCount.text = game.Board.GetPawnsOnBoard(PawnColor.Black).Count.ToString();

            switch (game.CurrentTurn.PawnColor)
            {
                case PawnColor.None:
                    whoTurn.text = "Выберите шашку для хода";
                    break;
                case PawnColor.Black:
                    whoTurn.text = "Ходят черные";
                    break;
                case PawnColor.White:
                    whoTurn.text = "Ходят белые";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ;
        }

        private void OnGameEnded(PawnColor winColor)
        {
            UpdateUI();
            switch (winColor)
            {
                case PawnColor.None:
                    whoTurn.text = "Ничья";
                    break;
                default:
                    whoTurn.text = $"Победил игрок: {winColor} ";
                    break;
            }
        }

        private void OnDestroy()
        {
            game.OnStartTurn -= UpdateUI;
            game.OnGameStart -= OnStartTurn;
            game.OnGameEnd -= OnGameEnded;
        }
        
        public void BackToMainMenu()
        {
            gameManager.CurrentState = GameState.MainMenu;
        }
    }
}