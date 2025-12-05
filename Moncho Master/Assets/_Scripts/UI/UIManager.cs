using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Nombres de las escenas")]
    [SerializeField] private string gameplayScene = "Puesto";
    [SerializeField] private string mainMenuScene = "Main Menu";
    [SerializeField] private string creditsScene = "Creditos";

    [Header("Panel de opciones")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Panel de Pausa")]
    [SerializeField] private GameObject pausePanel;

    public void PlayGame()
    {
        AudioManager.Instance.Jugar();

        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.PLAY);
        SceneManager.LoadScene(gameplayScene);
    }

    public void Creditos()
    {
        AudioManager.Instance.ClickSelect();
        SceneManager.LoadScene(creditsScene);
    }

    public void MainMenu()
    {
        AudioManager.Instance.ClickSelect();
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameManager.GameState.MENU);
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ExitGame()
    {
        AudioManager.Instance.ClickAtras();
        Application.Quit();
    }

    public void OpenOptions()
    {
        AudioManager.Instance.ClickSelect();
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        AudioManager.Instance.ClickSelect();
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (GameManager.Instance == null) return;

        var currentState = GameManager.Instance.CurrentState;

        if (currentState == GameManager.GameState.PLAY)
        {
            OpenPausePanel();
        }
        else if (currentState == GameManager.GameState.PAUSE)
        {
            ClosePausePanel();
        }
    }

    public void OpenPausePanel()
    {
        AudioManager.Instance.ClickPausa();
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            if (GameManager.Instance != null)
                GameManager.Instance.PauseGame();
        }
    }

    public void ClosePausePanel()
    {
        AudioManager.Instance.ClickAtras();
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            if (GameManager.Instance != null)
                GameManager.Instance.ResumeGame();
        }
    }

    public void ResumeFromPause()
    {
        ClosePausePanel();
    }

    public void GoToMenuFromPause()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMenu();
    }
}