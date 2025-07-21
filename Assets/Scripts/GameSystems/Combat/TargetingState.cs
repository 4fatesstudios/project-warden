using FourFatesStudios.ProjectWarden.Characters.Controllers;
using FourFatesStudios.ProjectWarden.UI;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.GameSystems.Combat
{
    public class TargetingState : CombatState{
        
        private CombatManager combatManager;
        private CombatUIManager combatUIManager;
        private PlayerController playerController;
        
        public override void Enter(CombatManager combatManager, CombatUIManager combatUIManager){
            Debug.Log("Entering Targeting State: Select a Target");
            this.combatManager = combatManager;
            this.combatUIManager = combatUIManager;
            playerController = GameObject.FindObjectOfType<PlayerController>();
            BindInputs();
        }
        
        public void OnBack() {
            combatManager.SwitchState(combatManager.partySelectActionState);
        }
    }
}