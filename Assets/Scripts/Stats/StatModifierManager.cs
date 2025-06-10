using System;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Stats;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    public class StatModifierManager : IDisposable {
        public static Action<StatModifierList> OnStatModifierListAdded;
        public static Action<StatModifierList> OnStatModifierListRemoved;

        public StatModifierManager() {
            OnStatModifierListAdded += HandleOnStatModifierListAdded;
            OnStatModifierListRemoved += HandleOnStatModifierListRemoved;
        }

        private void HandleOnStatModifierListAdded(StatModifierList obj) {
            throw new NotImplementedException();
        }
        
        private void HandleOnStatModifierListRemoved(StatModifierList obj) {
            throw new NotImplementedException();
        }

        public int GetAdditiveModifierValue(Stat stat) {
            return 0;
        }

        public int GetMultiplicativeModifierValue(Stat stat) {
            return 0;
        }

        public void Dispose() {
            OnStatModifierListAdded -= HandleOnStatModifierListAdded;
            OnStatModifierListRemoved -= HandleOnStatModifierListRemoved;
        }
    }
}