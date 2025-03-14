using System;
using System.Threading;
using AI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.GameState
{
    public abstract class Turn: GameState
    {
        private CancellationTokenSource cts;
        private const int TURN_DELAY_MS = 1500; // Задержка смены хода, что б шашки успели улететь
        protected Turn(Game game) : base(game)
        {
        }

        public override void Enter()
        {
            base.Enter();

            Pawn.OnEndAiming += OnKickPawn;
            ThisGame.OnStartTurn?.Invoke();
        }

        public override void Exit()
        {
            base.Exit();
            Pawn.OnEndAiming -= OnKickPawn;
            CancelAsync();
            ThisGame.OnEndTurn?.Invoke();
        }

        public override void Next()
        {
            base.Next();
            SwitchNextState();
        }

        
        // вызывают Next с ожиданием TURN_DELAY_MS, после удара по шашке
        private void OnKickPawn()
        {
            CancelAsync();
            cts = new CancellationTokenSource();
            
            UniTask.Void(async () => await HandleKickAsync(cts)); 
        }
        private async UniTask HandleKickAsync(CancellationTokenSource ct)
        {
            try
            {
                await UniTask.Delay(TURN_DELAY_MS, cancellationToken: ct.Token );
                Next();
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("HandleKickAsync Cancelled");
            }
        }

        private void CancelAsync()
        {
            cts?.Cancel();
            cts = null;
        }
        
        /// <summary>
        /// Переключаем на следущее состояние в зависимости от текущего игрока
        /// Переключаетель хода 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void SwitchNextState()
        {
            ThisGame.SwitchPlayer();
            // если сейчас ходит второй игрок 
            var currentPlayer = ThisGame.GetOppositePlayer(ThisGame.CurrentTurn);

            // в зависимости от типа следующего игрока выбираем состояние
            switch (currentPlayer.Type)
            {
                case PlayerType.Human:

                    ThisGame.CurrentState = ThisGame.HumanMove;
                    break;
                case PlayerType.AI:

                    ThisGame.CurrentState = ThisGame.AIMove;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}