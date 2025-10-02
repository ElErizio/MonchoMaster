using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moncho/Recipe", fileName = "Recipe")]
public class RecipeSO : ScriptableObject
{
    [Serializable]
    public struct DishData
    {
        public string id;
        public string displayName;
        public Sprite icon;
        public int price;
    }

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
    }

    [Header("Meta")]
    public string id;
    public bool isUnlocked = true;

    [Header("Resultado (antes estaba en DishSO)")]
    public DishData output;

    [Header("Match exacto (opcional)")]
    public bool useExactIngredients;
    public bool orderMatters;
    public List<IngredientRequirement> exactIngredients;

    [Header("Match por categorías (opcional)")]
    public List<CategoryRequirement> categoryRequirements;
    public int maxExtras = 0;
}
