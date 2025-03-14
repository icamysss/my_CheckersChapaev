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
            ThisGame.UpdatePawnsInteractivity(ThisGame.CurrentTurn);
        }

        public override void Exit()
        {
            base.Next();
            ThisGame.UpdatePawnsInteractivity(ThisGame.CurrentTurn, false);
        }
    }
}