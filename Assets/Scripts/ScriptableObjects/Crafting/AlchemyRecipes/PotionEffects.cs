using UnityEngine;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects
{
    [CreateAssetMenu(fileName = "NewPotionEffect", menuName = "Alchemy/Potion Effect")]
    public class PotionEffect : ScriptableObject
    {
        [SerializeField, Tooltip("Display name shown to the player.")]
        private string suffix = "Unnamed Item";

        [SerializeField, Tooltip("Description shown to the player.")]
        private string effectDescription = "Empty Description";

        [SerializeField, Tooltip("Unique identifier for this effect.")]
        private int id = -1;

        public string Suffix => suffix;
        public string EffectDescription => effectDescription;
        public int ID => id;

        [SerializeField, Tooltip("The types of effects this potion applies.")]
        private List<ItemPotionType> effectTypes;

        [SerializeField, Tooltip("The aspects tied to the effects.")]
        private List<Aspect> effectAspects;

        [SerializeField, Tooltip("How strong each effect is.")]
        private List<float> potencies;

        [SerializeField, Tooltip("How long each effect lasts (in rounds), if applicable.")]
        private List<float> durations;

        public IReadOnlyList<ItemPotionType> EffectTypes => effectTypes;
        public IReadOnlyList<Aspect> EffectAspects => effectAspects;
        public IReadOnlyList<float> Potencies => potencies;
        public IReadOnlyList<float> Durations => durations;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Basic string validations
            if (string.IsNullOrWhiteSpace(suffix))
                Debug.LogWarning($"[{name}] has an empty or missing suffix.");

            if (string.IsNullOrWhiteSpace(effectDescription))
                Debug.LogWarning($"[{name}] has an empty or missing description.");

            if (id < 0)
                Debug.LogWarning($"[{name}] has an invalid ID (must be >= 0).");

            // Effect types and matching fields
            int count = effectTypes?.Count ?? 0;

            if (count == 0)
            {
                Debug.LogWarning($"[{name}] has no defined effect types.");
                return;
            }

            // Ensure other lists are initialized
            effectAspects ??= new List<Aspect>();
            potencies ??= new List<float>();
            durations ??= new List<float>();

            TrimListToMatch(effectAspects, count, default(Aspect));
            TrimListToMatch(potencies, count, 1f);  // default potency
            TrimListToMatch(durations, count, 1f);  // default duration

            if (effectAspects.Count != count)
                Debug.LogWarning($"[{name}] 'effectAspects' count corrected to match 'effectTypes'.");
            if (potencies.Count != count)
                Debug.LogWarning($"[{name}] 'potencies' count corrected to match 'effectTypes'.");
            if (durations.Count != count)
                Debug.LogWarning($"[{name}] 'durations' count corrected to match 'effectTypes'.");

            for (int i = 0; i < count; i++)
            {
                if (potencies[i] <= 0)
                    Debug.LogWarning($"[{name}] Potency at index {i} is <= 0.");
                if (durations[i] < 0)
                    Debug.LogWarning($"[{name}] Duration at index {i} is negative.");
            }
        }

        private void TrimListToMatch<T>(List<T> list, int count, T defaultValue)
        {
            while (list.Count < count)
                list.Add(defaultValue);

            while (list.Count > count)
                list.RemoveAt(list.Count - 1);
        }
#endif
    }
}
