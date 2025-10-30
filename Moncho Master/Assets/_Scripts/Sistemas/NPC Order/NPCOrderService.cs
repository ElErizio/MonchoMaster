using System;
using UnityEngine;

namespace Moncho.Orders
{
    public class NPCOrderService : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private CraftingManager crafting;
        [SerializeField] private UnlockService unlocks;

        [Header("Pools de ingredientes permitidos")]
        [SerializeField] private IngredientSO[] baseOptions;
        [SerializeField] private IngredientSO[] salsaOptions;
        [SerializeField] private IngredientSO[] toppingOptions;

        [Header("Reglas de pedido")]
        [SerializeField] private int minSalsas = 1;
        [SerializeField] private int maxSalsas = 2;
        [SerializeField] private int minToppings = 1;
        [SerializeField] private int maxToppings = 2;
        [SerializeField] private bool allowExtrasOnDelivery = true;

        [Header("Limite practico")]
        [SerializeField] private int maxItemsPerOrder = 6;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;

        [Serializable]
        public struct OrderSpec
        {
            public IngredientSO baseIng;
            public IngredientSO[] salsas;
            public IngredientSO[] toppings;
        }

        private OrderSpec _current;

        public delegate void OrderEvent(OrderSpec spec);
        public event OrderEvent OnOrderChanged;
        public event OrderEvent OnOrderFulfilled;
        public event OrderEvent OnOrderFailed;

        public OrderSpec CurrentOrder { get { return _current; } }

        private void OnEnable()
        {
            if (crafting != null) crafting.OnDishDelivered += HandleDelivered;
            GenerateNextOrder();
        }
        private void OnDisable()
        {
            if (crafting != null) crafting.OnDishDelivered -= HandleDelivered;
        }

        public void GenerateNextOrder()
        {
            if (!ValidatePools()) return;

            IngredientSO[] basePool = (unlocks != null) ? unlocks.FilterUnlocked(baseOptions) : baseOptions;
            IngredientSO[] salsaPool = (unlocks != null) ? unlocks.FilterUnlocked(salsaOptions) : salsaOptions;
            IngredientSO[] toppingPool = (unlocks != null) ? unlocks.FilterUnlocked(toppingOptions) : toppingOptions;

            if (basePool == null || basePool.Length == 0)
            {
                Debug.LogWarning("[Orders] No hay bases desbloqueadas. Revisa UnlockService.initiallyUnlocked.", this);
                return;
            }
            if (salsaPool == null || salsaPool.Length == 0)
            {
                Debug.LogWarning("[Orders] No hay salsas desbloqueadas. Revisa UnlockService o la loteria.", this);
                return;
            }
            if (toppingPool == null || toppingPool.Length == 0)
            {
                Debug.LogWarning("[Orders] No hay toppings desbloqueados. Revisa UnlockService o la loteria.", this);
                return;
            }

            int sMin = Mathf.Max(1, minSalsas);
            int tMin = Mathf.Max(1, minToppings);

            int sMaxClamp = Mathf.Clamp(maxSalsas, sMin, salsaPool.Length);
            int tMaxClamp = Mathf.Clamp(maxToppings, tMin, toppingPool.Length);

            int maxSAllowed = sMaxClamp;
            int maxTAllowed = tMaxClamp;
            int totalMax = 1 + maxSAllowed + maxTAllowed;
            if (maxItemsPerOrder > 0 && totalMax > maxItemsPerOrder)
            {
                int reduce = totalMax - maxItemsPerOrder;
                while (reduce > 0 && (maxTAllowed > tMin || maxSAllowed > sMin))
                {
                    if (maxTAllowed > tMin) { maxTAllowed--; reduce--; }
                    else if (maxSAllowed > sMin) { maxSAllowed--; reduce--; }
                    else break;
                }
            }

            IngredientSO pickBase = basePool[UnityEngine.Random.Range(0, basePool.Length)];
            int sCount = UnityEngine.Random.Range(sMin, maxSAllowed + 1);
            int tCount = UnityEngine.Random.Range(tMin, maxTAllowed + 1);

            _current.baseIng = pickBase;
            _current.salsas = PickUnique(salsaPool, sCount);
            _current.toppings = PickUnique(toppingPool, tCount);

            if (debugLogs) Log("Nuevo pedido (solo desbloqueados) -> " + BuildOrderText(_current));
            var ch = OnOrderChanged; if (ch != null) ch(_current);
        }


