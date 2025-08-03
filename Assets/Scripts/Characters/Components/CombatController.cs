using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    
    public class CombatController : MonoBehaviour
    {
        private StatsComponent statsComponent;
        private HealthComponent healthComponent;
        private NumoComponent numoComponent;
        
        [SerializeField] public ClassComponent classComponent;

        public bool IsAlive(){
            return healthComponent.GetCurrentHealth() > 0;
        }

        public bool CanMove(){
            return true;
        }
    }
    
}