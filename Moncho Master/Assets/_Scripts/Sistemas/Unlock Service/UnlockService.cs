using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]

[DefaultExecutionOrder(-200)]
public class UnlockService : MonoBehaviour
{
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

    private void Awake()
    {
        LoadState();
    }

    private void Start()
    {
        StartCoroutine(NotifyReadyNextFrame());
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
                for (int i = 0; i < parts.Length; i++)
                {
                    string id = parts[i];
                    if (!string.IsNullOrEmpty(id)) _unlocked.Add(id);
                }
            }
        }
        else
        {
            if (initiallyUnlocked != null)
            {
                for (int i = 0; i < initiallyUnlocked.Length; i++)
                {
                    var ing = initiallyUnlocked[i];
                    if (ing != null && !string.IsNullOrEmpty(ing.Id)) _unlocked.Add(ing.Id);
                }
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
        if (ing == null || string.IsNullOrEmpty(ing.Id)) return false;
        return _unlocked.Contains(ing.Id);
    }

    public bool Unlock(IngredientSO ing)
    {
        if (ing == null || string.IsNullOrEmpty(ing.Id)) return false;
        if (_unlocked.Contains(ing.Id)) return false;
        _unlocked.Add(ing.Id);
        SaveState();
        var h = OnUnlocksChanged; if (h != null) h();
        Debug.Log("[Unlocks] Desbloqueado: " + ing.Id, ing);
        return true;
    }

    public IngredientSO GetRandomLocked()
    {
        List<IngredientSO> locked = new List<IngredientSO>();
        if (allIngredients != null)
        {
            for (int i = 0; i < allIngredients.Length; i++)
            {
                var ing = allIngredients[i];
                if (ing == null || string.IsNullOrEmpty(ing.Id)) continue;
                if (!_unlocked.Contains(ing.Id)) locked.Add(ing);
            }
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
        if (string.IsNullOrEmpty(id) || allIngredients == null) return null;
        for (int i = 0; i < allIngredients.Length; i++)
        {
            var ing = allIngredients[i];
            if (ing != null && ing.Id == id) return ing;
        }
        return null;
    }

    [ContextMenu("Reset to Initially Unlocked (Editor)")]
    private void ResetToInitiallyUnlocked()
    {
        _unlocked.Clear();
        if (initiallyUnlocked != null)
            for (int i = 0; i < initiallyUnlocked.Length; i++)
                if (initiallyUnlocked[i] != null && !string.IsNullOrEmpty(initiallyUnlocked[i].Id))
                    _unlocked.Add(initiallyUnlocked[i].Id);
        SaveState();
        var h = OnUnlocksChanged; if (h != null) h();
        Debug.Log("[Unlocks] Reset a iniciales.");
    }

    private System.Collections.IEnumerator NotifyReadyNextFrame()
    {
        yield return null; // 1 frame
        var h = OnUnlocksChanged; if (h != null) h();
    }
}
