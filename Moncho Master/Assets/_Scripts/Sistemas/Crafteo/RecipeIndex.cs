using System;
using System.Collections.Generic;
using UnityEngine;

public class RecipeIndex : MonoBehaviour
{
    [SerializeField] private List<RecipeSO> recipes = new List<RecipeSO>();
    private Dictionary<string, RecipeSO> _exactIndex = new Dictionary<string, RecipeSO>();

    private void Awake() { RebuildIndex(); }

    public void RebuildIndex()
    {
        _exactIndex.Clear();
        for (int i = 0; i < recipes.Count; i++)
        {
            RecipeSO r = recipes[i];
            if (r == null || !r.isUnlocked) continue;
            if (r.useExactIngredients && r.exactIngredients != null && r.exactIngredients.Count > 0)
            {
                string key = BuildKey(r);
                if (!string.IsNullOrEmpty(key)) _exactIndex[key] = r;
            }
        }
    }

    private static string BuildKey(RecipeSO r)
    {
        List<string> ids = new List<string>();
        for (int i = 0; i < r.exactIngredients.Count; i++)
        {
            var req = r.exactIngredients[i];
            if (req.ingredient == null || req.quantity <= 0) continue;
            for (int q = 0; q < req.quantity; q++) ids.Add(req.ingredient.Id);
        }
        if (ids.Count == 0) return null;

        ids.Sort(StringComparer.Ordinal);

        Dictionary<string, int> counts = new Dictionary<string, int>();
        for (int i = 0; i < ids.Count; i++)
        {
            string id = ids[i];
            int cur;
            if (counts.TryGetValue(id, out cur)) counts[id] = cur + 1;
            else counts[id] = 1;
        }

        List<string> parts = new List<string>(counts.Count);
        foreach (var kv in counts) parts.Add(kv.Key + ":" + kv.Value);
        parts.Sort(StringComparer.Ordinal);
        return string.Join("|", parts.ToArray());
    }

    public bool TryFindExact(string normalizedKey, out RecipeSO recipe)
    {
        return _exactIndex.TryGetValue(normalizedKey, out recipe);
    }

    public IEnumerable<RecipeSO> All
    {
        get
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                RecipeSO r = recipes[i];
                if (r != null && r.isUnlocked) yield return r;
            }
        }
    }
}