using UnityEngine;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects
{
    [CreateAssetMenu(fileName = "NewPotionEffect", menuName = "Alchemy/Potion Effect")]
    public class PotionEffect : ScriptableObject
    {
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
            int count = effectTypes.Count;

            if (effectAspects.Count != count)
                Debug.LogWarning($"[{name}] effectAspects count ({effectAspects.Count}) does not match effectTypes count ({count}).");

            if (potencies.Count != count)
                Debug.LogWarning($"[{name}] potencies count ({potencies.Count}) does not match effectTypes count ({count}).");

            if (durations.Count != count)
                Debug.LogWarning($"[{name}] durations count ({durations.Count}) does not match effectTypes count ({count}).");
        }
#endif
    }
}
