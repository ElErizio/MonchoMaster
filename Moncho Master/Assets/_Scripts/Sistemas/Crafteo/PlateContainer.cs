using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateContainer : MonoBehaviour
{
    [Serializable]
    public struct CategoryLimit
    {
        public IngredientCategory category;
        public int min;
        public int max;
    }

    [Header("Reglas del plato")]
    [SerializeField] private int capacity = 6;
    [SerializeField] private bool allowDuplicates = true;
    [SerializeField] private List<CategoryLimit> categoryLimits = new List<CategoryLimit>();
    [SerializeField] private bool locked = false;
    public bool IsLocked { get { return locked; } }
    public void SetLocked(bool value) { locked = value; }

    private readonly List<IngredientSO> _items = new List<IngredientSO>();
    public event Action OnContentChanged;

    public IReadOnlyList<IngredientSO> Items { get { return _items; } }

    private void OnValidate()
    {
        if (capacity < 0) capacity = 0;
    }

    public bool CanAccept(IngredientSO ing)
    {
        if (locked) return false;
        if (ing == null) return false;
        if (!ing.IsUnlocked) return false;
        if (_items.Count >= capacity) return false;

        if (!allowDuplicates)
        {
            for (int i = 0; i < _items.Count; i++)
                if (ReferenceEquals(_items[i], ing)) return false;
        }

        int countCat = 0;
        for (int i = 0; i < _items.Count; i++)
            if (_items[i].Category == ing.Category) countCat++;

        for (int i = 0; i < categoryLimits.Count; i++)
        {
            if (categoryLimits[i].category == ing.Category)
            {
                int max = categoryLimits[i].max;
                if (max > 0 && (countCat + 1) > max) return false;
                break;
            }
        }

        return true;
    }

    public bool TryAdd(IngredientSO ing)
    {
        if (!CanAccept(ing)) return false;
        _items.Add(ing);
        var h = OnContentChanged; if (h != null) h();
        return true;
    }

    public bool Remove(IngredientSO ing)
    {
        bool removed = _items.Remove(ing);
        if (removed) { var h = OnContentChanged; if (h != null) h(); }
        return removed;
    }

    public void Clear()
    {
        if (_items.Count == 0) return;
        _items.Clear();
        var h = OnContentChanged; if (h != null) h();
    }

    public string GetNormalizedKey()
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        for (int i = 0; i < _items.Count; i++)
        {
            IngredientSO ing = _items[i];
            if (ing == null || string.IsNullOrEmpty(ing.Id)) continue;
            int cur;
            if (counts.TryGetValue(ing.Id, out cur)) counts[ing.Id] = cur + 1;
            else counts[ing.Id] = 1;
        }

        List<string> parts = new List<string>(counts.Count);
        foreach (var kv in counts) parts.Add(kv.Key + ":" + kv.Value);
        parts.Sort(StringComparer.Ordinal);
        return string.Join("|", parts.ToArray());
    }

    public Dictionary<IngredientCategory, int> GetCategoryCounts()
    {
        Dictionary<IngredientCategory, int> counts = new Dictionary<IngredientCategory, int>();
        for (int i = 0; i < _items.Count; i++)
        {
            var cat = _items[i].Category;
            int cur;
            if (counts.TryGetValue(cat, out cur)) counts[cat] = cur + 1;
            else counts[cat] = 1;
        }
        return counts;
    }

    public void SetCapacity(int value) { capacity = Mathf.Max(0, value); }
    public void SetAllowDuplicates(bool value) { allowDuplicates = value; }
    public void SetCategoryLimit(IngredientCategory cat, int min, int max)
    {
        int idx = -1;
        for (int i = 0; i < categoryLimits.Count; i++)
            if (categoryLimits[i].category == cat) { idx = i; break; }

        CategoryLimit cl = new CategoryLimit { category = cat, min = min, max = max };
        if (idx >= 0) categoryLimits[idx] = cl; else categoryLimits.Add(cl);
    }
}
