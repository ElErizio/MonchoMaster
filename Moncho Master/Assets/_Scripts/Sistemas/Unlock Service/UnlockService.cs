using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class UnlockService : MonoBehaviour
{
    public static UnlockService Instance { get; private set; }

    [Header("Catálogo")]
    [SerializeField] private IngredientSO[] allIngredients;
    [SerializeField] private IngredientSO[] initiallyUnlocked;

    [Header("Opcional: Guardado")]
    [SerializeField] private bool usePlayerPrefs = false;
    [SerializeField] private string prefsKey = "MM_IngredientUnlocks";

    private HashSet<string> _unlocked = new HashSet<string>();
    private Dictionary<string, IngredientSO> ingredientsById;

    public delegate void UnlocksChanged();
    public event UnlocksChanged OnUnlocksChanged;

    public int UnlockedCount => _unlocked.Count;
    public int TotalIngredients => ingredientsById?.Count ?? 0;
    public float Progress => TotalIngredients > 0 ? (float)UnlockedCount / TotalIngredients : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeDictionary();
        LoadState();
        StartCoroutine(NotifyReadyNextFrame());
    }

    [ContextMenu("Debug All Ingredients State")]
    public void DebugAllIngredientsState()
    {
        Debug.Log("=== DEBUG UNLOCK SERVICE ===");
        Debug.Log($"Total ingredientes en catálogo: {ingredientsById?.Count ?? 0}");
        Debug.Log($"Ingredientes desbloqueados: {_unlocked.Count}");

        if (allIngredients != null)
        {
            foreach (var ing in allIngredients)
            {
                if (ing != null)
                {
                    bool unlocked = IsUnlocked(ing);
                    Debug.Log($"{ing.name} (ID: {ing.Id}) - {(unlocked ? "DESBLOQUEADO" : "BLOQUEADO")}");
                }
            }
        }
    }

    public Sprite GetIngredientCard(string ingredientId)
    {
        return ingredientsById != null
            && ingredientsById.TryGetValue(ingredientId, out var so)
            ? so.Card
            : null;
    }

    public Sprite GetIngredientIcon(string ingredientId)
    {
        return ingredientsById != null
            && ingredientsById.TryGetValue(ingredientId, out var so)
            ? so.Icon
            : null;
    }

    private void InitializeDictionary()
    {
        ingredientsById = new Dictionary<string, IngredientSO>();
        foreach (var ing in allIngredients ?? Array.Empty<IngredientSO>())
        {
            if (ing != null && !string.IsNullOrEmpty(ing.Id))
                ingredientsById[ing.Id] = ing;
        }
    }

    private void LoadState()
    {
        _unlocked.Clear();

        if (usePlayerPrefs && PlayerPrefs.HasKey(prefsKey))
        {
            string csv = PlayerPrefs.GetString(prefsKey, "");
            if (!string.IsNullOrEmpty(csv))
            {
                string[] parts = csv.Split(',');
                foreach (string id in parts)
                {
                    string trimmedId = id.Trim();
                    if (!string.IsNullOrEmpty(trimmedId) && ingredientsById.ContainsKey(trimmedId))
                        _unlocked.Add(trimmedId);
                }
            }
        }
        else
        {
            foreach (var ing in initiallyUnlocked ?? Array.Empty<IngredientSO>())
            {
                if (ing != null && !string.IsNullOrEmpty(ing.Id) && ingredientsById.ContainsKey(ing.Id))
                    _unlocked.Add(ing.Id);
            }
        }
    }

    private void SaveState()
    {
        if (!usePlayerPrefs) return;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        bool first = true;
        foreach (var id in _unlocked)
        {
            if (!first) sb.Append(",");
            first = false;
            sb.Append(id);
        }
        PlayerPrefs.SetString(prefsKey, sb.ToString());
        PlayerPrefs.Save();
    }

    public bool IsUnlocked(IngredientSO ing)
    {
        return _unlocked.Contains(ing.Id);
    }

    public bool Unlock(IngredientSO ing)
    {
        if (ing == null || string.IsNullOrEmpty(ing.Id)) return false;
        if (!ingredientsById.ContainsKey(ing.Id))
        {
            Debug.LogWarning($"[Unlocks] Intento de desbloquear ingrediente no catalogado: {ing.Id}");
            return false;
        }

        if (_unlocked.Contains(ing.Id)) return false;

        _unlocked.Add(ing.Id);
        SaveState();
        OnUnlocksChanged?.Invoke();
        Debug.Log($"[Unlocks] Desbloqueado: {ing.Id} ({_unlocked.Count}/{TotalIngredients})", ing);
        return true;
    }

    private void OnValidate()
    {
        if (allIngredients != null)
        {
            var ids = new HashSet<string>();
            foreach (var ing in allIngredients)
            {
                if (ing != null && !string.IsNullOrEmpty(ing.Id))
                {
                    if (ids.Contains(ing.Id))
                        Debug.LogError($"ID duplicado encontrado: {ing.Id}", this);
                    else
                        ids.Add(ing.Id);
                }
            }
        }
    }

    public IngredientSO GetRandomLocked()
    {
        List<IngredientSO> locked = new List<IngredientSO>();
        foreach (var kvp in ingredientsById)
        {
            var ing = kvp.Value;
            if (!_unlocked.Contains(ing.Id)) locked.Add(ing);
        }
        if (locked.Count == 0) return null;
        int idx = UnityEngine.Random.Range(0, locked.Count);
        return locked[idx];
    }

    public IngredientSO[] FilterUnlocked(IngredientSO[] pool)
    {
        if (pool == null || pool.Length == 0) return new IngredientSO[0];
        List<IngredientSO> res = new List<IngredientSO>();
        for (int i = 0; i < pool.Length; i++)
        {
            var ing = pool[i];
            if (IsUnlocked(ing)) res.Add(ing);
        }
        return res.ToArray();
    }

    public IngredientSO FindById(string id)
    {
        if (string.IsNullOrEmpty(id) || ingredientsById == null) return null;
        ingredientsById.TryGetValue(id, out var result);
        return result;
    }

    [ContextMenu("Reset to Initially Unlocked (Editor)")]
    private void ResetToInitiallyUnlocked()
    {
        #if UNITY_EDITOR
                _unlocked.Clear();
                foreach (var ing in initiallyUnlocked ?? Array.Empty<IngredientSO>())
                {
                    if (ing != null && !string.IsNullOrEmpty(ing.Id))
                        _unlocked.Add(ing.Id);
                }
                SaveState();
                OnUnlocksChanged?.Invoke();
                Debug.Log($"[Unlocks] Reset a {_unlocked.Count} ingredientes iniciales.");
        #endif
    }

    public IReadOnlyCollection<string> GetUnlockedIds()
    {
        return _unlocked.ToList().AsReadOnly();
    }

    private System.Collections.IEnumerator NotifyReadyNextFrame()
    {
        yield return null;
        var h = OnUnlocksChanged; if (h != null) h();
    }

    //Punto Safe
}