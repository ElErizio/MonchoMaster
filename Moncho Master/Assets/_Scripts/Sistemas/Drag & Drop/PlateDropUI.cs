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

    public void OnDrop(PointerEventData eventData)
    {
        if (logDebug) Debug.Log("[PlateDropUI] OnDrop");

        if (eventData.pointerDrag == null)
        {
            if (logDebug) Debug.LogWarning("[PlateDropUI] pointerDrag es null");
            return;
        }

        var src = eventData.pointerDrag.GetComponentInParent<IngredientSource>();
        if (src == null || src.Ingredient == null)
        {
            if (logDebug) Debug.LogWarning("[PlateDropUI] No encontré IngredientSource/IngredientSO en el drag");
            return;
        }

        if (plate == null)
        {
            if (logDebug) Debug.LogWarning("[PlateDropUI] plate NO asignado");
            return;
        }

        if (ingredientPrefab == null || stackRoot == null)
        {
            if (logDebug) Debug.LogWarning("[PlateDropUI] imagePrefab o stackRoot NO asignados");
            return;
        }

        var icon = src.Ingredient.Icon;
        if (icon == null)
        {
            if (logDebug) Debug.LogWarning("[PlateDropUI] El IngredientSO no tiene Icon");
            return;
        }

        var img = Instantiate(ingredientPrefab, stackRoot);
        img.sprite = icon;

        int index = stackRoot.childCount - 1;
        var rt = img.rectTransform;
        rt.anchoredPosition = stackOffset * index;
        rt.localScale = Vector3.one;

        if (logDebug) Debug.Log($"[PlateDropUI] Instancié capa {index} con {icon.name}");
    }

    [ContextMenu("Rebuild From Model")]
    public void RebuildFromModel()
    {
        if (plate == null || stackRoot == null || ingredientPrefab == null) return;

        for (int i = stackRoot.childCount - 1; i >= 0; i--)
            DestroyImmediate(stackRoot.GetChild(i).gameObject);

        for (int i = 0; i < plate.Items.Count; i++)
        {
            var ing = plate.Items[i];
            if (ing == null || ing.Icon == null) continue;

            var img = Instantiate(ingredientPrefab, stackRoot);
            img.sprite = ing.Icon;
            var rt = img.rectTransform;
            rt.anchoredPosition = stackOffset * i;
            rt.localScale = Vector3.one;
        }

        if (logDebug) Debug.Log("[PlateDropUI] RebuildFromModel completado");
    }

}
