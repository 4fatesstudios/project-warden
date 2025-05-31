using System;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects;

namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    
    public class StatsComponent : MonoBehaviour {
        
        [SerializeField] BaseStatSO defaultStats;
        [SerializeField] private int level;
    
        private HealthComponent healthComponent;
    
        private void Awake() {
            
        }
        
        
    }
    
}