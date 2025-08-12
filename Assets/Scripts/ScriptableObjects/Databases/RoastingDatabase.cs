using System.Collections.Generic;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Roasts;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    [CreateAssetMenu(fileName = "NewRoastDatabase", menuName = "Databases/Roast Database")]
    public class RoastDatabase : ScriptableObject
    {
        private static RoastDatabase _instance;

        public static RoastDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<RoastDatabase>("Databases/RoastDatabase");
                if (_instance == null)
                    Debug.LogError("RoastDatabase asset not found in Resources/Databases/RoastDatabase");
                return _instance;
            }
        }

        [SerializeField] private List<Roast> roasts = new();
        private Dictionary<string, Roast> roastLookup;

        public IReadOnlyList<Roast> Roasts => roasts;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            roastLookup = new Dictionary<string, Roast>();
            foreach (var roast in roasts)
            {
                if (roast == null || roast.InputIngredient == null) continue;

                string key = roast.InputIngredient.ID;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogWarning($"Roast has an InputIngredient with missing ItemID: {roast.name}");
                    continue;
                }

                if (roastLookup.ContainsKey(key))
                {
                    Debug.LogWarning($"Duplicate roast for ItemID: {key} in {roast.name}");
                    continue;
                }

                roastLookup[key] = roast;
            }
        }

        public Roast GetRoastByInputId(string itemID)
        {
            if (roastLookup == null)
                BuildLookup();

            roastLookup.TryGetValue(itemID, out var roast);
            return roast;
        }
    }
}
