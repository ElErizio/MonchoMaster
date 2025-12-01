using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    private bool _isCurrentOrderCorrect = false;
    private bool _hasOrderBeenProcessed = false;

    public bool IsCurrentOrderCorrect { get => _isCurrentOrderCorrect; }
    public bool HasOrderBeenProcessed { get => _hasOrderBeenProcessed; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetOrderResult(bool isCorrect)
    {
        _isCurrentOrderCorrect = isCorrect;
        _hasOrderBeenProcessed = true;
        Debug.Log($"[OrderManager] Order result set to: {isCorrect}");
    }

    public void ResetOrder()
    {
        _isCurrentOrderCorrect = false;
        _hasOrderBeenProcessed = false;
    }

    public void ForceOrderState(bool isCorrect, bool isProcessed)
    {
        _isCurrentOrderCorrect = isCorrect;
        _hasOrderBeenProcessed = isProcessed;
    }
}