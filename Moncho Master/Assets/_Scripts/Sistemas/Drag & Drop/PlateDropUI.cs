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
        if (plate != null) plate.OnContentChanged += RebuildFromModel;
        RebuildFromModel();
    }

    private void OnDisable()
    {
        if (plate != null) plate.OnContentChanged -= RebuildFromModel;
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        var src = eventData.pointerDrag.GetComponentInParent<IngredientSource>();
        if (src == null || src.Ingredient == null) return;
        if (plate == null) return;

        string reason;
        if (!plate.TryAdd(src.Ingredient, out reason))
        {
            if (logDebug) Debug.LogWarning($"[PlateDropUI] TryAdd rechazó: {reason}");
            return;
        }
    }

    public void RebuildFromModel()
    {
        if (plate == null || stackRoot == null || ingredientPrefab == null) return;

        for (int i = stackRoot.childCount - 1; i >= 0; i--)
            Destroy(stackRoot.GetChild(i).gameObject);

        var items = plate.Items;
        if (items == null) return;

        for (int i = 0; i < items.Count; i++)
        {
            var ing = items[i];
            if (ing == null || ing.Icon == null) continue;

            var img = Instantiate(ingredientPrefab, stackRoot);
            img.sprite = ing.Icon;
            var rt = img.rectTransform;
            rt.anchoredPosition = stackOffset * i;
            rt.localScale = Vector3.one;
        }
    }
}
