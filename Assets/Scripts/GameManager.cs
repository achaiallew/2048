using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public TileBoard board;
    public CanvasGroup gameOver;
    public CanvasGroup winScreen;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;

    private int score;

    public Button undo; 

    private bool checkforWin = true;

    void Start()
    {
        int difficulty = PlayerPrefs.GetInt("Difficulty", 0);

        if (difficulty == 1) // Hard
        {
            undo.interactable = false; // Disable Undo
        }
        else if (difficulty == 0)
        {
            undo.interactable = true; // Enable Undo
        }

        NewGame();
    }

    void Update()
    {
        if (checkforWin == true)
        {
            if (board.winState == true)
            {
                WinScreen();
                checkforWin = false;
            }
        }
        
    }

    public void NewGame()
    {
        Debug.Log("NewGame called!");
        SetScore(0);
        highscoreText.text = LoadHighScore().ToString();
        gameOver.alpha = 0f;
        gameOver.interactable = false;
        gameOver.GameObject().SetActive(false);
        winScreen.alpha = 0f;
        winScreen.interactable = false;
        winScreen.GameObject().SetActive(false);
        board.ClearBoard();
        board.SpawnTile();
        board.SpawnTile(); 
        board.enabled = true;
    }

    public void GameOver()
    {
        gameOver.GameObject().SetActive(true);
        board.enabled = false;
        gameOver.interactable = true;
        HighScore();

        StartCoroutine(Fade(gameOver, 1f, 1f));
    }

    public void WinScreen()
    {
        winScreen.GameObject().SetActive(true);
        board.enabled = false;
        winScreen.interactable = true;
        StartCoroutine(Fade(winScreen, 1f, 0.5f));
    }

    public void Continue()
    {
        winScreen.alpha = 0f;
        winScreen.interactable = false;
        board.enabled = true;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }


    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.1f;
        float from = canvasGroup.alpha;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    //Track Score
    public void TrackScore(int points)
    {
        SetScore(score + points);
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
        int highscore = LoadHighScore();

        if (score > highscore)
        {
            highscoreText.text = score.ToString();
        }
    }

    private void HighScore()
    {
        int highscore = LoadHighScore();

        if (score > highscore)
        {
            PlayerPrefs.SetInt("highscore", score);
        }
    }

    private int LoadHighScore()
    {
        return PlayerPrefs.GetInt("highscore", 0);
    }

    // Undo
    public int GetScore()
    {
        return score;
    }

    public void SetScoreFromUndo(int value)
    {
        score = value;
        scoreText.text = score.ToString();
    }

}
