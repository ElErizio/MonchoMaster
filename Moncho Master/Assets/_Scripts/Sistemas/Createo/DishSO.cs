using UnityEngine;

[CreateAssetMenu(menuName = "Moncho/Dish", fileName = "Dish")]
public class DishSO : ScriptableObject
{
    [SerializeField] private string id;          
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
}
