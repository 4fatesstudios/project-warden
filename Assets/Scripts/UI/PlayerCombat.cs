using System;
using FourFatesStudios.ProjectWarden.Characters.Controllers;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public enum CombatState{
    SelectingAction, SelectingSkill, SelectingItem, NormalAttack, Guard, Targeting
}

namespace FourFatesStudios.ProjectWarden.UI
{
    public class PlayerCombat : MonoBehaviour{
        
        private VisualElement root;
        private VisualElement skillsWrapper;
        private VisualElement itemsWrapper;

        private CombatState CurrentState{ get; set; } = CombatState.SelectingAction;

        private void Start(){
            root = GetComponent<UIDocument>().rootVisualElement;
            skillsWrapper = root.Q<VisualElement>("SkillsWrapper");
            itemsWrapper = root.Q<VisualElement>("ItemsWrapper");
            
            skillsWrapper.AddToClassList("ListHidden");
            itemsWrapper.AddToClassList("ListHidden");
        }

        public void SetState(CombatState newState){
            CurrentState = newState;
        }
        
        private void OpenSkillsList(){
            if (CurrentState != CombatState.SelectingAction)
                return;
            SetState(CombatState.SelectingSkill);
            skillsWrapper.RemoveFromClassList("ListHidden");
        }
        
        private void OpenItemsList(){
            if (CurrentState != CombatState.SelectingAction)
                return;
            SetState(CombatState.SelectingItem);
            itemsWrapper.RemoveFromClassList("ListHidden");
        }
        
        private void ConfirmGuard(){
            if (CurrentState != CombatState.SelectingAction)
                return;
            SetState(CombatState.Guard);
            //skillsWrapper.RemoveFromClassList("GuardHidden");
        }

        private void SelectTarget(){
            if (CurrentState != CombatState.SelectingAction)
                return;
            SetState(CombatState.Targeting);
            //skillsWrapper.RemoveFromClassList("TargetingHidden");
        }

        private void OnBack(){
            if (CurrentState != CombatState.SelectingAction){
                skillsWrapper.AddToClassList("ListHidden");
                itemsWrapper.AddToClassList("ListHidden");
                SetState(CombatState.SelectingAction);
            }
        }

        private void OnEnable(){
            PlayerController.OnOpenSkills += OpenSkillsList;
            PlayerController.OnOpenItems += OpenItemsList;
            PlayerController.OnGuard += ConfirmGuard;
            PlayerController.OnAttack += SelectTarget;
            // PlayerController.OnCycleUp += OpenSkillsList;
            // PlayerController.OnCycleDown += OpenSkillsList;
            // PlayerController.OnCycleLeft += OpenSkillsList;
            // PlayerController.OnCycleRight += OpenSkillsList;
            // PlayerController.OnSelect += OpenSkillsList;
            PlayerController.OnBack += OnBack;
        }

        private void OnDisable(){
            PlayerController.OnOpenSkills -= OpenSkillsList;
            PlayerController.OnOpenItems -= OpenItemsList;
            PlayerController.OnGuard -= ConfirmGuard;
            PlayerController.OnAttack -= SelectTarget;
            PlayerController.OnBack -= OnBack;
        }
    }
}