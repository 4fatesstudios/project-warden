using FourFatesStudios.ProjectWarden.Characters.Controllers;
using FourFatesStudios.ProjectWarden.UI;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.GameSystems.Combat
{
    public class TargetingState : CombatState{
        
        private CombatManager combatManager;
        private CombatUIManager combatUIManager;
        private PlayerController playerController;
        
        public override void Enter(CombatManager combatManagerTemp, CombatUIManager combatUIManagerTemp){
            Debug.Log("Entering Targeting State: Select a Target");
            this.combatManager = combatManagerTemp;
            this.combatUIManager = combatUIManagerTemp;
            playerController = GameObject.FindFirstObjectByType<PlayerController>();
            BindInputs();
        }
        
        public void OnBack() {
            combatManager.SwitchState(combatManager.partySelectActionState);
        }
    }
}