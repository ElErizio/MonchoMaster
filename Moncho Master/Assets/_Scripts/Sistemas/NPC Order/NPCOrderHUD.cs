using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Moncho.Orders;

public class NPCOrderHUD : MonoBehaviour
{
    [SerializeField] private NPCOrderService orderSvc;
    [SerializeField] private GameObject tokenPrefab;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private Text titleLabel;

    private readonly List<GameObject> _spawned = new List<GameObject>();

    private void OnEnable()
    {
        if (orderSvc != null) orderSvc.OnOrderChanged += Rebuild;
        if (orderSvc != null) Rebuild(orderSvc.CurrentOrder);
    }
    private void OnDisable()
    {
        if (orderSvc != null) orderSvc.OnOrderChanged -= Rebuild;
    }

    private void Clear()
    {
        for (int i = 0; i < _spawned.Count; i++) Destroy(_spawned[i]);
        _spawned.Clear();
    }

    private void Rebuild(NPCOrderService.OrderSpec spec)
    {
        Clear();
        if (titleLabel != null) titleLabel.text = "Pedido";
        if (tokenPrefab == null) return;

        Transform parent = contentRoot != null ? contentRoot : transform;

        if (spec.baseIng != null)
        {
            var go = Instantiate(tokenPrefab, parent);
            _spawned.Add(go);
            var tok = go.GetComponent<OrderItemTokenUI>();
            if (tok != null) tok.Bind(spec.baseIng, "BASE");
        }
        if (spec.salsas != null)
        {
            for (int i = 0; i < spec.salsas.Length; i++)
            {
                if (spec.salsas[i] == null) continue;
                var go = Instantiate(tokenPrefab, parent);
                _spawned.Add(go);
                var tok = go.GetComponent<OrderItemTokenUI>();
                if (tok != null) tok.Bind(spec.salsas[i], "SALSA");
            }
        }
        if (spec.toppings != null)
        {
            for (int i = 0; i < spec.toppings.Length; i++)
            {
                if (spec.toppings[i] == null) continue;
                var go = Instantiate(tokenPrefab, parent);
                _spawned.Add(go);
                var tok = go.GetComponent<OrderItemTokenUI>();
                if (tok != null) tok.Bind(spec.toppings[i], "TOP");
            }
        }
    }
}
