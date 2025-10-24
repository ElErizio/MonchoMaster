using UnityEngine;

[CreateAssetMenu(menuName = "Moncho/Ingredient", fileName = "Ingredient")]
public class IngredientSO : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private IngredientCategory category;
    [SerializeField] private bool isUnlocked = true;
    [SerializeField] private Sprite card;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public Sprite Card => card;
    public IngredientCategory Category => category;
    public bool IsUnlocked => isUnlocked;
}
