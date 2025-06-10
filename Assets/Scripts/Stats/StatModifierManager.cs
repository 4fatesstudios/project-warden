using System;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.Stats;

namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    public class StatModifierManager : IDisposable
    {
        public static Action<StatModifierList> OnStatModifierListAdded;
        public static Action<StatModifierList> OnStatModifierListRemoved;

        private readonly List<StatModifierList> allModifierLists = new();
        private readonly Dictionary<Stat, List<StatModifierEntry>> additiveModifiers = new();
        private readonly Dictionary<Stat, List<StatModifierEntry>> multiplicativeModifiers = new();

        public StatModifierManager() {
            OnStatModifierListAdded += HandleOnStatModifierListAdded;
            OnStatModifierListRemoved += HandleOnStatModifierListRemoved;
        }

        private void HandleOnStatModifierListAdded(StatModifierList list) {
            allModifierLists.Add(list);
            foreach (var mod in list.StatModifiers) {
                var entry = new StatModifierEntry(mod, list.SourceModifierID);
                var targetDict = mod.type == StatModifierType.Additive ? additiveModifiers : multiplicativeModifiers;
                if (!targetDict.TryGetValue(mod.stat, out var modList)) {
                    modList = new List<StatModifierEntry>();
                    targetDict[mod.stat] = modList;
                }
                modList.Add(entry);
            }
        }


        private void HandleOnStatModifierListRemoved(StatModifierList list) {
            allModifierLists.RemoveAll(l => l.SourceModifierID == list.SourceModifierID);
            foreach (var mod in list.StatModifiers) {
                var targetDict = mod.type == StatModifierType.Additive ? additiveModifiers : multiplicativeModifiers;
                if (targetDict.TryGetValue(mod.stat, out var modList)) {
                    modList.RemoveAll(entry => entry.SourceID == list.SourceModifierID && entry.Modifier == mod);
                    if (modList.Count == 0)
                        targetDict.Remove(mod.stat);
                }
            }
        }

        public int GetAdditiveModifierValue(Stat stat) {
            if (!additiveModifiers.TryGetValue(stat, out var entries)) return 0;
            int total = 0;
            foreach (var entry in entries)
                total += entry.Modifier.modifier;
            return total;
        }

        public int GetMultiplicativeModifierValue(Stat stat) {
            if (!multiplicativeModifiers.TryGetValue(stat, out var entries)) return 0;
            int total = 0;
            foreach (var entry in entries) 
                total += entry.Modifier.modifier;
            return total;
        }

        public List<StatModifierEntry> GetAllModifiersForStat(Stat stat) {
            List<StatModifierEntry> result = new();
            if (additiveModifiers.TryGetValue(stat, out var adds))
                result.AddRange(adds);
            if (multiplicativeModifiers.TryGetValue(stat, out var mults))
                result.AddRange(mults);
            return result;
        }
        
        public List<StatModifierEntry> GetModifiersBySourceID(string sourceID) {
            List<StatModifierEntry> results = new();
            foreach (var dict in new[] { additiveModifiers, multiplicativeModifiers }) {
                foreach (var list in dict.Values) {
                    results.AddRange(list.FindAll(entry => entry.SourceID == sourceID));
                }
            }
            return results;
        }

        public void Dispose() {
            OnStatModifierListAdded -= HandleOnStatModifierListAdded;
            OnStatModifierListRemoved -= HandleOnStatModifierListRemoved;
        }
        
    }
    
    public class StatModifierEntry {
        public StatModifier Modifier { get; }
        public string SourceID { get; }

        public StatModifierEntry(StatModifier modifier, string sourceID) {
            Modifier = modifier;
            SourceID = sourceID;
        }
    }


}