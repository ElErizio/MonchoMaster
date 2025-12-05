using UnityEngine;
using Moncho.Orders;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Services")]
    [SerializeField] private UnlockService unlockService;
    [SerializeField] private AudioManager audioManager;

    [Header("Score System")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int pointsPerSuccess = 10;
    [SerializeField] private int pointsPerFailure = -5;

    [Header("Order Tracking")]
    [SerializeField] private int totalOrders = 0;
    [SerializeField] private int successfulOrders = 0;
    [SerializeField] private int failedOrders = 0;

    [Header("Game Conditions")]
    [SerializeField] private int maxFailedOrders = 3;
    [SerializeField] private int winningScore = 100;

    private Text scoreText;
    private Text ordersText;
    private Text orderDetailsText;

    private GameState currentState = GameState.PLAY;
    private NPCOrderService.OrderSpec _currentOrder;

    public enum GameState { PLAY, PAUSE, MENU, GAME_OVER }

    public GameState CurrentState => currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeServices();
        Time.timeScale = 1f;
    }

    private void Start()
    {
    }

    private void InitializeServices()
    {
        if (unlockService == null)
            unlockService = GetComponentInChildren<UnlockService>();

        if (audioManager == null)
            audioManager = GetComponentInChildren<AudioManager>();
    }

    public void SetGameState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.PLAY:
                Time.timeScale = 1f;
                break;
            case GameState.PAUSE:
                Time.timeScale = 0f;
                break;
            case GameState.MENU:
                Time.timeScale = 1f;
                break;
            case GameState.GAME_OVER:
                Time.timeScale = 0f;
                break;
        }

        Debug.Log($"Estado del juego cambiado a: {currentState}");
    }

    public void TogglePause()
    {
        if (currentState == GameState.PLAY)
        {
            SetGameState(GameState.PAUSE);
        }
        else if (currentState == GameState.PAUSE)
        {
            SetGameState(GameState.PLAY);
        }
    }

    public void PauseGame()
    {
        SetGameState(GameState.PAUSE);
    }

    public void ResumeGame()
    {
        SetGameState(GameState.PLAY);
    }

    public void GoToMenu()
    {
        SetGameState(GameState.MENU);
        SceneManager.LoadScene("Main Menu");
    }

    public void OnNewOrderGenerated(NPCOrderService.OrderSpec newOrder)
    {
        _currentOrder = newOrder;
        totalOrders++;

        if (orderDetailsText != null)
        {
            orderDetailsText.text = "¡Nuevo Pedido!";
        }

        if (audioManager != null)
        {
            audioManager.PlaySFX("NewOrder");
        }

        Debug.Log($"Nueva orden generada! Total de órdenes: {totalOrders}");
    }

    public void OnOrderCompleted(NPCOrderService.OrderSpec completedOrder, bool wasSuccessful)
    {
        if (currentState != GameState.PLAY) return;

        if (wasSuccessful)
        {
            successfulOrders++;
            currentScore += pointsPerSuccess;

            Debug.Log($"¡Orden completada con éxito! +{pointsPerSuccess} puntos");

            if (audioManager != null)
            {
                audioManager.PlaySFX("OrderSuccess");
            }

            if (orderDetailsText != null)
            {
                orderDetailsText.text = "¡Correcto! +" + pointsPerSuccess + " puntos";
            }
        }
        else
        {
            failedOrders++;
            currentScore += pointsPerFailure;

            Debug.Log($"Orden fallida! {pointsPerFailure} puntos");

            if (audioManager != null)
            {
                audioManager.PlaySFX("OrderFail");
            }

            if (orderDetailsText != null)
            {
                orderDetailsText.text = "Incorrecto! " + pointsPerFailure + " puntos";
            }
        }

        currentScore = Mathf.Max(0, currentScore);

        CheckGameConditions();
    }


    private void CheckGameConditions()
    {
        if (currentState == GameState.GAME_OVER) return;

        if (currentScore >= winningScore)
        {
            SetGameState(GameState.GAME_OVER);
            if (orderDetailsText != null)
                orderDetailsText.text = "¡VICTORIA!";

            SceneManager.LoadScene("Moncho Master");
            return;
        }

        if (failedOrders >= maxFailedOrders)
        {
            SetGameState(GameState.GAME_OVER);
            if (orderDetailsText != null)
                orderDetailsText.text = "GAME OVER";

            SceneManager.LoadScene("Fail Master");
            return;
        }
    }

    public void AddBonusPoints(int bonus)
    {
        if (currentState == GameState.PLAY)
        {
            currentScore += bonus;
            CheckGameConditions();
        }
    }

    public void ResetGame()
    {
        currentScore = 0;
        totalOrders = 0;
        successfulOrders = 0;
        failedOrders = 0;
        SetGameState(GameState.PLAY);
        if (orderDetailsText != null)
            orderDetailsText.text = "¡Comienza a cocinar!";
    }

    public int GetCurrentScore() => currentScore;
    public int GetSuccessfulOrders() => successfulOrders;
    public int GetTotalOrders() => totalOrders;
    public float GetSuccessRate() => totalOrders > 0 ? (float)successfulOrders / totalOrders : 0f;
    public NPCOrderService.OrderSpec GetCurrentOrder() => _currentOrder;

    public UnlockService GetUnlockService() => unlockService;
    public AudioManager GetAudioManager() => audioManager;
}