using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    
    public class CombatController : MonoBehaviour
    {
        private StatsComponent statsComponent;
        [SerializeField] public HealthComponent healthComponent;
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