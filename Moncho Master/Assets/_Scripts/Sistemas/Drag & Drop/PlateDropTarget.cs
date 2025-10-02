using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlateDropTarget : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private PlateContainer plate;
    [SerializeField] private Image highlight;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || plate == null) return;

        DraggableIngredientUI drag = eventData.pointerDrag.GetComponent<DraggableIngredientUI>();
        if (drag == null) return;

        IngredientSO ing = drag.Ingredient;
        if (ing == null) return;

        bool ok = plate.TryAdd(ing);
        if (!ok) FlashRed();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = false;
    }

    private void FlashRed()
    {
        if (highlight == null) return;
        Color original = highlight.color;
        highlight.color = Color.red;
        Invoke(nameof(Restore), 0.12f);
        void Restore() { highlight.color = original; }
    }
}
