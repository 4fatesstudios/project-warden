using UnityEngine;

public class CombatController : MonoBehaviour
{
    private StatsComponent statsComponent;

    private void Awake() {
        statsComponent = GetComponent<StatsComponent>();
    }
    
    
}
