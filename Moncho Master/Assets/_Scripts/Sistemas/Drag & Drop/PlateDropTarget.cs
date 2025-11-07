using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlateDropTarget : MonoBehaviour, IDropHandler
{
    [SerializeField] private PlateContainer plate;

    public void OnDrop(PointerEventData eventData)
    {
        if (plate == null || plate.IsLocked) return;

        if (eventData.pointerDrag == null || plate == null) return;

        DraggableIngredientUI drag = eventData.pointerDrag.GetComponent<DraggableIngredientUI>();
        if (drag == null) return;

        IngredientSO ing = drag.Ingredient;
        if (ing == null) return;

        bool ok = plate.TryAdd(ing);
    }
}
