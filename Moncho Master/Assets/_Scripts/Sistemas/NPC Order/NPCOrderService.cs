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
        [SerializeField] private GameManager gameManager;

        [Header("Preparaciones específicas")]
        [SerializeField] private OrderRecipe[] specificRecipes;

        [Header("Clientes - Sistema con Prefabs")]
        [SerializeField] private RectTransform clientContainer;
        [SerializeField] private GameObject[] clientPrefabs;
        private bool showClientOnStart = true;
        private Vector2 clientPosition = Vector2.zero;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;

        [Serializable]
        public struct OrderSpec
        {
            public IngredientSO baseIng;
            public IngredientSO[] salsas;
            public IngredientSO[] toppings;
        }

        [Serializable]
        public class OrderRecipe
        {
            public string recipeName;
            public IngredientSO[] requiredIngredients;
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
            ClearCurrentClient();
        }

        private void Start()
        {
            InitializeService();
        }

        private void InitializeService()
        {
            if (_isInitialized) return;

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
                if (Application.isPlaying)
                    Destroy(_currentClientInstance);
                else
                    DestroyImmediate(_currentClientInstance);

                _currentClientInstance = null;
            }

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
            if (!ValidateRecipes()) return;
            AudioManager.Instance.NuevoCliente();

            List<OrderRecipe> availableRecipes = GetRecipesWithUnlockedIngredients();

            if (availableRecipes.Count == 0)
            {
                Log("No hay recetas disponibles (todos los ingredientes requeridos están bloqueados).");
                return;
            }

            OrderRecipe selectedRecipe = GetRandomRecipeFromList(availableRecipes);

            if (selectedRecipe == null || selectedRecipe.requiredIngredients == null || selectedRecipe.requiredIngredients.Length == 0)
            {
                Log("Receta seleccionada no válida");
                return;
            }

            _current = ConvertRecipeToOrderSpec(selectedRecipe);

            UpdateClient();

            if (debugLogs) Log($"Nuevo pedido: {selectedRecipe.recipeName} - {BuildOrderText(_current)}");
            OnOrderChanged?.Invoke(_current);

            gameManager?.OnNewOrderGenerated(_current);
        }

        private List<OrderRecipe> GetRecipesWithUnlockedIngredients()
        {
            List<OrderRecipe> availableRecipes = new List<OrderRecipe>();

            if (UnlockService.Instance == null)
            {
                Log("UnlockService no asignado");
                return availableRecipes;
            }

            foreach (var recipe in specificRecipes)
            {
                if (recipe == null || recipe.requiredIngredients == null) continue;

                bool allIngredientsUnlocked = true;

                foreach (var ingredient in recipe.requiredIngredients)
                {
                    if (ingredient == null)
                    {
                        allIngredientsUnlocked = false;
                        break;
                    }

                    if (!UnlockService.Instance.IsUnlocked(ingredient))
                    {
                        allIngredientsUnlocked = false;
                        break;
                    }
                }

                if (allIngredientsUnlocked)
                {
                    availableRecipes.Add(recipe);

                    if (debugLogs)
                    {
                        Log($"Receta disponible: {recipe.recipeName}");
                        string ingredientList = "";
                        foreach (var ing in recipe.requiredIngredients)
                            ingredientList += $"{ing.name} (ID: {ing.Id}), ";
                        Log($"Ingredientes: {ingredientList}");
                    }
                }
            }

            if (debugLogs) Log($"Total recetas disponibles: {availableRecipes.Count}/{specificRecipes.Length}");

            return availableRecipes;
        }

        private OrderSpec ConvertRecipeToOrderSpec(OrderRecipe recipe)
        {
            var orderSpec = new OrderSpec();
            var ingredients = new List<IngredientSO>(recipe.requiredIngredients);

            if (ingredients.Count > 0)
            {
                orderSpec.baseIng = ingredients[0];
                ingredients.RemoveAt(0);
            }

            int salsaCount = Mathf.CeilToInt(ingredients.Count / 2f);
            orderSpec.salsas = ingredients.GetRange(0, salsaCount).ToArray();
            orderSpec.toppings = ingredients.GetRange(salsaCount, ingredients.Count - salsaCount).ToArray();

            return orderSpec;
        }

        private bool ValidateRecipes()
        {
            if (specificRecipes == null || specificRecipes.Length == 0)
            {
                Log("No hay preparaciones específicas asignadas.");
                return false;
            }

            foreach (var recipe in specificRecipes)
            {
                if (recipe.requiredIngredients == null || recipe.requiredIngredients.Length == 0)
                {
                    Log($"La receta '{recipe.recipeName}' no tiene ingredientes asignados.");
                    return false;
                }

                foreach (var ingredient in recipe.requiredIngredients)
                {
                    if (ingredient == null)
                    {
                        Log($"La receta '{recipe.recipeName}' tiene un ingrediente nulo.");
                        return false;
                    }
                }
            }

            return true;
        }

        public bool ValidateCurrentOrder(CraftingManager.DeliveryPayload payload)
        {
            return MatchesOrder(payload, _current);
        }

        private OrderRecipe GetRandomRecipeFromList(List<OrderRecipe> recipeList)
        {
            if (recipeList.Count == 0) return null;
            return recipeList[UnityEngine.Random.Range(0, recipeList.Count)];
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

                RectTransform rectTransform = _currentClientInstance.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = clientPosition;
                    rectTransform.localScale = Vector3.one;
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);

                    LayoutRebuilder.ForceRebuildLayoutImmediate(clientContainer);
                    Canvas.ForceUpdateCanvases();
                }

                Log($"Cliente cambiado al prefab índice: {newIndex}");

                StartCoroutine(ForceUIUpdateNextFrame());
            }
            else
            {
                Log($"Prefab de cliente en índice {newIndex} es null");
            }
        }

        private System.Collections.IEnumerator ForceUIUpdateNextFrame()
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(clientContainer);
            Canvas.ForceUpdateCanvases();
        }

        private void HandleDelivered(CraftingManager.DeliveryPayload p)
        {
            bool isOrderCorrect = p.matched;

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

        private bool MatchesOrder(CraftingManager.DeliveryPayload p, OrderSpec order)
        {
            if (p.ingredientIds == null || p.ingredientIds.Length == 0)
                return false;

            var requiredIngredients = GetAllRequiredIngredients(order);
            var deliveredIds = new List<string>(p.ingredientIds);

            foreach (var required in requiredIngredients)
            {
                if (string.IsNullOrEmpty(required.Id))
                {
                    Log("Ingrediente requerido sin ID válido");
                    return false;
                }

                if (!deliveredIds.Contains(required.Id))
                {
                    return false;
                }

                deliveredIds.Remove(required.Id);
            }

            return true;
        }

        private List<IngredientSO> GetAllRequiredIngredients(OrderSpec order)
        {
            var ingredients = new List<IngredientSO>();

            if (order.baseIng != null)
                ingredients.Add(order.baseIng);

            if (order.salsas != null)
                ingredients.AddRange(order.salsas);

            if (order.toppings != null)
                ingredients.AddRange(order.toppings);

            return ingredients;
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