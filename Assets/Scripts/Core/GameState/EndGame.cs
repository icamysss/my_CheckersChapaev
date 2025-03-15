namespace Core.GameState
{
    public class EndGame: GameState
    {
        public EndGame(Game game) : base(game)
        {
        }

        public override void Enter()
        {
            base.Enter();
            ThisGame.OnEndGame?.Invoke();
        }
    }
}