using UnityEngine;
using UnityEngine.UI;

public class IngredientButtonLocker : MonoBehaviour
{
    [SerializeField] private UnlockService unlocks;
    [SerializeField] private IngredientSO ingredient;
    [SerializeField] private Selectable uiSelectable;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private bool hideWhenLocked = false;
    [SerializeField] private float lockedAlpha = 0.4f;

    private bool _deferredQueued;

    private void Awake()
    {
        if (uiSelectable == null) uiSelectable = GetComponent<Selectable>();
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (unlocks != null) unlocks.OnUnlocksChanged += Refresh;
        Refresh();
    }

    private void Start()
    {
        Refresh();
        QueueDeferred();
    }

    private void OnDisable()
    {
        if (unlocks != null) unlocks.OnUnlocksChanged -= Refresh;
        _deferredQueued = false;
        StopAllCoroutines();
    }

    private void QueueDeferred()
    {
        if (_deferredQueued) return;
        _deferredQueued = true;
        StartCoroutine(CoDeferred());
    }

    private System.Collections.IEnumerator CoDeferred()
    {
        yield return null;
        _deferredQueued = false;
        Refresh();
    }

    public void Refresh()
    {
        bool can = true;

        if (unlocks != null && ingredient != null)
        {
            string id = ingredient.Id;
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[Locker] '{ingredient.name}' no tiene Id. Lo trato como DESBLOQUEADO.");
                can = true;
            }
            else
            {
                can = unlocks.IsUnlocked(ingredient);
            }
        }

        if (uiSelectable != null) uiSelectable.interactable = can;

        if (group != null)
        {
            group.alpha = can ? 1f : lockedAlpha;
            group.blocksRaycasts = can;
            group.interactable = can;
        }

        if (hideWhenLocked)
            gameObject.SetActive(can);
        else
            gameObject.SetActive(true);
    }
}
