using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Interactions.Interactables
{
    public class InteractableLoot : Interactable {
        [SerializeField] private Loot.Loot loot;
        
        public override void Interact(GameObject interactor) {
            loot.LootItem();
        }
    }
}