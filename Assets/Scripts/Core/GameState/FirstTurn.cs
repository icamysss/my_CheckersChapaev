using System.Threading;
using AI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.GameState
{
    /// <summary>
    ///  Выполнение первого хода, выбор цвета игрока, первичная настройки
    /// </summary>
    public class FirstTurn : Turn
    {
        private CancellationTokenSource cts;
        private AIController aiController;
        
        public FirstTurn(Game game, AIController aiController) : base(game)
        {
            this.aiController = aiController;
        }

        public override void Enter()
        {
            base.Enter();
           
            cts?.Cancel();
            cts = null;
            cts = new CancellationTokenSource();
            
            // ПРОВЕРЬ ОТПИСКУ
            Pawn.OnSelect += OnSelect;


            // инициализация доски
            ThisGame.Board.ClearBoard();
            ThisGame.Board.InitializeBoard(ThisGame);
            // инициализация типов игроков в зависимости от типа игры
            ThisGame.InitPlayerTypes();
            // Кто первый ходит
            ThisGame.WhoseTurnFirst();

           

            if (ThisGame.CurrentTurn.Type == PlayerType.AI)
            {
                Debug.Log("AI turn");
                // ИИ Выбирает цвет для хода и соответственно свой
                ThisGame.CurrentTurn.PawnColor = Random.Range(0, 2) == 0 ? PawnColor.Black : PawnColor.White;
                // другому игоку достается противоположный цвет
                var anotherPlayer = ThisGame.GetOppositePlayer(ThisGame.CurrentTurn);
                anotherPlayer.PawnColor = ThisGame.GetOppositeColor(ThisGame.CurrentTurn.PawnColor);
                
                ThisGame.OnStart?.Invoke();
                
                // ход ии
                UniTask.Void(async () => await aiController.MakeMove(ThisGame.CurrentTurn, cts.Token));
            }
            else
            {
                Debug.Log("Player turn");
                // Игрок выбирает цвет, все шашки активны, игрок ходит
                var allPawns = ThisGame.Board.Pawns;

                foreach (var pawn in allPawns)
                {
                    pawn.Interactable = true;
                }
                
                ThisGame.OnStart?.Invoke();
            }
        }

        public override void Exit()
        {
            base.Exit();
            Pawn.OnSelect -= OnSelect;
            cts?.Cancel();
        }
        
        private void OnSelect(Pawn pawn)
        {
            if (ThisGame.CurrentTurn == null)
            {
                Debug.LogWarning("Current player is null");
                return;
            }
            if (ThisGame.CurrentTurn.Type == PlayerType.AI) return; 
            
            // при первом клике текущий игрок получает цвет шашки
            ThisGame.CurrentTurn.PawnColor = pawn.PawnColor;
            // другой игрок противоположный цвет 
            var anotherPlayer = ThisGame.GetOppositePlayer(ThisGame.CurrentTurn);
            anotherPlayer.PawnColor = ThisGame.GetOppositeColor(pawn.PawnColor);
            // блокируем противоположный цвет 
            ThisGame.UpdatePawnsInteractivity(anotherPlayer, false );
           
        }
    }
}