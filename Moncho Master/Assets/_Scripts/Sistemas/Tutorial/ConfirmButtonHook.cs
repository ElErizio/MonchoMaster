using UnityEngine;

public class ConfirmButtonHook : MonoBehaviour
{
    [SerializeField] private CraftingManager crafting;
    public void OnConfirmClicked()
    {
        if (crafting != null) crafting.ConfirmAndDeliver();
        else Debug.LogWarning("[ConfirmButtonHook] Falta asignar CraftingManager.");
    }
}
