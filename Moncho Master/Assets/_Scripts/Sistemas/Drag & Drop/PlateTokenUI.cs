using UnityEngine;
using UnityEngine.UI;

public class PlateItemTokenUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Button removeButton;

    private PlateContainer _plate;
    private IngredientSO _ingredient;

    public void Bind(PlateContainer plate, IngredientSO ing)
    {
        _plate = plate;
        _ingredient = ing;

        if (icon != null && ing != null) icon.sprite = ing.Icon;
        if (removeButton != null) removeButton.onClick.AddListener(RemoveOne);
    }

    private void OnDestroy()
    {
        if (removeButton != null) removeButton.onClick.RemoveListener(RemoveOne);
    }

    private void RemoveOne()
    {
        if (_plate != null && _ingredient != null)
        {
            _plate.Remove(_ingredient);
        }
    }
}
