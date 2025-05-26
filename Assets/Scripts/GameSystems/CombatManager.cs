using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour {
    
    public delegate void StartCombatDelegate(List<CombatController> playerParty, List<CombatController> enemyParty);
    public static StartCombatDelegate OnStartCombat;
    
    private void Start() {
        OnStartCombat += StartCombat;
    }

    private void StartCombat(List<CombatController> playerParty, List<CombatController> enemyParty) {
        // set up variables
        // enter State Machine here
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