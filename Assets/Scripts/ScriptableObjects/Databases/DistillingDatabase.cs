using System.Collections.Generic;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Distills;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    [CreateAssetMenu(fileName = "NewDistillDatabase", menuName = "Databases/Distill Database")]
    public class DistillDatabase : ScriptableObject
    {
        private static DistillDatabase _instance;

        public static DistillDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<DistillDatabase>("Databases/DistillDatabase");
                if (_instance == null)
                    Debug.LogError("DistillDatabase asset not found in Resources/Databases/DistillDatabase");
                return _instance;
            }
        }

        [SerializeField] private List<Distill> distills = new();
        private Dictionary<string, Distill> distillLookup;

        public IReadOnlyList<Distill> Distills => distills;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            distillLookup = new Dictionary<string, Distill>();
            foreach (var distill in distills)
            {
                if (distill == null || distill.InputIngredient == null) continue;

                string key = distill.InputIngredient.ID;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogWarning($"Distill has an InputIngredient with missing ItemID: {distill.name}");
                    continue;
                }

                if (distillLookup.ContainsKey(key))
                {
                    Debug.LogWarning($"Duplicate distill for ItemID: {key} in {distill.name}");
                    continue;
                }

                distillLookup[key] = distill;
            }
        }

        public Distill GetDistillByInputId(string itemID)
        {
            if (distillLookup == null)
                BuildLookup();

            distillLookup.TryGetValue(itemID, out var distill);
            return distill;
        }
    }
}
