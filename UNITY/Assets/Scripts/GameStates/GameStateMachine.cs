
using WarOfTanks.StateMachine;

public class GameStateMachine : StateMachine<GameManager>
{
    public GameStateMachine(GameManager manager) : base(manager)
    {
        // Start in the "neutral" state, where the game is in its initial state.
        ChangeState(new PlayingState(this));
    }
}
