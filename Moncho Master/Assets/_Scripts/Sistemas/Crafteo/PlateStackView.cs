using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlateStackView : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] private PlateContainer plate;

    [Header("Visual")]
    [SerializeField] private Transform stackRoot;
    [SerializeField] private GameObject ingredientPrefab;
    [SerializeField] private Vector3 stackOffset = new Vector3(0f, 0.12f, 0f);
    [SerializeField] private int baseSortingOrder = 10;

    private void OnEnable()
    {
        if (plate != null) plate.OnContentChanged += Refresh;
        Refresh(); // forzar sync al habilitar
    }

    private void OnDisable()
    {
        if (plate != null) plate.OnContentChanged -= Refresh;
    }

    public void Refresh()
    {
        if (plate == null || stackRoot == null || ingredientPrefab == null) return;

        int target = plate.Items.Count;
        int current = stackRoot.childCount;

        // Quitar extras
        for (int i = current - 1; i >= target; i--)
            Destroy(stackRoot.GetChild(i).gameObject);

        // Añadir faltantes
        for (int i = current; i < target; i++)
            CreateVisualAt(i);

        // Asegurar sprites/posiciones correctas (por si cambió orden)
        for (int i = 0; i < stackRoot.childCount; i++)
            ApplyVisual(i, stackRoot.GetChild(i).gameObject);
    }

    private void CreateVisualAt(int index)
    {
        var go = Instantiate(ingredientPrefab, stackRoot);
        go.name = $"Layer_{index}";
        ApplyVisual(index, go);

        go.transform.localScale = Vector3.zero;
        LeanScale(go, Vector3.one, 0.12f);
    }

    private void ApplyVisual(int index, GameObject go)
    {
        var ing = plate.Items[index];
        go.transform.localPosition = stackOffset * (index + 1);

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = ing.Icon;
            sr.sortingOrder = baseSortingOrder + index;
            return;
        }

        var img = go.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = ing.Icon;
        }
    }

    private void LeanScale(GameObject go, Vector3 target, float time)
    {
        StartCoroutine(ScaleRoutine(go.transform, target, time));
    }
    private System.Collections.IEnumerator ScaleRoutine(Transform t, Vector3 target, float time)
    {
        Vector3 start = t.localScale;
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / time);
            float e = 1f - Mathf.Pow(1f - k, 2f);
            t.localScale = Vector3.LerpUnclamped(start, target, e);
            yield return null;
        }
        t.localScale = target;
    }
}
