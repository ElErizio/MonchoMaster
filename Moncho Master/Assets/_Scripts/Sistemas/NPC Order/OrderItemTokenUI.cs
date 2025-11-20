using UnityEngine;
using UnityEngine.UI;

public class OrderItemTokenUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text smallLabel;

    public void Bind(IngredientSO ing, string tag)
    {
        if (icon != null && ing != null)
        {
            icon.sprite = ing.Card;
            icon.preserveAspect = true;
            icon.enabled = (icon.sprite != null);
        }
        if (smallLabel != null) smallLabel.text = tag;
    }
}