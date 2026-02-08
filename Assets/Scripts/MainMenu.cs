using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI highScore;

    public TMP_Dropdown difficultyDropdown;

    public TextMeshProUGUI difficultyWarning;

    void Start()
    {
        highScore.text = PlayerPrefs.GetInt("highscore").ToString();
        PlayerPrefs.SetInt("Difficulty", 0);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GamePlay");
    }

    public void OnDifficultyChanged()
    {
        if (difficultyDropdown.value == 1)
        {
            difficultyWarning.enabled = true;
            PlayerPrefs.SetInt("Difficulty", difficultyDropdown.value);
        }
        else
        {
            difficultyWarning.enabled = false;
            PlayerPrefs.SetInt("Difficulty", 0);
        }
        PlayerPrefs.Save(); 
    }

}
