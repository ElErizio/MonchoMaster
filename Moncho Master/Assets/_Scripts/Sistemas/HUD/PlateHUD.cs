using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlateHUD : MonoBehaviour
{
    [SerializeField] private PlateContainer plate;
    [SerializeField] private GameObject tokenPrefab;
    [SerializeField] private Transform contentRoot;

    private readonly List<GameObject> _spawned = new List<GameObject>();

    private void OnEnable()
    {
        if (plate != null) plate.OnContentChanged += Rebuild;
        Rebuild();
    }

    private void OnDisable()
    {
        if (plate != null) plate.OnContentChanged -= Rebuild;
    }

    public void Rebuild()
    {
        for (int i = 0; i < _spawned.Count; i++) Destroy(_spawned[i]);
        _spawned.Clear();

        if (plate == null) return;
        var items = plate.Items;
        if (items == null || items.Count == 0) return;

        for (int i = 0; i < items.Count; i++)
        {
            IngredientSO ing = items[i];
            if (ing == null) continue;

            Transform parent = contentRoot != null ? contentRoot : transform;
            GameObject go = Instantiate(tokenPrefab, parent);
            _spawned.Add(go);

            PlateItemTokenUI token = go.GetComponent<PlateItemTokenUI>();
            if (token != null) token.Bind(plate, ing);
        }
    }
}
