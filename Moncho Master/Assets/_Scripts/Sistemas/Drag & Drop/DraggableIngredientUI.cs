using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableIngredientUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Datos")]
    [SerializeField] private IngredientSO ingredient;
    [SerializeField] private Image icon;

    [Header("Refs UI")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private RectTransform dragLayer;

    private RectTransform _rect;
    private CanvasGroup _cg;
    private Transform _startParent;
    private int _startSibling;
    private Vector3 _startPosition;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();

        if (icon != null && ingredient != null) icon.sprite = ingredient.Icon;
    }

    public IngredientSO Ingredient => ingredient;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startParent = _rect.parent;
        _startSibling = _rect.GetSiblingIndex();
        _startPosition = _rect.position;

        _cg.blocksRaycasts = false;
        _cg.alpha = 0.8f;

        icon.color = new Color(1, 1, 1, 1);

        if (dragLayer != null) _rect.SetParent(dragLayer, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (uiCanvas == null) return;

        Vector2 localPoint;
        RectTransform canvasRect = uiCanvas.transform as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, uiCanvas.worldCamera, out localPoint))
        {
            _rect.position = uiCanvas.transform.TransformPoint(localPoint);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _cg.blocksRaycasts = true;
        _cg.alpha = 1f;

        _rect.SetParent(_startParent, true);
        _rect.SetSiblingIndex(_startSibling);
        _rect.position = _startPosition;

        icon.color = new Color(1, 1, 1, 0);
    }
}
