using System;
using UnityEngine;

[DisallowMultipleComponent]
public class LoteriaService : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CraftingManager crafting;

    [Header("Tablero")]
    [SerializeField] private int rows = 3;
    [SerializeField] private int cols = 3;

    [Serializable]
    public struct Cell
    {
        public string ingredientId;
        public bool marked;
    }

    [SerializeField] private Cell[] _board;

    public delegate void BoardChanged();
    public event BoardChanged OnBoardChanged;

    public int Rows { get { return rows; } }
    public int Cols { get { return cols; } }
    public Cell[] Snapshot { get { return _board; } }

    private void Awake()
    {
        BuildBoard();
    }

    void OnEnable()
    {
        if (crafting != null) crafting.OnDishDelivered += HandleDelivered;
    }

    void OnDisable()
    {
        if (crafting != null) crafting.OnDishDelivered -= HandleDelivered;
    }

    private void HandleDelivered(CraftingManager.DeliveryPayload p)
    {
        bool isOrderCorrect = IsOrderCorrect();

        if (!isOrderCorrect)
        {
            Debug.Log("[Loteria] Pedido incorrecto - no se marca ninguna casilla");
            return;
        }

        int n = _board != null ? _board.Length : 0;
        if (n == 0) return;

        int free = 0;
        for (int i = 0; i < n; i++) if (!_board[i].marked) free++;
        if (free == 0) { BuildBoard(); return; }

        int targetFreeIndex = UnityEngine.Random.Range(0, free);
        int seen = 0;
        for (int i = 0; i < n; i++)
        {
            if (_board[i].marked) continue;
            if (seen == targetFreeIndex)
            {
                _board[i].marked = true;
                break;
            }
            seen++;
        }

        OnBoardChanged?.Invoke();

        if (HasLineComplete())
        {
            UnlockRandomIngredient();
            BuildBoard();
        }
    }

    private bool IsOrderCorrect()
    {
        if (OrderManager.Instance != null)
        {
            return OrderManager.Instance.IsCurrentOrderCorrect;
        }

        Debug.LogWarning("[Loteria] OrderManager no encontrado. Revisa la configuración.");
        return false;
    }

    private void BuildBoard()
    {
        if (rows <= 0) rows = 3;
        if (cols <= 0) cols = 3;
        int n = rows * cols;
        if (_board == null || _board.Length != n) _board = new Cell[n];

        var pool = BuildUnlockedPool();
        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning("[Loteria] Sin pool de ingredientes para el tablero. Revisa catalogForBoard / UnlockService.");
            for (int i = 0; i < n; i++) { _board[i].ingredientId = null; _board[i].marked = false; }
            OnBoardChanged?.Invoke();
            return;
        }

        for (int i = 0; i < n; i++)
        {
            _board[i].ingredientId = PickRandomIngredientId(pool);
            _board[i].marked = false;
        }

        OnBoardChanged?.Invoke();
    }

    private IngredientSO[] BuildUnlockedPool()
    {
        if (catalogForBoard == null || catalogForBoard.Length == 0)
            return Array.Empty<IngredientSO>();

        if (UnlockService.Instance == null)
            return catalogForBoard;

        var list = new System.Collections.Generic.List<IngredientSO>();
        foreach (var ing in catalogForBoard)
            if (ing != null && UnlockService.Instance.IsUnlocked(ing))
                list.Add(ing);

        if (list.Count == 0)
            list.AddRange(catalogForBoard);

        return list.ToArray();
    }

    [SerializeField] private IngredientSO[] catalogForBoard;

    private string PickRandomIngredientId(IngredientSO[] pool)
    {
        if (pool == null || pool.Length == 0) return string.Empty;
        int idx = UnityEngine.Random.Range(0, pool.Length);
        var ing = pool[idx];
        return (ing != null && !string.IsNullOrEmpty(ing.Id)) ? ing.Id : (ing != null ? ing.name : "unknown");
    }

    private bool HasLineComplete()
    {
        for (int r = 0; r < rows; r++)
        {
            bool all = true;
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;
                if (!_board[idx].marked) { all = false; break; }
            }
            if (all) return true;
        }
        for (int c = 0; c < cols; c++)
        {
            bool all = true;
            for (int r = 0; r < rows; r++)
            {
                int idx = r * cols + c;
                if (!_board[idx].marked) { all = false; break; }
            }
            if (all) return true;
        }
        return false;
    }

    private void UnlockRandomIngredient()
    {
        if (UnlockService.Instance == null) return;
        var locked = UnlockService.Instance.GetRandomLocked();
        if (locked == null)
        {
            Debug.Log("[Loteria] No hay ingredientes bloqueados para desbloquear.");
            return;
        }
        bool ok = UnlockService.Instance.Unlock(locked);
        if (ok) Debug.Log("[Loteria] Desbloqueado por línea: " + locked.Id, locked);
    }

    [ContextMenu("DEBUG/Mark Random")]
    private void DebugMarkRandom()
    {
        if (OrderManager.Instance != null)
            OrderManager.Instance.SetOrderResult(true);
        HandleDelivered(new CraftingManager.DeliveryPayload { matched = true });
    }
}