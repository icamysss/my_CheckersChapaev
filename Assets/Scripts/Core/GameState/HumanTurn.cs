namespace Core.GameState
{
    public class HumanTurn : Turn

    {
        public HumanTurn(Game game) : base(game)
        {
        }

        public override void Enter()
        {
            base.Enter();
            ThisGame.SwitchPlayer();
            ThisGame.UpdatePawnsInteractivity(ThisGame.CurrentTurn);
            
        }

        public override void Exit()
        {
            base.Exit();
            ThisGame.UpdatePawnsInteractivity(ThisGame.CurrentTurn, false);
        }
    }
}