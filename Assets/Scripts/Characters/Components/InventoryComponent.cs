using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    public class InventoryComponent : MonoBehaviour
    {
        private List<Trinket> trinkets;
        private List<Potion> potions;
        private List<Ingredient> ingredients;
        
        public IReadOnlyList<Trinket> Trinkets => trinkets.AsReadOnly();
        public IReadOnlyList<Potion> Potions => potions.AsReadOnly();
        public IReadOnlyList<Ingredient> Ingredients => ingredients.AsReadOnly();

        public void Awake() {
            trinkets = new List<Trinket>();
            potions = new List<Potion>();
            ingredients = new List<Ingredient>();
        }
    }
}