using System;
using UnityEngine;

public class StatsComponent : MonoBehaviour {
    
    [SerializeField] StatsSO defaultStats;
    [SerializeField] private int level;

    private HealthComponent healthComponent;

    private void Awake() {
        
    }
    
    
}
