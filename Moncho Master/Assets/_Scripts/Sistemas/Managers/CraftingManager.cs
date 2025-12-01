using System.Collections.Generic;
using UnityEngine;
using Moncho.Orders;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private PlateContainer plate;
    [SerializeField] private RecipeIndex index;
    [SerializeField] private NPCOrderService npcOrderService;
    [SerializeField] private bool autoCraftOnMatch = true;

    public delegate void DishCrafted(RecipeSO.DishData dish, RecipeSO recipe);
    public event DishCrafted OnDishCrafted;
    public delegate void DishDelivered(DeliveryPayload payload);
    public event DishDelivered OnDishDelivered;

    public struct DeliveryPayload
    {
        public bool matched;
        public RecipeSO recipe;
        public RecipeSO.DishData dish;
        public string normalizedKey;
        public string[] ingredientIds;
    }

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

    public bool ConfirmAndDeliver()
    {
        if (plate == null || index == null) return false;
        var items = plate.Items;
        if (items == null || items.Count == 0) return false;

        plate.SetLocked(true);

        string normKey = plate.GetNormalizedKey();
        string[] ids = new string[items.Count];
        for (int i = 0; i < items.Count; i++)
            ids[i] = (items[i] != null) ? items[i].Id : "null";

        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.ResetOrder();
        }

        bool npcValidation = false;
        DeliveryPayload validationPayload = new DeliveryPayload
        {
            ingredientIds = ids,
            normalizedKey = normKey
        };

        if (npcOrderService != null)
        {
            npcValidation = npcOrderService.ValidateCurrentOrder(validationPayload);
            Debug.Log($"[CraftingManager] Validación NPCOrderService: {npcValidation}");
        }

        RecipeSO match;
        bool craftingValidation = false;
        DeliveryPayload p = new DeliveryPayload();

        if (index.TryFindExact(normKey, out match) && IsValidPlateFor(match))
        {
            craftingValidation = true;
            p.recipe = match;
            p.dish = match.output;
            Craft(match);
        }
        else
        {
            foreach (var r in index.All)
            {
                if (r == null) continue;
                if (r.categoryRequirements == null || r.categoryRequirements.Count == 0) continue;
                if (MatchesByCategory(r) && IsValidPlateFor(r))
                {
                    craftingValidation = true;
                    p.recipe = r;
                    p.dish = r.output;
                    Craft(r);
                    break;
                }
            }
        }

        if (!craftingValidation)
        {
            p.recipe = null;
            p.dish = new RecipeSO.DishData
            {
                id = "custom_" + normKey,
                displayName = "Platillo improvisado",
                icon = null,
                price = 0
            };
            plate.Clear();
        }

        bool finalMatched = npcOrderService != null ? npcValidation : craftingValidation;
        p.matched = finalMatched;
        p.normalizedKey = normKey;
        p.ingredientIds = ids;

        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.SetOrderResult(finalMatched);
            Debug.Log($"[CraftingManager] OrderManager notificado con: {finalMatched} (NPC: {npcValidation}, Crafting: {craftingValidation})");
        }

        var h = OnDishDelivered;
        if (h != null) h(p);

        plate.SetLocked(false);
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

    [ContextMenu("DEBUG/Force Correct Delivery")]
    private void DebugForceCorrectDelivery()
    {
        if (OrderManager.Instance != null)
            OrderManager.Instance.SetOrderResult(true);

        var payload = new DeliveryPayload
        {
            matched = true,
            dish = new RecipeSO.DishData { displayName = "DEBUG Dish" }
        };

        var h = OnDishDelivered;
        if (h != null) h(payload);
    }

    [ContextMenu("DEBUG/Force Incorrect Delivery")]
    private void DebugForceIncorrectDelivery()
    {
        if (OrderManager.Instance != null)
            OrderManager.Instance.SetOrderResult(false);

        var payload = new DeliveryPayload
        {
            matched = false,
            dish = new RecipeSO.DishData { displayName = "DEBUG Failed Dish" }
        };

        var h = OnDishDelivered;
        if (h != null) h(payload);
    }
}