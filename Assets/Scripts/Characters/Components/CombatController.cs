using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    
    public class CombatController : MonoBehaviour
    {
        private StatsComponent statsComponent;
    
        private void Awake() {
            statsComponent = GetComponent<StatsComponent>();
        }
        
        
    }
    
}