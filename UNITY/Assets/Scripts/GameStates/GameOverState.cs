using WarOfTanks.StateMachine;
using UnityEngine;

public class GameOverState : State<GameManager>
{
    public GameOverState(StateMachine<GameManager> machine) : base(machine) {}
    protected override void Enter()
    {
        Context.EndMatch();
        Context.ShowGameOver
    }
    protected override void Exit() { }
}

