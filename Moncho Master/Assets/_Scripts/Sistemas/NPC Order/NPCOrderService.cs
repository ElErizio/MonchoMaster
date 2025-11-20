using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Moncho.Orders
{
    public class NPCOrderService : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private CraftingManager crafting;
        [SerializeField] private UnlockService unlocks;
        [SerializeField] private GameManager gameManager;

        [Header("Pools de ingredientes permitidos")]
        [SerializeField] private IngredientSO[] baseOptions;
        [SerializeField] private IngredientSO[] salsaOptions;
        [SerializeField] private IngredientSO[] toppingOptions;

        [Header("Clientes - Sistema con Prefabs")]
        [SerializeField] private RectTransform clientContainer;
        [SerializeField] private GameObject[] clientPrefabs;
        [SerializeField] private Vector2 clientPosition = Vector2.zero;
        [SerializeField] private bool showClientOnStart = true;

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
        private int _currentClientIndex = -1;
        private GameObject _currentClientInstance;
        private bool _isInitialized = false;

        public delegate void OrderEvent(OrderSpec spec);
        public event OrderEvent OnOrderChanged;
        public event OrderEvent OnOrderFulfilled;
        public event OrderEvent OnOrderFailed;

        public OrderSpec CurrentOrder => _current;

        private void Awake()
        {
            // Limpiar cualquier cliente existente al inicio
            ClearCurrentClient();
        }

        private void Start()
        {
            InitializeService();
        }

        private void InitializeService()
        {
            if (_isInitialized) return;

            // Asegurar que tenemos las referencias necesarias
            if (gameManager == null)
                gameManager = GameManager.Instance;

            if (showClientOnStart)
            {
                GenerateNextOrder();
            }

            _isInitialized = true;
        }

        private void OnEnable()
        {
            if (crafting != null)
                crafting.OnDishDelivered += HandleDelivered;

            // Si ya está inicializado, forzar una actualización del cliente
            if (_isInitialized && _currentClientInstance == null)
            {
                GenerateNextOrder();
            }
        }

        private void OnDisable()
        {
            if (crafting != null)
                crafting.OnDishDelivered -= HandleDelivered;
        }

        private void ClearCurrentClient()
        {
            if (_currentClientInstance != null)
            {
                // Usar DestroyImmediate si estamos en modo edición o para limpieza inmediata
                if (Application.isPlaying)
                    Destroy(_currentClientInstance);
                else
                    DestroyImmediate(_currentClientInstance);

                _currentClientInstance = null;
            }

            // Limpiar también hijos del contenedor por si acaso
            if (clientContainer != null && Application.isPlaying)
            {
                foreach (Transform child in clientContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void GenerateNextOrder()
        {
            if (!ValidatePools()) return;

            IngredientSO[] basePool = unlocks?.FilterUnlocked(baseOptions) ?? baseOptions;
            IngredientSO[] salsaPool = unlocks?.FilterUnlocked(salsaOptions) ?? salsaOptions;
            IngredientSO[] toppingPool = unlocks?.FilterUnlocked(toppingOptions) ?? toppingOptions;

            if (!ValidatePool(basePool, "bases") ||
                !ValidatePool(salsaPool, "salsas") ||
                !ValidatePool(toppingPool, "toppings")) return;

            var (salsasCount, toppingsCount) = CalculateIngredientCounts(salsaPool.Length, toppingPool.Length);

            _current = new OrderSpec
            {
                baseIng = GetRandomIngredient(basePool),
                salsas = PickUnique(salsaPool, salsasCount),
                toppings = PickUnique(toppingPool, toppingsCount)
            };

            UpdateClient();

            if (debugLogs) Log("Nuevo pedido: " + BuildOrderText(_current));
            OnOrderChanged?.Invoke(_current);

            gameManager?.OnNewOrderGenerated(_current);
        }

        private (int salsas, int toppings) CalculateIngredientCounts(int availableSalsas, int availableToppings)
        {
            int sMin = Mathf.Max(1, minSalsas);
            int tMin = Mathf.Max(1, minToppings);

            int sMax = Mathf.Clamp(maxSalsas, sMin, availableSalsas);
            int tMax = Mathf.Clamp(maxToppings, tMin, availableToppings);

            if (maxItemsPerOrder > 0)
            {
                int totalItems = 1 + sMax + tMax;
                if (totalItems > maxItemsPerOrder)
                {
                    int excess = totalItems - maxItemsPerOrder;
                    while (excess > 0 && (sMax > sMin || tMax > tMin))
                    {
                        if (tMax > tMin) tMax--;
                        else if (sMax > sMin) sMax--;
                        excess--;
                    }
                }
            }

            return (
                UnityEngine.Random.Range(sMin, sMax + 1),
                UnityEngine.Random.Range(tMin, tMax + 1)
            );
        }

        private void UpdateClient()
        {
            ClearCurrentClient();

            if (clientPrefabs == null || clientPrefabs.Length == 0)
            {
                Log("No hay prefabs de clientes asignados");
                return;
            }

            if (clientContainer == null)
            {
                Log("No hay clientContainer asignado");
                return;
            }

            int newIndex;
            do
            {
                newIndex = UnityEngine.Random.Range(0, clientPrefabs.Length);
            } while (newIndex == _currentClientIndex && clientPrefabs.Length > 1);

            _currentClientIndex = newIndex;

            GameObject selectedPrefab = clientPrefabs[newIndex];
            if (selectedPrefab != null)
            {
                _currentClientInstance = Instantiate(selectedPrefab, clientContainer);

                // Configurar el RectTransform correctamente
                RectTransform rectTransform = _currentClientInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = clientPosition;
                    rectTransform.localScale = Vector3.one;
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);

                    // Forzar actualización del layout
                    LayoutRebuilder.ForceRebuildLayoutImmediate(clientContainer);
                    Canvas.ForceUpdateCanvases();
                }

                Log($"Cliente cambiado al prefab índice: {newIndex}");

                // Forzar una actualización del frame
                StartCoroutine(ForceUIUpdateNextFrame());
            }
            else
            {
                Log($"Prefab de cliente en índice {newIndex} es null");
            }
        }

        private System.Collections.IEnumerator ForceUIUpdateNextFrame()
        {
            yield return null; // Esperar un frame
            LayoutRebuilder.ForceRebuildLayoutImmediate(clientContainer);
            Canvas.ForceUpdateCanvases();
        }

        // ... (el resto de los métodos se mantienen igual)
        private bool ValidatePools()
        {
            if (baseOptions == null || baseOptions.Length == 0)
            {
                Log("Sin baseOptions.");
                return false;
            }
            if (salsaOptions == null || salsaOptions.Length == 0)
            {
                Log("Sin salsaOptions.");
                return false;
            }
            if (toppingOptions == null || toppingOptions.Length == 0)
            {
                Log("Sin toppingOptions.");
                return false;
            }
            return true;
        }

        private bool ValidatePool(IngredientSO[] pool, string poolName)
        {
            if (pool == null || pool.Length == 0)
            {
                Log($"No hay {poolName} desbloqueadas.");
                return false;
            }
            return true;
        }

        private IngredientSO GetRandomIngredient(IngredientSO[] pool) =>
            pool[UnityEngine.Random.Range(0, pool.Length)];

        private IngredientSO[] PickUnique(IngredientSO[] pool, int count)
        {
            if (pool == null || count <= 0)
                return Array.Empty<IngredientSO>();

            count = Mathf.Min(count, pool.Length);
            var list = new List<IngredientSO>(pool);
            var result = new List<IngredientSO>();

            for (int i = 0; i < count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, list.Count);
                result.Add(list[randomIndex]);
                list.RemoveAt(randomIndex);
            }

            return result.ToArray();
        }

        private void HandleDelivered(CraftingManager.DeliveryPayload p)
        {
            bool isOrderCorrect = MatchesOrder(p, _current, allowExtrasOnDelivery);

            if (isOrderCorrect)
            {
                if (debugLogs) Log("[OK] Pedido cumplido.");
                OnOrderFulfilled?.Invoke(_current);
                gameManager?.OnOrderCompleted(_current, true);
            }
            else
            {
                if (debugLogs) Log("[FAIL] Pedido incorrecto.");
                OnOrderFailed?.Invoke(_current);
                gameManager?.OnOrderCompleted(_current, false);
            }

            GenerateNextOrder();
        }

        private bool MatchesOrder(CraftingManager.DeliveryPayload p, OrderSpec order, bool allowExtras)
        {
            if (p.ingredientIds == null || p.ingredientIds.Length == 0)
                return false;

            string baseId = order.baseIng?.Id;
            if (string.IsNullOrEmpty(baseId) || !ContainsExact(p.ingredientIds, baseId, 1))
                return false;

            if (!VerifyIngredients(order.salsas, p.ingredientIds))
                return false;

            if (!VerifyIngredients(order.toppings, p.ingredientIds))
                return false;

            return true;
        }

        private bool VerifyIngredients(IngredientSO[] ingredients, string[] deliveredIds)
        {
            if (ingredients == null) return false;

            foreach (var ingredient in ingredients)
            {
                string id = ingredient?.Id;
                if (string.IsNullOrEmpty(id) || !ContainsAtLeast(deliveredIds, id, 1))
                    return false;
            }
            return true;
        }

        private bool ContainsExact(string[] arr, string id, int expectedCount)
        {
            int count = 0;
            foreach (var item in arr)
                if (item == id) count++;

            return count == expectedCount;
        }

        private bool ContainsAtLeast(string[] arr, string id, int minCount)
        {
            int count = 0;
            foreach (var item in arr)
                if (item == id) count++;

            return count >= minCount;
        }

        private string BuildOrderText(OrderSpec o)
        {
            return $"Base={o.baseIng?.Id ?? "null"} | " +
                   $"Salsas={ArrayToString(o.salsas)} | " +
                   $"Toppings={ArrayToString(o.toppings)}";
        }

        private string ArrayToString(IngredientSO[] array)
        {
            if (array == null) return "null";
            return string.Join(", ", System.Array.ConvertAll(array, i => i?.Id ?? "null"));
        }

        private void Log(string m) => Debug.Log($"[NPCOrder] {m}", this);
    }
}