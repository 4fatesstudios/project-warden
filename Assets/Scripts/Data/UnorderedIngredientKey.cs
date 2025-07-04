using System;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

[Serializable]
public struct UnorderedIngredientKey : IEquatable<UnorderedIngredientKey>
{
    private readonly Ingredient[] sortedIngredients;

    public UnorderedIngredientKey(params Ingredient[] ingredients)
    {
        if (ingredients.Length < 2 || ingredients.Length > 3)
            throw new ArgumentException("Key must have 2 or 3 ingredients.");

        sortedIngredients = ingredients
            .OrderBy(i => i != null ? i.name : "") // Avoid null reference
            .ToArray();
    }

    public bool Equals(UnorderedIngredientKey other)
    {
        return sortedIngredients.SequenceEqual(other.sortedIngredients);
    }

    public override bool Equals(object obj)
    {
        return obj is UnorderedIngredientKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (var ingredient in sortedIngredients)
            {
                hash = hash * 31 + (ingredient != null ? ingredient.name.GetHashCode() : 0);
            }
            return hash;
        }
    }

    public override string ToString()
    {
        return string.Join(" + ", sortedIngredients.Select(i => i ? i.name : "null"));
    }
}
