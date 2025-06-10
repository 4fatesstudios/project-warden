using System;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using UnityEngine.PlayerLoop;

namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    
    public class StatsComponent : MonoBehaviour 
    {
        private HealthComponent healthComponent;
        [SerializeField] private BaseStatSO baseStats;
        private StatsAllocation statsAllocation;
        private StatsModifier statsModifier;
        
        
        #region Stats Declarations
        private int level;
        private int defense;
        
        private int vitality;
        private int strength;
        private int wisdom;
        private int agility;
        private int luck;
        
        private int critChance;
        private int zeal;
        private int potency;
        
        private int meleeDmg;
        private int rangedDmg;
        private int corporealDmg;
        private int frigidDmg;
        private int scorchingDmg;
        private int causticDmg;
        private int arcDmg;
        private int divineDmg;
        
        private int corporealRes;
        private int frigidRes;
        private int scorchRes;
        private int causticRes;
        private int arcRes;
        private int divineRes;
        #endregion

        private void Awake(){
            UpdateStats();
        }

        private void UpdateStats(){
            healthComponent.UpdateMaxHealth(CalcMaxHealth());
            vitality = CalcVitality();
        }

        private int CalcMaxHealth(){
            return 0;
        }

        private int CalcVitality(){
            return baseStats.BaseVitality + statsAllocation.getVitality(); //add modifier
        }
        
        private int CalcStrength(){
            return baseStats.BaseStrength + statsAllocation.getVitality(); //add modifier
        }
    }
    
}