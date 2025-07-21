using FourFatesStudios.ProjectWarden.Characters.Controllers;
using FourFatesStudios.ProjectWarden.UI;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.GameSystems.Combat
{
    public abstract class CombatState{

        public CombatState(){ }

        public virtual void Enter(CombatManager combatManager, CombatUIManager combatUIManager){ }
        public virtual void Exit() { }

        public virtual void BindInputs() { }
        
        public virtual void UnbindInputs() { }
        

    }
}