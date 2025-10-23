using UnityEngine;
using UnityEngine.Events;

public class OrderTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 45f;

    [Header("Recipe Pool")]
    [Tooltip("Lista de recetas que pueden salir aleatoriamente.")]
    [SerializeField] private RecipeSO[] recipePool;

    [Header("References")]
    [SerializeField] private CraftingManager craftingManager;

    private float startTime;
    private bool isRunning;
    private RecipeSO currentRecipe;

    public UnityEvent<float> OnTimerTick;
    public UnityEvent OnTimerEnd;
    public UnityEvent<RecipeSO> OnNewRecipe;

    private void OnEnable()
    {
        if (craftingManager != null)
            craftingManager.OnDishDelivered += HandleDishDelivered;
    }

    private void OnDisable()
    {
        if (craftingManager != null)
            craftingManager.OnDishDelivered -= HandleDishDelivered;
    }

    private void Start()
    {
        StartNewOrder();
    }

    private void Update()
    {
        if (!isRunning) return;

        float elapsed = Time.time - startTime;
        float remaining = Mathf.Max(0, timeLimit - elapsed);

        OnTimerTick?.Invoke(remaining);

        if (remaining <= 0f)
        {
            TimerEnd();
        }
    }

    private void TimerEnd()
    {
        isRunning = false;
        OnTimerEnd?.Invoke();

        StartNewOrder();
    }

    private void HandleDishDelivered(CraftingManager.DeliveryPayload payload)
    {
        StartNewOrder();
    }

    public void StartNewOrder()
    {
        currentRecipe = GetRandomRecipeFromPool();
        if (currentRecipe != null)
            OnNewRecipe?.Invoke(currentRecipe);

        ResetTimer();
    }

    private void ResetTimer()
    {
        startTime = Time.time;
        isRunning = true;
    }

    private RecipeSO GetRandomRecipeFromPool()
    {
        if (recipePool == null || recipePool.Length == 0)
        {
            Debug.LogWarning("[OrderTimer] No hay recetas en el pool local. Asigna recetas en el inspector.");
            return null;
        }

        int randomIndex = Random.Range(0, recipePool.Length);
        return recipePool[randomIndex];
    }
}
