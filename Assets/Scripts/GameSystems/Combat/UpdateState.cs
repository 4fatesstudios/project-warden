using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Characters.Controllers;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.UI;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.GameSystems.Combat
{
    
    public class UpdateState : CombatState{
        
        private CombatManager combatManager;
        private CombatUIManager combatUIManager;
        private PlayerController playerController;
        
        public override void Enter(CombatManager combatManager, CombatUIManager combatUIManager){
            Debug.Log("Entering Update State: Apply Dmg/Eff");
            this.combatManager = combatManager;
            this.combatUIManager = combatUIManager;
            // BindInputs();
            SkillCalculation(this.combatUIManager.selectedSkill, this.combatManager.currentPartyMember, this.combatUIManager.selectedTarget);
        }

        public void SkillCalculation(Skill skill, CombatController player, CombatController target) {
            // Debug.Log("Health before: " + target.healthComponent.GetCurrentHealth());
            // target.healthComponent.UpdateHealth(target.healthComponent.GetCurrentHealth() - 10);
            // Debug.Log("Health after: " + target.healthComponent.GetCurrentHealth());
            combatManager.SwitchState(combatManager.partyState);
        }
    }
}