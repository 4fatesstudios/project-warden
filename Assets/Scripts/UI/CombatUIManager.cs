using System;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Characters.Controllers;
using FourFatesStudios.ProjectWarden.GameSystems;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Class;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace FourFatesStudios.ProjectWarden.UI
{
    public class CombatUIManager : MonoBehaviour
    {
        private CombatManager combatManager;
        
        private VisualElement root;
        public VisualElement partyWrapper;
        public VisualElement skillsWrapper;
        public VisualElement itemsWrapper;
        public VisualElement targetingWrapper;
        private ScrollView skillsScrollView;
        private ScrollView itemsScrollView;
        private ScrollView targetingScrollView;
        
        public List<Button> skillButtonsList = new List<Button>();
        public List<Button> itemButtonsList = new List<Button>();
        public List<Button> targetingButtonsList = new List<Button>();
        
        public Skill selectedSkill;
        public Item selectedItem;
        public CombatController selectedTarget;

        private void Start()
        {
            combatManager = FindFirstObjectByType<CombatManager>();
            
            root = GetComponent<UIDocument>().rootVisualElement;
            skillsWrapper = root.Q<VisualElement>("SkillsWrapper");
            itemsWrapper = root.Q<VisualElement>("ItemsWrapper");
            targetingWrapper = root.Q<VisualElement>("TargetingWrapper");
            skillsScrollView = root.Q<ScrollView>("SkillsScrollView");
            itemsScrollView = root.Q<ScrollView>("ItemsScrollView");
            targetingScrollView = root.Q<ScrollView>("TargetingScrollView");

            skillsWrapper.AddToClassList("ListHidden");
            itemsWrapper.AddToClassList("ListHidden");
            targetingWrapper.AddToClassList("ListHidden");

        }

        public void OpenPartySelection() 
        {
            //partyWrapper.RemoveFromClassList("ListHidden");
            
        }

        public void OpenSkillsList()
        {
            // if (CurrentState != CombatUIState.SelectingAction)
            //     return;
            // SetState(CombatUIState.SelectingSkill);
            skillsWrapper.RemoveFromClassList("ListHidden");
            Debug.Log("Skills list no longer hidden");
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
            //skillsWrapper.RemoveFromClassList("GuardHidden");
        }

        public void SelectTarget()
        {
            targetingWrapper.RemoveFromClassList("ListHidden");
        }
        
        public void GenerateTargetingButtons(List<CombatController> targets){
            targetingButtonsList.Clear();
            targetingScrollView.Clear();

            foreach (CombatController target in targets)
            {
                Button button = new Button(() =>
                {
                    Debug.Log("Target clicked: " + target.name);
                    selectedTarget = target;
                    RemoveUIMenu();
                    combatManager.SwitchState(combatManager.updateState);
                })
                {
                    text = $"{target.name}",
                    focusable = true
                };
                targetingButtonsList.Add(button);
                targetingScrollView.Add(button);
            }
        }

        public void RemoveUIMenu()
        {
            if (!skillsWrapper.ClassListContains("ListHidden")) {
                skillsWrapper.AddToClassList("ListHidden");
                for (int i = 0; i < skillButtonsList.Count; i++) {
                    skillButtonsList[i].focusable = false;
                }
            }
            else if (!itemsWrapper.ClassListContains("ListHidden")) {
                itemsWrapper.AddToClassList("ListHidden");
                for (int i = 0; i < skillButtonsList.Count; i++) {
                    itemButtonsList[i].focusable = false;
                }
            }
            else if (!targetingWrapper.ClassListContains("ListHidden"))
            {
                targetingWrapper.AddToClassList("ListHidden");
                for (int i = 0; i < targetingButtonsList.Count; i++)
                {
                    targetingButtonsList[i].focusable = false;
                }
            }
        }

        public void GenerateSkillsButtons(List<IntPair<Skill>> skills){
            skillButtonsList.Clear();
            skillsScrollView.Clear();

            for (int i = 0; i < skills.Count; i++)
            {
                Skill skill = skills[i].key;
                
                Button button = new Button(() =>
                {
                    Debug.Log("Skill clicked: " + skill.SkillName);
                    selectedSkill = skill;
                    RemoveUIMenu();
                    combatManager.SwitchState(combatManager.targetingState);
                })
                {
                    text = $"{skill.SkillName}",
                    focusable = true
                };
                
                skillButtonsList.Add(button);
                skillsScrollView.Add(button);
            }
        }

        public void FocusSkillButton(int index) {
            Button button = skillButtonsList[index];
            button.Focus();
            skillsScrollView.ScrollTo(button);
            Debug.Log("focusing skill " + index);
        }
        
        public void GenerateItemButtons(List<Item> items){
            itemButtonsList.Clear();
            itemsScrollView.Clear();

            foreach (Item item in items)
            {
                Button button = new Button(() =>
                {
                    Debug.Log("Item clicked: " + item.ItemName);
                    selectedItem = item;
                })
                {
                    text = $"{item.ItemName}",
                    focusable = true
                };
                
                itemButtonsList.Add(button);
                itemsScrollView.Add(button);
            }
        }

        public void FocusItemButton(int index) {
            Button button = itemButtonsList[index];
            button.Focus();
            itemsScrollView.ScrollTo(button);
            Debug.Log("focusing item " + index);
        }

    }
}