using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Nombres de las escenas")]
    [SerializeField] private string gameplayScene = "GameScene";
    [SerializeField] private string mainMenuScene = "MainMenuScene";
    [SerializeField] private string creditsScene = "Creditos";

    [Header("Panel de opciones")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Panel de Pausa")]
    [SerializeField] private GameObject pausePanel;

    public void PlayGame()
    {
        SceneManager.LoadScene(gameplayScene);
    }
    public void Creditos()
    {
        SceneManager.LoadScene(creditsScene);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ExitGame()
    { 
        Application.Quit();
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }
    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void OpenPausePanel()
    {
            pausePanel.SetActive(true);
    }

    public void ClosePausePanel()
    {
            pausePanel.SetActive(false);
    }
}

