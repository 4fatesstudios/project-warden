using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Controllers;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Class;
using FourFatesStudios.ProjectWarden.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.Combat
{
    public class PartySelectActionState : CombatState{
        
        private CombatManager combatManager;
        private CombatUIManager combatUIManager;
        private PlayerController playerController;
        private int currentSkillIndex = 0;
        private List<Button> buttonsList;
        private List<IntPair<Skill>> availableClassSkills;
        
        public override void Enter(CombatManager combatManager, CombatUIManager combatUIManager){
            Debug.Log("Entering Select Action State: Select an Action");
            this.combatManager = combatManager;
            this.combatUIManager = combatUIManager;
            buttonsList = combatUIManager.buttonsList;
            playerController = GameObject.FindFirstObjectByType<PlayerController>();
            availableClassSkills = combatManager.currentPartyMember.classComponent.GetAvailableClassSkills();
            BindInputs();
        }

        public void OpenSkillsList() {
            combatUIManager.OpenSkillsList();
            combatUIManager.GenerateButtons(availableClassSkills);
            combatUIManager.FocusSkillButton(currentSkillIndex);
            playerController.EnableInputMapOnly(ActionMap.UINavigation);
        }
        
        public void OpenItemsList() {
            combatUIManager.OpenItemsList();
            playerController.EnableInputMapOnly(ActionMap.UINavigation);
        }
        
        public void ConfirmGuard() {
            // combatUIManager.OpenSkillsList();
            // playerController.EnableInputMapOnly(ActionMap.UINavigation);
        }
        
        public void SelectTarget() {
            Debug.Log("Using Skill: "  + availableClassSkills[currentSkillIndex].key.SkillName);
            combatManager.SwitchState(combatManager.targetingState);
        }
        
        public void OnBack() {
            if (!combatUIManager.skillsWrapper.ClassListContains("ListHidden") ||
                !combatUIManager.itemsWrapper.ClassListContains("ListHidden")) {
                combatUIManager.OnBack();
                currentSkillIndex = 0;
                playerController.EnableInputMapOnly(ActionMap.ActionSelection);
            }
            else
                combatManager.SwitchState(combatManager.partyState);
        }

        public void CycleUp() {
            currentSkillIndex = (currentSkillIndex - 1 + buttonsList.Count) % buttonsList.Count;
            combatUIManager.FocusSkillButton(currentSkillIndex);
            Debug.Log(currentSkillIndex);
        }
        
        public void CycleDown() {
            currentSkillIndex = (currentSkillIndex + 1) % buttonsList.Count;
            combatUIManager.FocusSkillButton(currentSkillIndex);
            Debug.Log(currentSkillIndex);
        }

        public void CycleLeft() {
            
        }

        public void CycleRight() {
            
        }

        public override void Exit(){
            Debug.Log("Exiting Select Action State");
            combatManager.SwitchState(new PartyState());
        }

        public override void BindInputs() {
            Debug.Log("Binding ActionSelection Inputs");
            playerController.EnableInputMapOnly(ActionMap.ActionSelection);
        }

        public override void UnbindInputs() {

        }
    }
}