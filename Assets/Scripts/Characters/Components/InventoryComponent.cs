using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    public class InventoryComponent : MonoBehaviour
    {
        private List<Trinket> trinkets;
        
        public IReadOnlyList<Trinket> Trinkets => trinkets.AsReadOnly();

        public void Awake() {
            trinkets = new List<Trinket>();
        }
    }
}