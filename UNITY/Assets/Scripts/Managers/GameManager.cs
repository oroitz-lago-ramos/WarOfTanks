using UnityEngine;
using WarOfTanks.StateMachine;
using WarOfTanks.Zone;

public class GameManager : SingletonBehaviour<GameManager>
{
    [Header("Debug")]
    [SerializeField] private bool _enableLogs = false;

    [Header("Match Settings")]
    [SerializeField] private float _matchDuration = 180f;
    [SerializeField] private int _scoreLimit = 100;

    [Header("References")]
    [SerializeField] private Zone _zone;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameOverScreen _gameOverScreen;

    private TeamManager _teamManager;
    private ScoreManager _scoreManager;
    private MatchTimer _matchTimer;
    private StateMachine<GameManager> _stateMachine;
    private bool _matchEnded;

    protected override void Awake()
    {
        base.Awake();
        ApplyDebugSettings();
        _teamManager = new TeamManager();
        _scoreManager = new ScoreManager(_scoreLimit);
        _matchTimer = new MatchTimer(_matchDuration);
    }
    private void OnDestroy()
    {
        _zone.OnZoneScored -= OnZoneScored;
    }
    private void Start()
    {
        Tank[] tanks = FindObjectsOfType<Tank>();
        foreach (Tank tank in tanks)
        {
            _teamManager.RegisterTank(tank, (int)tank.TeamId);
            tank.OnDied += () => OnTankDestroyed(tank);
        }
        _zone.OnZoneScored += OnZoneScored;
        _stateMachine = new GameStateMachine(this);
    }
    private void Update()
    {
        _stateMachine.Update();
        _matchTimer.Tick(Time.deltaTime);
        if (!_matchEnded && _matchTimer.IsTimeUp)
        {
            _matchEnded = true;
            _stateMachine.ChangeState(new GameOverState(_stateMachine));
        }
    }
    private void OnValidate()
    {
        ApplyDebugSettings();
    }

    private void ApplyDebugSettings()
    {
        DebugLogger.IsEnabled = _enableLogs;
    }

    #region GameLoop Methods
    public void StartMatch() { _matchTimer.StartTimer(); }
    public void PauseMatch() { _matchTimer.PauseTimer(); }
    public void EndMatch() { _matchTimer.PauseTimer(); }

    public void OnZoneScored(int teamId)
    {
        _scoreManager.AddZoneScore(teamId, 1);
        if (!_matchEnded && _scoreManager.HasTeamWon(teamId))
        {
            _matchEnded = true;
            _stateMachine.ChangeState(new GameOverState(_stateMachine));
        }
    }

    public void OnTankDestroyed(Tank tank) { _scoreManager.AddKillScore((int)tank.TeamId); }
    public int GetWinner() { return _scoreManager.GetLeadingTeam(); }
    public int GetScore(int teamId) { return _scoreManager.GetScore(teamId); }
    public float GetRemainingTime() { return _matchTimer.RemainingTime; }
    public void SetPauseUI(bool active) { _pausePanel.SetActive(active); }
    public void ShowGameOver() { _gameOverScreen.Show(GetWinner(), GetScore(0), GetScore(1)); }
    #endregion
}
