using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Stats;
using FourFatesStudios.ProjectWarden.Stats;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor {
    public class StatsComponentTests : MonoBehaviour {
        private Trinket _trinketA;
        private Trinket _trinketB;
        private StatModifierManager _statModifierManager;
        private StatsComponent _statsComponent;
        private BaseStatSO _baseStatSO;
        private StatsAllocation _statsAllocation;
        private GameObject _gameObject;
        
        [SetUp]
        public void Setup() {
            _trinketA = ScriptableObject.CreateInstance<Trinket>();
            _trinketA.name = "Trinket A";
            _trinketA.ID = "1";
            _trinketA.StatModifierList = new StatModifierList{
                StatModifiers = new List<StatModifier>{
                    new(Stat.Vitality, StatModifierType.Multiplicative, 5),
                    new(Stat.DivineDMG, StatModifierType.Multiplicative, -4),
                    new(Stat.CritChance, StatModifierType.Additive, 15)
                }
            };
            _trinketA.StatModifierList.SetSourceID(_trinketA.ID);

            _trinketB = ScriptableObject.CreateInstance<Trinket>();
            _trinketB.name = "Trinket B";
            _trinketB.ID = "2";
            _trinketB.StatModifierList = new StatModifierList {
                StatModifiers = new List<StatModifier> {
                    new (Stat.Vitality, StatModifierType.Additive, -6),
                    new (Stat.DivineDMG, StatModifierType.Multiplicative, -2),
                    new (Stat.CritChance, StatModifierType.Additive, -20)
                }
            };
            _trinketB.StatModifierList.SetSourceID(_trinketB.ID);

            _statModifierManager = new StatModifierManager();

            _gameObject = new GameObject("Test");
            
            _statsComponent = _gameObject.AddComponent<StatsComponent>();

            _statsAllocation = new StatsAllocation();
            
            _baseStatSO = ScriptableObject.CreateInstance<BaseStatSO>();
            
            _statsComponent.SetBaseStats(_baseStatSO);
            _statsComponent.SetStatsManager(_statModifierManager);
            _statsComponent.SetStatsAllocation(_statsAllocation);

            Assert.IsNotNull(_trinketA);
            Assert.IsNotNull(_trinketB);
            Assert.IsNotNull(_statModifierManager);
            Assert.IsNotNull(_gameObject);
            Assert.IsNotNull(_statsAllocation);
            Assert.IsNotNull(_baseStatSO);
            Assert.IsNotNull(_statsComponent);
            Assert.AreNotEqual(_trinketA.ID, _trinketB.ID);
        }

        [Test]
        public void CalcVitality_StatsComponent() {
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketA.StatModifierList);
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketB.StatModifierList);
            
            _baseStatSO.BaseVitality = 10;
            _baseStatSO.BaseAgility = 0;
            
            _statsAllocation.SetVitality(0);
            
            Assert.AreEqual(20, _statsComponent.CalcVitality());
            
            _baseStatSO.BaseVitality = 4;
            Assert.AreEqual(0, _statsComponent.CalcVitality());
        }
        
        [Test]
        public void CalcMeleeDmg_StatsComponent() {
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketA.StatModifierList);
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketB.StatModifierList);
            
            _baseStatSO.BaseVitality = 10;
            _baseStatSO.BaseAgility = 0;
            
            _statsAllocation.SetVitality(0);
            
            Assert.AreEqual(20, _statsComponent.CalcVitality());
            
            _baseStatSO.BaseVitality = 4;
            Assert.AreEqual(0, _statsComponent.CalcVitality());
        }
    }
}