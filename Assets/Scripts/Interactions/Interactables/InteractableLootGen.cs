using FourFatesStudios.ProjectWarden.Loot;
using FourFatesStudios.ProjectWarden.ScriptableObjects.LootTable;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Interactions.Interactables
{
    public class InteractableLootGen : Interactable
    {
        [SerializeField] private LootGenerator lootGenerator;
        
        public override void Interact(GameObject interactor) {
            lootGenerator.GenerateLoot();
        }
    }
}