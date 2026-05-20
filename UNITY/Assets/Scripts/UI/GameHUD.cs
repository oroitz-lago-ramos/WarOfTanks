using UnityEngine;
using TMPro;
using System;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreTeamAText;
    [SerializeField] private TextMeshProUGUI _scoreTeamBText;
    [SerializeField] private TextMeshProUGUI _timerText;

    private void Update()
    {
        if (GameManager.Instance == null) return;
        _scoreTeamAText.text = GameManager.Instance.GetScore(0).ToString();
        _scoreTeamBText.text = GameManager.Instance.GetScore(1).ToString();
        float remaining = GameManager.Instance.GetRemainingTime();
        _timerText.text = TimeSpan.FromSeconds(remaining).ToString(@"mm\:ss");
    }
}