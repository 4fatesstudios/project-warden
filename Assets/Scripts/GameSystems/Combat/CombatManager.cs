using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Characters.Controllers;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.GameSystems.Combat;
using FourFatesStudios.ProjectWarden.UI;

namespace FourFatesStudios.ProjectWarden.GameSystems
{

    public class CombatManager : MonoBehaviour
    {
        private CombatUIManager combatUIManager;
        private CombatState currentState;
        public PartyState partyState = new PartyState();
        public PartySelectActionState partySelectActionState = new PartySelectActionState();
        public TargetingState targetingState = new TargetingState();

        private List<CombatController> partyLineup;
        private List<CombatController> enemyLineup;

        public CombatController currentPartyMember;

        private void Start()
        {
            currentState = partyState;
            combatUIManager = FindFirstObjectByType<CombatUIManager>();
            Debug.Log("Entering Party State");
            
            partyLineup.Add(FindFirstObjectByType<CombatController>());
            
            currentState.Enter(this, combatUIManager);
            
        }

        private void Update()
        {

        }

        public void SwitchState(CombatState newState)
        {
            currentState = newState;
            currentState.Enter(this, combatUIManager);
            OnDisableInputs();
            OnEnableInputs();
        }
        
        private void OnEnableInputs(){
            if (currentState == partySelectActionState)
            {
                PlayerController.OnOpenSkills += partySelectActionState.OpenSkillsList;
                PlayerController.OnOpenItems += partySelectActionState.OpenItemsList;
                PlayerController.OnGuard += partySelectActionState.ConfirmGuard;
                PlayerController.OnAttack += partySelectActionState.SelectTarget;
                PlayerController.OnBack += partySelectActionState.OnBack;
                
                PlayerController.OnCycleUp += partySelectActionState.CycleUp;
                PlayerController.OnCycleDown += partySelectActionState.CycleDown;
                PlayerController.OnCycleLeft += partySelectActionState.CycleLeft;
                PlayerController.OnCycleRight += partySelectActionState.CycleRight;
            }
            
            else if (currentState == targetingState)
            {
                PlayerController.OnBack += targetingState.OnBack;
            }
        }

        public void OnDisableInputs(){
            //Disable partySelectActionState inputs
            PlayerController.OnOpenSkills -= partySelectActionState.OpenSkillsList;
            PlayerController.OnOpenItems -= partySelectActionState.OpenItemsList;
            PlayerController.OnGuard -= partySelectActionState.ConfirmGuard;
            PlayerController.OnAttack -= partySelectActionState.SelectTarget;
            PlayerController.OnBack -= partySelectActionState.OnBack;
            
            //Disable targetingState inputs
            PlayerController.OnBack -= targetingState.OnBack;
        }
    }

    /*

    State Machine

    int MaxPartyActionPoints
    int MaxEnemyActionPoints
    int CurrentPartyActionPoints
    int CurrentEnemyActionPoints

    CombatController[] Enemy
    CombatController[] Party

    Start State (Enemy[], Party[])
    - Determine combat order (enemy or party)
    - For each Enemy.CombatController.OnDeath<this> += OnEnemyDeath;
    - For each PlayableCharacters.CombatController.OnDeath<this> += OnPartyDeath;
    - MaxEnemyActionPoints = Enemy.Length
    - MaxPartyActionPoints = Party.Length
    - If enemy first >>> "Enemy State"
    - If player first >>> "Party State"

    Enemy State (Enemy[])
    - CurrentEnemyActionPoints = MaxEnemyActionPoints
    - For each Enemy
    - - Apply DoT
    - - If cannot move decrease MaxEnemyActionPoints by 1, continue
    - - Enemy controller determines action
    - - call PerformAction(Action, Enemy)
    - - If MaxEnemyActionPoints <= 0 >>> "Party State"
    - If Enemy[] empty >>> "Victory State"
    - If Party[] empty >>> "Defeat State"

    Party State (Party[])
    - CurrentPartyActionPoints = MaxPartyActionPoints
    - For each PartyMember
    - - Apply DoT
    - - If cannot move decrease CurrentPartyActionPoints by 1, continue
    - - Awaiting Player Input for Action
    - - call PerformAction(Action, PlayableCharacter)
    - - If CurrentPartyActionPoints <= 0 >>> "Enemy State"
    - If Enemy[] empty >>> "Victory State"
    - If Party[] empty >>> "Defeat State"

    Victory State
    - For each Enemy.CombatController.OnDeath<this> -= OnEnemyDeath;
    - For each PlayableCharacters.CombatController.OnDeath<this> -= OnPartyDeath;
    - Play victory
    - Allot experience
    - Exit combat

    Defeat State
    - For each Enemy.CombatController.OnDeath<this> -= OnEnemyDeath;
    - For each PlayableCharacters.CombatController.OnDeath<this> -= OnPartyDeath;
    - Play defeat ("game over")
    - Death animation
    - Go to Game Over menu
    - Exit combat

    Functions

    OnPartyDeath(CombatController)
    - MaxPartyActionPoints -= 1
    - Party.Remove(CombatController)

    OnEnemyDeath(CombatController)
    - MaxEnemyActionPoints -= 1
    - Enemy.Remove(CombatController)

    PerformAction(Action, OriginParty)
    - animation
    - camera change
    - Action switch
    - - Attack > call Attack()
    - -
    - -
    - if Action == Pass decrement "OriginParty Action Points" by 0.5 else decrement by 1.0
    - return

    */
}