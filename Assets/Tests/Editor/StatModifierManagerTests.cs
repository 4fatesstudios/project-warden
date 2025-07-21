using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Stats;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor {
    public class StatModifierManagerTests : MonoBehaviour {
        private Trinket _trinketA;
        private Trinket _trinketB;
        private StatModifierManager _statModifierManager;

        [SetUp]
        public void Setup() {
            _trinketA = ScriptableObject.CreateInstance<Trinket>();
            _trinketA.name = "Trinket A";
            _trinketA.ID = "1";
            _trinketA.StatModifierList = new StatModifierList {
                StatModifiers = new List<StatModifier> {
                    new (Stat.Agility, StatModifierType.Multiplicative, 5),
                    new (Stat.DivineDMG, StatModifierType.Multiplicative, -4),
                    new (Stat.CritChance, StatModifierType.Additive, 15)
                }
            };
            _trinketA.StatModifierList.SetSourceID(_trinketA.ID);

            _trinketB = ScriptableObject.CreateInstance<Trinket>();
            _trinketB.name = "Trinket B";
            _trinketB.ID = "2";
            _trinketB.StatModifierList = new StatModifierList {
                StatModifiers = new List<StatModifier> {
                    new (Stat.Agility, StatModifierType.Additive, -6),
                    new (Stat.DivineDMG, StatModifierType.Multiplicative, -2),
                    new (Stat.CritChance, StatModifierType.Additive, -20)
                }
            };
            _trinketB.StatModifierList.SetSourceID(_trinketB.ID);

            _statModifierManager = new StatModifierManager();

            Assert.IsNotNull(_trinketA);
            Assert.IsNotNull(_trinketB);
            Assert.IsNotNull(_statModifierManager);
            Assert.AreNotEqual(_trinketA.ID, _trinketB.ID);
        }

        [Test]
        public void HandleOnStatModifierListAdded_StatModifierList() {
            Assert.AreEqual(0, _statModifierManager.AllModifierLists.Count);
            Assert.AreEqual(0, _statModifierManager.AdditiveModifiers.Count);
            Assert.AreEqual(0, _statModifierManager.MultiplicativeModifiers.Count);
            
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketA.StatModifierList);
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketB.StatModifierList);
            
            Assert.AreEqual(2, _statModifierManager.AllModifierLists.Count);
            Assert.AreEqual(2, _statModifierManager.AdditiveModifiers.Count);
            Assert.AreEqual(2, _statModifierManager.MultiplicativeModifiers.Count);
        }

        [Test]
        public void HandleOnStatModifierListRemoved_StatModifierList() {
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketA.StatModifierList);
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketB.StatModifierList);
            
            StatModifierManager.OnStatModifierListRemoved?.Invoke(_trinketA.StatModifierList);
            
            Assert.AreEqual(1, _statModifierManager.AllModifierLists.Count);
            Assert.AreEqual(2, _statModifierManager.AdditiveModifiers.Count);
            Assert.AreEqual(1, _statModifierManager.MultiplicativeModifiers.Count);
        }

        [Test]
        public void GetAdditiveModifierValue_StatModifierList() {
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketA.StatModifierList);
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketB.StatModifierList);
            
            Assert.AreEqual(-5, _statModifierManager.GetAdditiveModifierValue(Stat.CritChance));
        }
        
        [Test]
        public void GetMultiplicativeModifierValue_StatModifierList() {
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketA.StatModifierList);
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketB.StatModifierList);
            
            Assert.AreEqual(-6, _statModifierManager.GetMultiplicativeModifierValue(Stat.DivineDMG));
        }

        [Test]
        public void GetAllModifiersForStat_StatModifierList() {
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketA.StatModifierList);
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketB.StatModifierList);

            var list = _statModifierManager.GetAllModifiersForStat(Stat.Agility);
            
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(5, list[1].Modifier.modifier);
            Assert.AreEqual(StatModifierType.Multiplicative, list[1].Modifier.type);
            Assert.AreEqual(-6, list[0].Modifier.modifier);
            Assert.AreEqual(StatModifierType.Additive, list[0].Modifier.type);
        }

        [Test]
        public void GetModifiersBySourceID_StatModifierList() {
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketA.StatModifierList);
            StatModifierManager.OnStatModifierListAdded?.Invoke(_trinketB.StatModifierList);
            
            var list = _statModifierManager.GetModifiersBySourceID(_trinketB.ID);
            
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(-20, list[0].Modifier.modifier);
            Assert.AreEqual(StatModifierType.Additive, list[0].Modifier.type);
        }
    }
}