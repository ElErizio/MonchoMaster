using UnityEngine;

public class IngredientSource : MonoBehaviour
{
    [SerializeField] private IngredientSO ingredient;
    public IngredientSO Ingredient => ingredient;
}
