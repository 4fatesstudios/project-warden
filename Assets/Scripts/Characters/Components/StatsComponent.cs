using System;
using System.Runtime.InteropServices;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Stats;
using UnityEngine.PlayerLoop;

namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    
    public class StatsComponent : MonoBehaviour 
    {
        private HealthComponent healthComponent;
        private NumoComponent numoComponent;
        [SerializeField] private BaseStatSO baseStats;
        private StatsAllocation statsAllocation;
        private StatModifierManager statModifierManager;
        
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
        private int scorchDmg;
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
        
        // Removed for testing
        // private void Awake(){
        //     statModifierManager = new StatModifierManager();
        //     UpdateStats();
        // }

        private void UpdateStats(){
            healthComponent.UpdateMaxHealth(CalcMaxHealth());
            vitality = CalcVitality();
            strength = CalcStrength();
            wisdom = CalcWisdom();
            agility = CalcAgility();
            luck = CalcLuck();

            healthComponent.UpdateMaxHealth(CalcMaxHealth());
            numoComponent.UpdateMaxNumo(CalcMaxNumo());
            
            critChance = CalcCritChance();
            zeal = CalcZeal();
            potency = CalcPotency();

            meleeDmg = CalcMeleeDmg();
            rangedDmg = CalcRangedDmg();
            corporealDmg = CalcCorporealDmg();
            frigidDmg = CalcFrigidDmg();
            scorchDmg = CalcScorchDmg();
            causticDmg = CalcCausticDmg();
            arcDmg = CalcArcDmg();
            divineDmg = CalcDivineDmg();

            corporealRes = CalcCorporealRes();
            frigidRes = CalcFrigidRes();
            scorchRes = CalcScorchRes();
            causticRes = CalcCausticRes();
            arcRes = CalcArcRes();
            divineRes = CalcDivineRes();
        }

        public int CalcVitality(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.Vitality);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.Vitality);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            float value = (baseStats.BaseVitality + statsAllocation.GetVitality() + aMod) * mMod;
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcStrength(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.Strength);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.Strength);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            float value = (baseStats.BaseStrength + statsAllocation.GetStrength() + aMod) * mMod;
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcWisdom(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.Wisdom);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.Wisdom);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            float value = (baseStats.BaseWisdom + statsAllocation.GetWisdom() + aMod) * mMod;
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcAgility(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.Agility);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.Agility);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            float value = (baseStats.BaseAgility + statsAllocation.GetAgility() + aMod) * mMod;
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcLuck(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.Luck);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.Luck);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            float value = (baseStats.BaseLuck + statsAllocation.GetLuck() + aMod) * mMod;
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
                
        private int CalcMaxHealth(){
            int healthPerVitalityPoint = 10;
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.Health); // +10 HP
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.Health); // +10% HP
            mMod = ((mMod == 0) ? 1 : mMod);
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            float value = (baseStats.BaseHP + (vitality * healthPerVitalityPoint) + aMod) * mMod;
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcMaxNumo(){
            int numoPerWisdomPoint = 10;
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.Wisdom); // +10 HP
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.Wisdom); // +10% HP
            mMod = ((mMod == 0) ? 1 : mMod);
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            float value = (baseStats.BaseNumo + (vitality * numoPerWisdomPoint) + aMod) * mMod;
            value = (value < 0) ? 0 : value;
            return (int)value;
        }

        private int CalcCritChance(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.CritChance);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.CritChance);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(luck - midpoint) / softness));
            
            float value = ((baseStats.BaseCritChance * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }

        private int CalcZeal(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.Zeal);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.Zeal);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);

            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(strength - midpoint) / softness));
            
            float value = ((baseStats.BaseZeal * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }

        private int CalcPotency(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.Potency);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.Potency);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);

            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(luck - midpoint) / softness));
            
            float value = ((baseStats.BasePotency * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }

        private int CalcMeleeDmg(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.MeleeDMG);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.MeleeDMG);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            // Logistic curve parameters
            float maxBonusPercent = 50f;  // max +50% dmg from strength
            float midpoint = 50f;         // 50 STR gives half of max bonus
            float softness = 20f;         // higher = smoother curve

            // Calculate strength-based bonus % using logistic function
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(strength - midpoint) / softness));
            
            // Final melee damage
            float value = ((baseStats.BaseMeleeDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }

        private int CalcRangedDmg(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.RangedDMG);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.RangedDMG);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(agility - midpoint) / softness));
            
            float value = ((baseStats.BaseRangedDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }

        private int CalcCorporealDmg(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.CorporealDMG);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.CorporealDMG);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseCorporealDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcFrigidDmg(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.FrigidDMG);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.FrigidDMG);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseFrigidDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcScorchDmg(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.ScorchDMG);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.ScorchDMG);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseScorchDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcCausticDmg(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.CausticDMG);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.CausticDMG);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseCausticDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcArcDmg(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.ArcDMG);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.ArcDMG);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseArcDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcDivineDmg(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.DivineDMG);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.DivineDMG);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseDivineDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcCorporealRes(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.CorporealRES);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.CorporealRES);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            
            float value = (baseStats.BaseCorporealRes + aMod) * mMod;
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcFrigidRes(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.FrigidRES);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.FrigidRES);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseDivineDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcScorchRes(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.ScorchRES);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.ScorchRES);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);
            
            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseDivineDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcCausticRes(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.CausticRES);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.CausticRES);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);

            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseDivineDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcArcRes(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.ArcRES);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.ArcRES);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);

            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseDivineDmg * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }
        
        private int CalcDivineRes(){
            int aMod = statModifierManager.GetAdditiveModifierValue(Stat.DivineRES);
            float mMod = statModifierManager.GetMultiplicativeModifierValue(Stat.DivineRES);
            mMod = (mMod == 0) ? 1 : mMod;
            mMod = (mMod > 0) ? 1 + (mMod / 100.0f) : 1 - (mMod / 100.0f);

            float maxBonusPercent = 50f;
            float midpoint = 50f;
            float softness = 20f;
            
            float bonusPercent = maxBonusPercent / (1f + Mathf.Exp(-(wisdom - midpoint) / softness));
            
            float value = ((baseStats.BaseDivineRes * bonusPercent) + aMod) * mMod;
            
            value = (value < 0) ? 0 : value;
            return (int)value;
        }

        //For testing
        public void SetBaseStats(BaseStatSO value){
            baseStats = value;
        }

        public void SetStatsManager(StatModifierManager value){
            statModifierManager = value;
        }

        public void SetStatsAllocation(StatsAllocation value){
            statsAllocation = value;
        }
    }
    
}