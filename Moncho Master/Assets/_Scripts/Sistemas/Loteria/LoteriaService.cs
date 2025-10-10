using System;
using UnityEngine;

[DisallowMultipleComponent]
public class LoteriaService : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private UnlockService unlocks;
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

    private void OnEnable()
    {
        if (crafting != null) crafting.OnDishDelivered += HandleDelivered;
        if (unlocks != null) unlocks.OnUnlocksChanged += HandleUnlocksChanged;
        BuildBoard();
    }

    private void OnDisable()
    {
        if (crafting != null) crafting.OnDishDelivered -= HandleDelivered;
        if (unlocks != null) unlocks.OnUnlocksChanged -= HandleUnlocksChanged;
    }

    private void HandleUnlocksChanged()
    {
        BuildBoard();
    }

    private void BuildBoard()
    {
        if (rows <= 0) rows = 3;
        if (cols <= 0) cols = 3;
        int n = rows * cols;
        _board = new Cell[n];

        IngredientSO[] pool = (unlocks != null) ? unlocks.FilterUnlocked(null) : null;
        pool = BuildUnlockedPool();

        for (int i = 0; i < n; i++)
        {
            _board[i].ingredientId = PickRandomIngredientId(pool);
            _board[i].marked = false;
        }
        var h = OnBoardChanged; if (h != null) h();
    }

    private IngredientSO[] BuildUnlockedPool()
    {
        if (unlocks == null) return new IngredientSO[0];
        return _cachedUnlockedFromCatalog();
    }

    [SerializeField] private IngredientSO[] catalogForBoard;
    private IngredientSO[] _cachedUnlockedFromCatalog()
    {
        if (catalogForBoard == null) return new IngredientSO[0];
        System.Collections.Generic.List<IngredientSO> res = new System.Collections.Generic.List<IngredientSO>();
        for (int i = 0; i < catalogForBoard.Length; i++)
        {
            var ing = catalogForBoard[i];
            if (ing != null && unlocks != null && unlocks.IsUnlocked(ing)) res.Add(ing);
        }
        return res.ToArray();
    }

    private string PickRandomIngredientId(IngredientSO[] pool)
    {
        if (pool == null || pool.Length == 0) return null;
        int idx = UnityEngine.Random.Range(0, pool.Length);
        var ing = pool[idx];
        return (ing != null) ? ing.Id : null;
    }

    private void HandleDelivered(CraftingManager.DeliveryPayload p)
    {
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

        var h1 = OnBoardChanged; if (h1 != null) h1();

        if (HasLineComplete())
        {
            UnlockRandomIngredient();
            BuildBoard();
        }
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
        if (unlocks == null) return;
        var locked = unlocks.GetRandomLocked();
        if (locked == null)
        {
            Debug.Log("[Loteria] No hay ingredientes bloqueados para desbloquear.");
            return;
        }
        bool ok = unlocks.Unlock(locked);
        if (ok) Debug.Log("[Loteria] Desbloqueado por línea: " + locked.Id, locked);
    }

    [ContextMenu("DEBUG/Mark Random")]
    private void DebugMarkRandom() { HandleDelivered(new CraftingManager.DeliveryPayload { matched = true }); }
}
