using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Moncho/Plate Definition", fileName = "PlateDefinition")]
public class PlateDefinitionSO : ScriptableObject
{
    [Serializable]
    public struct CategoryLimit
    {
        public IngredientCategory category;
        public int min;
        public int max;
    }

    public string id;
    public int capacity = 6;
    public bool allowDuplicates = true;

    public List<CategoryLimit> categoryLimits;
}
