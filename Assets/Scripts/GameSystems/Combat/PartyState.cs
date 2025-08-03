using FourFatesStudios.ProjectWarden.Characters.Controllers;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.UI;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.GameSystems.Combat
{
    public class PartyState : CombatState{
        
        private CombatManager combatManager;
        private PlayerController playerController;
        
        public override void Enter(CombatManager combatManager, CombatUIManager combatUIManager){
            Debug.Log("Entering Party State: Select Party Member");
            this.combatManager = combatManager;
            //implement selecting character ui and implementation
            playerController = GameObject.FindFirstObjectByType<PlayerController>();
            BindInputs();
            combatUIManager.OpenPartySelection();
            combatManager.SwitchState(combatManager.partySelectActionState);
        }
        
        public override void BindInputs() {
            Debug.Log("Binding Party Inputs");
            playerController.EnableInputMapOnly(ActionMap.ActionSelection);
        }
    }
}