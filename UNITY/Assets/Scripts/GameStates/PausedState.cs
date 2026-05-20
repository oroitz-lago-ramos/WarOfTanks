using WarOfTanks.StateMachine;
using UnityEngine;
public class PausedState : State<GameManager>
{
    public PausedState(StateMachine<GameManager> machine) : base(machine) { }

    protected override void Enter()
    {
        Context.PauseMatch();
        Context.SetPauseUI(true);
    }
    protected override void Execute()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Machine.ChangeState(new PlayingState(Machine));
    }

    protected override void Exit()
    {
        Context.SetPauseUI(false);
    }
}
