using UnityEngine;

public class ConfirmDeliverButton : MonoBehaviour
{
    [SerializeField] private CraftingManager crafting;

    public void OnClickConfirm()
    {
        if (crafting != null) crafting.ConfirmAndDeliver();
    }
}
