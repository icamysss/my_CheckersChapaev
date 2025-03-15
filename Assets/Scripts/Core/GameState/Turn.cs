using System;
using System.Threading;
using AI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.GameState
{
    public abstract class Turn : GameState
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
                await UniTask.Delay(TURN_DELAY_MS, cancellationToken: ct.Token);
                if (cts.IsCancellationRequested) return;
                
                ThisGame.ChangeState(GetNextState());
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
        private GameState GetNextState()
        {
            var anotherPlayer = ThisGame.GetOppositePlayer(ThisGame.CurrentTurn);

            // в зависимости от типа следующего игрока выбираем состояние
            switch (anotherPlayer.Type)
            {
                case PlayerType.Human:
                    return ThisGame.HumanMove;

                case PlayerType.AI:
                    return ThisGame.AIMove;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }
}