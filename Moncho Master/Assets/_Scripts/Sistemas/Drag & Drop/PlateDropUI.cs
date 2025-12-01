using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlateDropUI : MonoBehaviour, IDropHandler
{
    [Header("Modelo")]
    [SerializeField] private PlateContainer plate;

    [Header("Visual")]
    [SerializeField] private RectTransform stackRoot;
    [SerializeField] private Image ingredientPrefab;
    [SerializeField] private Vector2 stackOffset = new Vector2(0f, 20f);

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private void OnEnable()
    {
        if (plate != null)
        {
            plate.OnContentChanged += RebuildFromModel;
            RebuildFromModel();

            if (logDebug)
                Debug.Log($"[PlateDropUI] Conectado al plato. Ingredientes actuales: {plate.Items?.Count ?? 0}");
        }
        else
        {
            Debug.LogError("[PlateDropUI] ¡Referencia al plato no asignada!");
        }
    }

    private void OnDisable()
    {
        if (plate != null)
        {
            plate.OnContentChanged -= RebuildFromModel;

            if (logDebug)
                Debug.Log("[PlateDropUI] Desconectado del plato");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            if (logDebug) Debug.Log("[PlateDropUI] Drop fallido: pointerDrag es null");
            return;
        }

        var src = eventData.pointerDrag.GetComponentInParent<IngredientSource>();
        if (src == null)
        {
            if (logDebug) Debug.Log("[PlateDropUI] Drop fallido: no se encontró IngredientSource");
            return;
        }

        if (src.Ingredient == null)
        {
            if (logDebug) Debug.Log("[PlateDropUI] Drop fallido: IngredientSource no tiene ingrediente asignado");
            return;
        }

        if (plate == null)
        {
            Debug.LogError("[PlateDropUI] Drop fallido: referencia al plato es null");
            return;
        }

        if (logDebug)
        {
            string ingredientInfo = $"Tipo: {src.Ingredient.GetType()}";

            string ingredientName = "Desconocido";
            var ingredientObj = src.Ingredient as UnityEngine.Object;
            if (ingredientObj != null)
            {
                ingredientName = ingredientObj.name;
            }

            Debug.Log($"[PlateDropUI] Intentando agregar ingrediente - Nombre: {ingredientName}, {ingredientInfo}");

            Debug.Log($"[PlateDropUI] Ingredientes actuales en el plato: {plate.Items.Count}");
            for (int i = 0; i < plate.Items.Count; i++)
            {
                var item = plate.Items[i];
                string itemName = "Desconocido";
                var itemObj = item as UnityEngine.Object;
                if (itemObj != null)
                {
                    itemName = itemObj.name;
                }
                Debug.Log($"  [{i}] {itemName}");
            }
        }

        string reason;
        bool success = plate.TryAdd(src.Ingredient, out reason);

        if (!success)
        {
            if (logDebug)
            {
                string ingredientName = "Desconocido";
                var ingredientObj = src.Ingredient as UnityEngine.Object;
                if (ingredientObj != null)
                {
                    ingredientName = ingredientObj.name;
                }

                Debug.LogWarning($"[PlateDropUI] TryAdd rechazó el ingrediente '{ingredientName}'");
                Debug.LogWarning($"[PlateDropUI] Razón: {reason}");

                Debug.Log($"[PlateDropUI] Tipo de ingrediente: {src.Ingredient.GetType()}");
                Debug.Log($"[PlateDropUI] ¿Ingrediente ya existe en el plato?: {plate.Items.Contains(src.Ingredient)}");
            }
        }
        else
        {
            if (logDebug)
            {
                string ingredientName = "Desconocido";
                var ingredientObj = src.Ingredient as UnityEngine.Object;
                if (ingredientObj != null)
                {
                    ingredientName = ingredientObj.name;
                }
                Debug.Log($"[PlateDropUI] Ingrediente '{ingredientName}' agregado exitosamente");
            }
        }
    }

    public void RebuildFromModel()
    {
        if (plate == null)
        {
            Debug.LogError("[PlateDropUI] RebuildFromModel fallido: plate es null");
            return;
        }

        if (stackRoot == null)
        {
            Debug.LogError("[PlateDropUI] RebuildFromModel fallido: stackRoot es null");
            return;
        }

        if (ingredientPrefab == null)
        {
            Debug.LogError("[PlateDropUI] RebuildFromModel fallido: ingredientPrefab es null");
            return;
        }

        int childCount = stackRoot.childCount;
        if (logDebug && childCount > 0)
            Debug.Log($"[PlateDropUI] Limpiando {childCount} elementos de la UI");

        for (int i = stackRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(stackRoot.GetChild(i).gameObject);
        }

        var items = plate.Items;
        if (items == null)
        {
            if (logDebug) Debug.Log("[PlateDropUI] plate.Items es null");
            return;
        }

        if (logDebug)
            Debug.Log($"[PlateDropUI] Reconstruyendo UI con {items.Count} ingredientes");

        for (int i = 0; i < items.Count; i++)
        {
            var ing = items[i];
            if (ing == null)
            {
                Debug.LogWarning($"[PlateDropUI] Ingrediente en índice {i} es null");
                continue;
            }

            Sprite icon = null;

            var prop = ing.GetType().GetProperty("Icon");
            if (prop != null && prop.PropertyType == typeof(Sprite))
            {
                icon = (Sprite)prop.GetValue(ing);
            }
            else
            {
                var field = ing.GetType().GetField("Icon");
                if (field != null && field.FieldType == typeof(Sprite))
                {
                    icon = (Sprite)field.GetValue(ing);
                }
            }

            if (icon == null)
            {
                string ingName = "Desconocido";
                var ingObj = ing as UnityEngine.Object;
                if (ingObj != null)
                {
                    ingName = ingObj.name;
                }
                Debug.LogWarning($"[PlateDropUI] El ingrediente '{ingName}' no tiene icono asignado");
                continue;
            }

            var img = Instantiate(ingredientPrefab, stackRoot);
            img.sprite = icon;
            var rt = img.rectTransform;
            rt.anchoredPosition = stackOffset * i;
            rt.localScale = Vector3.one;

            if (logDebug)
            {
                string ingName = "Desconocido";
                var ingObj = ing as UnityEngine.Object;
                if (ingObj != null)
                {
                    ingName = ingObj.name;
                }
                Debug.Log($"[PlateDropUI] Mostrando ingrediente {i}: {ingName}");
            }
        }
    }

    public void ClearPlate()
    {
        if (plate != null)
        {
            plate.Clear();
            if (logDebug) Debug.Log("[PlateDropUI] Plato limpiado");
        }
    }

    public void ListCurrentIngredients()
    {
        if (plate != null && plate.Items != null)
        {
            Debug.Log("[PlateDropUI] Listado de ingredientes actuales:");
            foreach (var ingredient in plate.Items)
            {
                string ingName = "Desconocido";
                var ingObj = ingredient as UnityEngine.Object;
                if (ingObj != null)
                {
                    ingName = ingObj.name;
                }
                Debug.Log($"- {ingName} (Tipo: {ingredient.GetType()})");
            }
        }
    }
}