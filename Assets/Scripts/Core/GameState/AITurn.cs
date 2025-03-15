using System.Threading;
using AI;
using Cysharp.Threading.Tasks;

namespace Core.GameState
{
    public class AITurn : Turn
    {
        private readonly AIController aiController;
        private CancellationTokenSource cts;
        public AITurn(Game game, AIController aiController) : base(game)
        {
            this.aiController = aiController;
        }

        public override void Enter()
        {
            cts?.Cancel();
            cts = null;
            cts = new CancellationTokenSource();
            
            base.Enter();
            // меняем игрока 
            ThisGame.SwitchPlayer();
            // ход ии
            UniTask.Void(async () => await aiController.MakeMove(ThisGame.CurrentTurn, cts.Token));
        }

        public override void Exit()
        {
            base.Exit();
            cts?.Cancel();
        }
    }
}