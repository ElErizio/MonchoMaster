using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoteriaHUD : MonoBehaviour
{
    [SerializeField] private LoteriaService loteria;
    [SerializeField] private UnlockService unlocks;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform gridRoot;

    private readonly List<GameObject> _spawned = new List<GameObject>();
    private bool _deferredQueued;

    private void OnEnable()
    {
        if (loteria != null) loteria.OnBoardChanged += Rebuild;
        Rebuild();
        QueueDeferredRebuild();
    }

    private void OnDisable()
    {
        if (loteria != null) loteria.OnBoardChanged -= Rebuild;
        _deferredQueued = false;
        StopAllCoroutines();
    }

    private void QueueDeferredRebuild()
    {
        if (_deferredQueued) return;
        _deferredQueued = true;
        StartCoroutine(CoDeferred());
    }

    private IEnumerator CoDeferred()
    {
        yield return null;
        _deferredQueued = false;
        Rebuild();
    }

    private void Clear()
    {
        for (int i = 0; i < _spawned.Count; i++) if (_spawned[i] != null) Destroy(_spawned[i]);
        _spawned.Clear();
    }

    [ContextMenu("Hola Papu")]
    public void Debuggg()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        Clear();

        if (loteria == null)
        {
            Debug.LogWarning("[LoteriaHUD] Falta referencia a LoteriaService.", this);
            return;
        }
        if (cellPrefab == null)
        {
            Debug.LogWarning("[LoteriaHUD] Falta cellPrefab.", this);
            return;
        }

        var cells = loteria.Snapshot;
        Transform parent = gridRoot != null ? gridRoot : transform;

        int created = 0;
        for (int i = 0; i < cells.Length; i++)
        {
            var go = Instantiate(cellPrefab, parent);
            _spawned.Add(go);
            created++;

            var ui = go.GetComponent<LoteriaCellUI>();
            if (ui == null)
            {
                Debug.LogWarning("[LoteriaHUD] cellPrefab no tiene LoteriaCellUI.", go);
                continue;
            }

            Sprite sp = null;
            if (unlocks != null && !string.IsNullOrEmpty(cells[i].ingredientId))
            {
                var ing = unlocks.FindById(cells[i].ingredientId);
                if (ing != null)
                    sp = ing.Card;
            }

            ui.Bind(sp, cells[i].marked);
        }
        // Debug para mencionar cuantas Celdas se crearon para la lotería
        // Debug.Log("[LoteriaHUD] Celdas instanciadas: " + created, this);
    }

}
