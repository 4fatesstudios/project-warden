using System;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Controllers;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Class;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public enum CombatUIState{
    SelectingAction, SelectingSkill, SelectingItem, NormalAttack, Guard, Targeting
}

namespace FourFatesStudios.ProjectWarden.UI
{
    public class CombatUIManager : MonoBehaviour
    {

        private VisualElement root;
        public VisualElement skillsWrapper;
        public VisualElement itemsWrapper;
        private ScrollView skillsScrollView;
        
        public List<Button> buttonsList = new List<Button>();

        private CombatUIState CurrentState { get; set; } = CombatUIState.SelectingAction;

        private void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            skillsWrapper = root.Q<VisualElement>("SkillsWrapper");
            itemsWrapper = root.Q<VisualElement>("ItemsWrapper");
            skillsScrollView = root.Q<ScrollView>("SkillsScrollView");

            skillsWrapper.AddToClassList("ListHidden");
            itemsWrapper.AddToClassList("ListHidden");

        }

        public void SetState(CombatUIState newState)
        {
            CurrentState = newState;
        }

        public void OpenSkillsList()
        {
            // if (CurrentState != CombatUIState.SelectingAction)
            //     return;
            // SetState(CombatUIState.SelectingSkill);
            skillsWrapper.RemoveFromClassList("ListHidden");
        }

        public void OpenItemsList()
        {
            // if (CurrentState != CombatUIState.SelectingAction)
            //     return;
            // SetState(CombatUIState.SelectingItem);
            itemsWrapper.RemoveFromClassList("ListHidden");
        }

        public void ConfirmGuard()
        {
            if (CurrentState != CombatUIState.SelectingAction)
                return;
            SetState(CombatUIState.Guard);
            //skillsWrapper.RemoveFromClassList("GuardHidden");
        }

        public void SelectTarget()
        {
            if (CurrentState != CombatUIState.SelectingAction)
                return;
            SetState(CombatUIState.Targeting);
            //skillsWrapper.RemoveFromClassList("TargetingHidden");
        }

        public void OnBack()
        {
            if(!skillsWrapper.ClassListContains("ListHidden"))
                skillsWrapper.AddToClassList("ListHidden");
            else if(!itemsWrapper.ClassListContains("ListHidden"))
                itemsWrapper.AddToClassList("ListHidden");
            // if (CurrentState != CombatUIState.SelectingAction)
            // {
            //     skillsWrapper.AddToClassList("ListHidden");
            //     itemsWrapper.AddToClassList("ListHidden");
            //     SetState(CombatUIState.SelectingAction);
            // }
        }

        public void GenerateButtons(List<IntPair<Skill>> skills){
            buttonsList.Clear();
            skillsScrollView.Clear();
            int count = skills.Count;

            for (int i = 0; i < count; i++)
            {
                int index = i;
                Button button = new Button()
                {
                    text = $"{skills[i].key.SkillName}",
                    focusable = true
                };
                
                buttonsList.Add(button);
                skillsScrollView.Add(button);
            }
        }

        public void FocusSkillButton(int index) {
            Button button = buttonsList[index];
            button.Focus();
            skillsScrollView.ScrollTo(button);
            Debug.Log("focusing skill " + index);
        }
    }
}