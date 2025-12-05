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

    private GameState currentGameState = GameState.Playing;
    private NPCOrderService.OrderSpec _currentOrder;

    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Menu
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeServices();
    }

    private void Start()
    {
        //UpdateUI();
    }

    private void InitializeServices()
    {
        if (unlockService == null)
            unlockService = GetComponentInChildren<UnlockService>();

        if (audioManager == null)
            audioManager = GetComponentInChildren<AudioManager>();
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
        //UpdateUI();
    }

    public void OnOrderCompleted(NPCOrderService.OrderSpec completedOrder, bool wasSuccessful)
    {
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

        //UpdateUI();
        CheckGameConditions();
    }

    /*private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Puntos: {currentScore}";

        if (ordersText != null)
        {
            float successRate = totalOrders > 0 ? (float)successfulOrders / totalOrders * 100 : 0f;
            ordersText.text = $"Éxito: {successfulOrders}/{totalOrders} ({successRate:F1}%)";
        }
    }*/

    private void CheckGameConditions()
    {
        if (currentScore >= winningScore)
        {
            currentGameState = GameState.GameOver;
            if (orderDetailsText != null)
                orderDetailsText.text = "¡VICTORIA!";

            SceneManager.LoadScene("Moncho Master");
            return;
        }

        if (failedOrders >= maxFailedOrders)
        {
            currentGameState = GameState.GameOver;
            if (orderDetailsText != null)
                orderDetailsText.text = "GAME OVER";

            SceneManager.LoadScene("Fail Master");
            return;
        }
    }

    public void PauseGame()
    {
        currentGameState = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        currentGameState = GameState.Playing;
        Time.timeScale = 1f;
    }


    public void AddBonusPoints(int bonus)
    {
        currentScore += bonus;
        //UpdateUI();
        CheckGameConditions(); // Verificar condiciones después de agregar puntos
    }

    public void ResetGame()
    {
        currentScore = 0;
        totalOrders = 0;
        successfulOrders = 0;
        failedOrders = 0;
        currentGameState = GameState.Playing;
        Time.timeScale = 1f;
        if (orderDetailsText != null)
            orderDetailsText.text = "¡Comienza a cocinar!";
        //UpdateUI();
    }
    public int GetCurrentScore() => currentScore;
    public int GetSuccessfulOrders() => successfulOrders;
    public int GetTotalOrders() => totalOrders;
    public float GetSuccessRate() => totalOrders > 0 ? (float)successfulOrders / totalOrders : 0f;
    public NPCOrderService.OrderSpec GetCurrentOrder() => _currentOrder;

    public UnlockService GetUnlockService() => unlockService;
    public AudioManager GetAudioManager() => audioManager;
    public GameState GetCurrentState() => currentGameState;
}