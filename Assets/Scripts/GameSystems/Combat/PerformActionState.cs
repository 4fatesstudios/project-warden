using FourFatesStudios.ProjectWarden.Characters.Controllers;
using FourFatesStudios.ProjectWarden.UI;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.GameSystems.Combat
{
    public class PerformActionState : CombatState{
        
        private CombatManager combatManager;
        private CombatUIManager combatUIManager;
        private PlayerController playerController;
        
        public override void Enter(CombatManager combatManager, CombatUIManager combatUIManager){
            Debug.Log("Entering Perform Action State: Playing animation");
            this.combatManager = combatManager;
            this.combatUIManager = combatUIManager;
            playerController = GameObject.FindObjectOfType<PlayerController>();
            // BindInputs();
            //need to implement animation playing later. for now just skip
            combatManager.SwitchState(combatManager.updateState);
        }
        
    }
}