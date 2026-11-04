using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;
    public Button goToMenuBtn;

    private void Start()
    {
        GameData.RecordHighScore();
        bool newBest = GameData.playerScore > 0 && GameData.playerScore >= GameData.highScore;
        gameOverText.text = "Game Over\nScore: " + GameData.playerScore +
            "\n<size=60%>" + (newBest ? "New high score!" : "Best: " + GameData.highScore) + "</size>";
        GameAudio.Play(Sfx.GameOver, 0.9f, 0f);

        goToMenuBtn.onClick.AddListener(GoToMenu);
    }

    public void GoToMenu()
    {
        GameData.ClearPrepProgress(); // reset kitchen-prep results; keeps high score and volume
        GameData.ResetGame();
        Time.timeScale = 1;
        SceneFader.Load("MainMenu");
    }
}
