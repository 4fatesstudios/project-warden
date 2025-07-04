using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects
{
    [CreateAssetMenu(fileName = "PotionEffectDatabase", menuName = "Alchemy/Potion Effect Database")]
    public class PotionEffectDatabase : ScriptableObject
    {
        [SerializeField, Tooltip("All registered Potion Effects.")]
        private List<PotionEffect> potionEffects;

        public IReadOnlyList<PotionEffect> PotionEffects => potionEffects;

        public PotionEffect GetEffectByName(string effectName)
        {
            return potionEffects.Find(effect => effect.name == effectName);
        }

        public List<PotionEffect> GetEffectsByType(FourFatesStudios.ProjectWarden.Enums.ItemPotionType type)
        {
            return potionEffects.FindAll(effect => effect.EffectTypes.Contains(type));
        }
    #if UNITY_EDITOR
        private void OnValidate()
        {
            var seen = new HashSet<string>();

            for (int i = 0; i < potionEffects.Count; i++)
            {
                var effect = potionEffects[i];

                if (effect == null)
                {
                    Debug.LogWarning($"[{name}] Null entry at index {i} in potionEffects list.");
                    continue;
                }

                if (!seen.Add(effect.name))
                {
                    Debug.LogWarning($"[{name}] Duplicate PotionEffect with name '{effect.name}' found.");
                }

                // Basic data validation inside the effect
                int effectCount = effect.EffectTypes.Count;

                if (effect.EffectAspects.Count != effectCount ||
                    effect.Potencies.Count != effectCount ||
                    effect.Durations.Count != effectCount)
                {
                    Debug.LogWarning($"[{effect.name}] has mismatched list lengths. Ensure all lists are the same size.");
                }
            }
        }
    #endif
    }
}
