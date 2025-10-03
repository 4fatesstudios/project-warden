using System.Collections.Generic;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.UI
{
    public class NewIngredientTracker : MonoBehaviour
    {
        private static NewIngredientTracker instance;
        public static NewIngredientTracker Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<NewIngredientTracker>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("NewIngredientTracker");
                        instance = go.AddComponent<NewIngredientTracker>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [SerializeField] private List<string> viewedIngredientIds = new List<string>();
        private const string VIEWED_INGREDIENTS_KEY = "ViewedIngredients";

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                LoadViewedIngredients();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public bool IsIngredientNew(Ingredient ingredient)
        {
            if (ingredient == null) return false;
            string ingredientId = ingredient.name; // Using name as unique ID
            return !viewedIngredientIds.Contains(ingredientId);
        }

        public void MarkIngredientAsViewed(Ingredient ingredient)
        {
            if (ingredient == null) return;
            
            string ingredientId = ingredient.name;
            if (!viewedIngredientIds.Contains(ingredientId))
            {
                viewedIngredientIds.Add(ingredientId);
                SaveViewedIngredients();
            }
        }

        public void ResetAllIngredients()
        {
            viewedIngredientIds.Clear();
            SaveViewedIngredients();
            Debug.Log("All ingredients marked as new!");
        }

        [ContextMenu("Reset All Ingredients (Mark as New)")]
        public void ResetAllIngredientsContextMenu()
        {
            ResetAllIngredients();
        }

        private void LoadViewedIngredients()
        {
            string savedData = PlayerPrefs.GetString(VIEWED_INGREDIENTS_KEY, "");
            if (!string.IsNullOrEmpty(savedData))
            {
                viewedIngredientIds = new List<string>(savedData.Split(','));
            }
        }

        private void SaveViewedIngredients()
        {
            string dataToSave = string.Join(",", viewedIngredientIds);
            PlayerPrefs.SetString(VIEWED_INGREDIENTS_KEY, dataToSave);
            PlayerPrefs.Save();
        }
    }
}