using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private PlateContainer plate;
    [SerializeField] private RecipeIndex index;
    [SerializeField] private bool autoCraftOnMatch = true;

    public delegate void DishCrafted(RecipeSO.DishData dish, RecipeSO recipe);
    public event DishCrafted OnDishCrafted;

    private void OnEnable() { if (plate != null) plate.OnContentChanged += Evaluate; }
    private void OnDisable() { if (plate != null) plate.OnContentChanged -= Evaluate; }

    public void Evaluate()
    {
        if (plate == null || index == null) return;

        RecipeSO exactR;
        if (index.TryFindExact(plate.GetNormalizedKey(), out exactR) && IsValidPlateFor(exactR))
        {
            if (autoCraftOnMatch) Craft(exactR);
            return;
        }

        foreach (RecipeSO r in index.All)
        {
            if (r == null) continue;
            if (r.categoryRequirements == null || r.categoryRequirements.Count == 0) continue;

            if (MatchesByCategory(r) && IsValidPlateFor(r))
            {
                if (autoCraftOnMatch) Craft(r);
                return;
            }
        }
    }

    private bool IsValidPlateFor(RecipeSO r) { return true; }

    private bool MatchesByCategory(RecipeSO r)
    {
        var items = plate.Items;
        if (items == null || items.Count == 0) return false;
        if (r.categoryRequirements == null || r.categoryRequirements.Count == 0) return false;

        Dictionary<IngredientCategory, int> catCounts = new Dictionary<IngredientCategory, int>();
        for (int i = 0; i < items.Count; i++)
        {
            IngredientCategory cat = items[i].Category;
            int cur;
            if (catCounts.TryGetValue(cat, out cur)) catCounts[cat] = cur + 1;
            else catCounts[cat] = 1;
        }

        for (int i = 0; i < r.categoryRequirements.Count; i++)
        {
            var req = r.categoryRequirements[i];
            int have;
            if (!catCounts.TryGetValue(req.category, out have)) have = 0;

            if (have < req.min) return false;
            if (req.max > 0 && have > req.max) return false;
        }

        int requiredCount = 0;
        for (int i = 0; i < r.categoryRequirements.Count; i++)
        {
            int min = r.categoryRequirements[i].min;
            if (min > 0) requiredCount += min;
        }

        int extras = items.Count - requiredCount;
        if (extras < 0) return false;
        if (r.maxExtras >= 0 && extras > r.maxExtras) return false;

        return true;
    }

    public void Craft(RecipeSO r)
    {
        if (r == null) return;
        var dish = r.output;
        plate.Clear();
        var h = OnDishCrafted; if (h != null) h(dish, r);
        Debug.Log("¡Platillo creado! -> " + dish.displayName + " ($" + dish.price + ")");
    }
}
