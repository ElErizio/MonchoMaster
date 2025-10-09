using UnityEngine;

public class ConfirmDeliverButton : MonoBehaviour
{
    [SerializeField] private CraftingManager crafting;

    public void OnClickConfirm()
    {
        if (crafting == null)
        {
            Debug.LogWarning("[ConfirmDeliverButton] CraftingManager no asignado.", this);
            return;
        }

        crafting.ConfirmAndDeliver();
    }
}