        private bool ValidatePools()
        {
            if (baseOptions == null || baseOptions.Length == 0) { Log("Sin baseOptions."); return false; }
            if (salsaOptions == null || salsaOptions.Length == 0) { Log("Sin salsaOptions."); return false; }
            if (toppingOptions == null || toppingOptions.Length == 0) { Log("Sin toppingOptions."); return false; }
            return true;
        }

        private IngredientSO[] PickUnique(IngredientSO[] pool, int count)
        {
            if (pool == null) return new IngredientSO[0];
            count = Mathf.Clamp(count, 0, pool.Length);
            IngredientSO[] temp = new IngredientSO[pool.Length];
            for (int i = 0; i < pool.Length; i++) temp[i] = pool[i];

            for (int i = 0; i < count; i++)
            {
                int j = UnityEngine.Random.Range(i, temp.Length);
                var swap = temp[i]; temp[i] = temp[j]; temp[j] = swap;
            }

            IngredientSO[] res = new IngredientSO[count];
            for (int k = 0; k < count; k++) res[k] = temp[k];
            return res;
        }

        private void HandleDelivered(CraftingManager.DeliveryPayload p)
        {
            bool ok = MatchesOrder(p, _current, allowExtrasOnDelivery);
            if (ok)
            {
                if (debugLogs) Log("[OK] Pedido cumplido.");
                var fh = OnOrderFulfilled; if (fh != null) fh(_current);
                GenerateNextOrder();
            }
            else
            {
                if (debugLogs) Log("[FAIL] Pedido incorrecto.");
                var ffh = OnOrderFailed; if (ffh != null) ffh(_current);
                GenerateNextOrder();
            }
        }

        private bool MatchesOrder(CraftingManager.DeliveryPayload p, OrderSpec order, bool allowExtras)
        {
            if (p.ingredientIds == null || p.ingredientIds.Length == 0) return false;

            string baseId = (order.baseIng != null) ? order.baseIng.Id : null;
            if (string.IsNullOrEmpty(baseId)) return false;
            if (!ContainsOnce(p.ingredientIds, baseId)) return false;

            if (order.salsas == null || order.salsas.Length == 0) return false;
            for (int i = 0; i < order.salsas.Length; i++)
            {
                string sid = order.salsas[i] != null ? order.salsas[i].Id : null;
                if (string.IsNullOrEmpty(sid)) return false;
                if (!ContainsAtLeastOnce(p.ingredientIds, sid)) return false;
            }

            if (order.toppings == null || order.toppings.Length == 0) return false;
            for (int i = 0; i < order.toppings.Length; i++)
            {
                string tid = order.toppings[i] != null ? order.toppings[i].Id : null;
                if (string.IsNullOrEmpty(tid)) return false;
                if (!ContainsAtLeastOnce(p.ingredientIds, tid)) return false;
            }

            if (allowExtras) return true;

            int expected = 1 + order.salsas.Length + order.toppings.Length;
            if (p.ingredientIds.Length != expected) return false;

            return true;
        }

        private bool ContainsOnce(string[] arr, string id)
        {
            int c = 0;
            for (int i = 0; i < arr.Length; i++) if (arr[i] == id) c++;
            return c == 1;
        }
        private bool ContainsAtLeastOnce(string[] arr, string id)
        {
            for (int i = 0; i < arr.Length; i++) if (arr[i] == id) return true;
            return false;
        }

        private string BuildOrderText(OrderSpec o)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("Base=");
            sb.Append(o.baseIng != null ? o.baseIng.Id : "null");
            sb.Append(" | Salsas=");
            if (o.salsas != null) for (int i = 0; i < o.salsas.Length; i++) { sb.Append(i == 0 ? "" : ", "); sb.Append(o.salsas[i] != null ? o.salsas[i].Id : "null"); }
            sb.Append(" | Toppings=");
            if (o.toppings != null) for (int i = 0; i < o.toppings.Length; i++) { sb.Append(i == 0 ? "" : ", "); sb.Append(o.toppings[i] != null ? o.toppings[i].Id : "null"); }
            return sb.ToString();
        }
        private void Log(string m) { Debug.Log("[NPCOrder] " + m, this); }
    }
}