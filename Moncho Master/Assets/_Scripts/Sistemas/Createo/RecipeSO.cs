using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moncho/Recipe", fileName = "Recipe")]
public class RecipeSO : ScriptableObject
{
    [Serializable]
    public struct IngredientRequirement
    {
        public IngredientSO ingredient;
        public int quantity;
    }

    [Serializable]
    public struct CategoryRequirement
    {
        public IngredientCategory category;
        public int min;                 
        public int max;                 
        public IngredientSO[] blacklist;
    }

    [Header("Definición")]
    public string id;
    public DishSO output;

    [Header("Match exacto (opcional)")]
    public bool useExactIngredients;
    public List<IngredientRequirement> exactIngredients;

    [Header("Match por categorías (opcional)")]
    public List<CategoryRequirement> categoryRequirements;

    [Header("Meta")]
    public bool isUnlocked = true;
}
