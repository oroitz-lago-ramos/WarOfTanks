using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private TextMeshProUGUI _scoreTeamAText;
    [SerializeField] private TextMeshProUGUI _scoreTeamBText;

    public void Show(int winner, int scoreA, int scoreB)
    {
        _panel.SetActive(true);
        _winnerText.text = winner == 0 ? "Player Team Wins!" : winner == 1 ? "Enemy Team Wins!" : "Draw!";
        _scoreTeamAText.text = "Player: " + scoreA;
        _scoreTeamBText.text = "Enemy: " + scoreB;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}