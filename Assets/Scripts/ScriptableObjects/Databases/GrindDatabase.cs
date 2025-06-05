using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Grinds;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    [CreateAssetMenu(fileName = "NewGrindDatabase", menuName = "Databases/Grind Database")]
    public class GrindDatabase : ScriptableObject
    {
        private static GrindDatabase _instance;

        public static GrindDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<GrindDatabase>("Databases/GrindDatabase");
                if (_instance == null)
                    Debug.LogError("GrindDatabase asset not found in Resources/Databases/GrindDatabase");
                return _instance;
            }
        }

        [SerializeField] private List<Grind> grinds = new();
        private Dictionary<string, Grind> grindLookup;

        public IReadOnlyList<Grind> Grinds => grinds;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            grindLookup = new Dictionary<string, Grind>();
            foreach (var grind in grinds)
            {
                if (grind == null || grind.InputIngredient == null) continue;

                string key = grind.InputIngredient.ItemID;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogWarning($"Grind has an InputIngredient with missing ItemID: {grind.name}");
                    continue;
                }

                if (grindLookup.ContainsKey(key))
                {
                    Debug.LogWarning($"Duplicate grind for ItemID: {key} in {grind.name}");
                    continue;
                }

                grindLookup[key] = grind;
            }
        }

        public Grind GetGrindByInputId(string itemID)
        {
            if (grindLookup == null)
                BuildLookup();

            grindLookup.TryGetValue(itemID, out var grind);
            return grind;
        }
    }
}
