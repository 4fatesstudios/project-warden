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
            playerController = GameObject.FindObjectOfType<PlayerController>();
            BindInputs();
            Exit();
        }

        public override void Exit(){
            Debug.Log("Exiting Party State");
            combatManager.SwitchState(combatManager.partySelectActionState);
        }
        
        public override void BindInputs() {
            Debug.Log("Binding Party Inputs");
            playerController.EnableInputMapOnly(ActionMap.ActionSelection);
        }
    }
}