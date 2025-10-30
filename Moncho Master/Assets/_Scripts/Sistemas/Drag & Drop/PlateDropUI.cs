using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlateDropUI : MonoBehaviour, IDropHandler
{
    [Header("Modelo")]
    [SerializeField] private PlateContainer plate;

    [Header("Visual")]
    [SerializeField] private RectTransform stackRoot;   // AS�GNALO EN INSPECTOR
    [SerializeField] private Image ingredientPrefab;         // ASSETS/PREFAB UI Image
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

        // Toma el IngredientSource del objeto arrastrado (o de alg�n padre)
        var src = eventData.pointerDrag.GetComponentInParent<IngredientSource>();
        if (src == null || src.Ingredient == null)
        {
            if (logDebug) Debug.LogWarning("[PlateDropUI] No encontr� IngredientSource/IngredientSO en el drag");
            return;
        }

        // Validaci�n del modelo
        if (plate == null)
        {
            if (logDebug) Debug.LogWarning("[PlateDropUI] plate NO asignado");
            return;
        }
        if (!plate.TryAdd(src.Ingredient))
        {
#if UNITY_EDITOR
            if (!plate.CanAcceptWithReason(src.Ingredient, out var why))
                Debug.LogWarning($"[PlateDropUI] TryAdd rechaz�: {why}");
            else
                Debug.LogWarning("[PlateDropUI] TryAdd rechaz� sin raz�n aparente (�cambios de estado de concurrencia?)");
#else
    Debug.LogWarning("[PlateDropUI] TryAdd rechaz� el ingrediente");
#endif
            return;
        }


        // Visual
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
        // img.SetNativeSize(); // opcional

        int index = stackRoot.childCount - 1; // reci�n creado queda al final
        var rt = img.rectTransform;
        rt.anchoredPosition = stackOffset * index;
        rt.localScale = Vector3.one;

        if (logDebug) Debug.Log($"[PlateDropUI] Instanci� capa {index} con {icon.name}");
    }

    // ��� Utilidad: reconstruir visual desde el modelo (si limpiaste o cambi� algo) ���
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
