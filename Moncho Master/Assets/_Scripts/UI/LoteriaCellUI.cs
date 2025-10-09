using UnityEngine;
using UnityEngine.UI;

public class LoteriaCellUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image markOverlay;

    public void Bind(Sprite sprite, bool marked)
    {
        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = (sprite != null);
            icon.preserveAspect = true;
        }
        if (markOverlay != null)
        {
            markOverlay.gameObject.SetActive(marked);
        }
    }
}
