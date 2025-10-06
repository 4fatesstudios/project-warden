using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using GameSystems.CraftingMenu.AlchemyBookMenu.Sections;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyBook
{
    [CreateAssetMenu(fileName = "New Alchemy Book Entry Template", menuName = "Project Warden/Alchemy Book/Entry Template")]
    public class AlchemyBookEntryTemplate : ScriptableObject
    {
        [Header("Template Settings")]
        public string templateName = "Default Template";
        public string description = "Standard alchemy book entry template";
        
        [Header("Default Values")]
        public bool defaultIsSeen = false;
        public bool defaultIsCompleted = false;
        public string defaultTitle = "New Entry";
        public string defaultDescription = "Entry description here...";
        
        [Header("Entry Type Settings")]
        public AlchemyBookEntryType entryType = AlchemyBookEntryType.Ingredient;
        
        [Header("Visual Settings")]
        public Sprite defaultIcon;
        public Color defaultBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        public Color defaultTextColor = Color.white;
        
        [Header("Template Fields")]
        public AlchemyEntryField[] fields;
        
        public BaseEntry CreateEntry()
        {
            BaseEntry entry = entryType switch
            {
                AlchemyBookEntryType.Ingredient => ScriptableObject.CreateInstance<IngredientEntry>(),
                AlchemyBookEntryType.Recipe => ScriptableObject.CreateInstance<RecipeEntry>(),
                AlchemyBookEntryType.Bestiary => ScriptableObject.CreateInstance<BestiaryEntry>(),
                AlchemyBookEntryType.Help => ScriptableObject.CreateInstance<HelpEntry>(),
                _ => ScriptableObject.CreateInstance<HelpEntry>() // Default to HelpEntry instead of non-existent base
            };
            
            // Apply template defaults
            entry.title = defaultTitle;
            entry.description = defaultDescription;
            entry.isSeen = defaultIsSeen;
            
            // Apply template-specific settings
            ApplyTemplateSettings(entry);
            
            return entry;
        }
        
        private void ApplyTemplateSettings(BaseEntry entry)
        {
            // Apply common base settings
            entry.title = defaultTitle;
            entry.description = defaultDescription;
            entry.isSeen = defaultIsSeen;
            
            switch (entryType)
            {
                case AlchemyBookEntryType.Ingredient:
                    ApplyIngredientTemplate(entry as IngredientEntry);
                    break;
                case AlchemyBookEntryType.Recipe:
                    ApplyRecipeTemplate(entry as RecipeEntry);
                    break;
                case AlchemyBookEntryType.Bestiary:
                    ApplyBestiaryTemplate(entry as BestiaryEntry);
                    break;
                case AlchemyBookEntryType.Help:
                    ApplyHelpTemplate(entry as HelpEntry);
                    break;
            }
        }
        
        private void ApplyIngredientTemplate(IngredientEntry entry)
        {
            if (entry == null) return;
            
            entry.aspect = Aspect.Corporeal; // Default aspect
            entry.dropSources = new System.Collections.Generic.List<DropSource>();
        }
        
        private void ApplyRecipeTemplate(RecipeEntry entry)
        {
            if (entry == null) return;
            
            entry.requiredIngredients = new System.Collections.Generic.List<IngredientRequirement>();
            entry.infusions = new System.Collections.Generic.List<string>();
            entry.discoveredInfusions = new System.Collections.Generic.List<string>();
        }
        
        private void ApplyBestiaryTemplate(BestiaryEntry entry)
        {
            if (entry == null) return;
            
            entry.habitat = "Unknown";
            entry.behavior = "Unknown";
            entry.weaknesses = new string[0];
            entry.resistances = new string[0];
            entry.droppedIngredients = new string[0];
            entry.hasBeenEncountered = false;
            entry.hasBeenDefeated = false;
            entry.encounterCount = 0;
        }
        
        private void ApplyHelpTemplate(HelpEntry entry)
        {
            if (entry == null) return;
            
            // Help entries use base fields only
        }
    }
    
    [System.Serializable]
    public class AlchemyEntryField
    {
        public string fieldName;
        public AlchemyFieldType fieldType;
        public string defaultValue;
        public bool isRequired = false;
        public string tooltip = "";
    }
    
    public enum AlchemyBookEntryType
    {
        Ingredient,
        Recipe, 
        Bestiary,
        Help
    }
    
    public enum AlchemyFieldType
    {
        Text,
        Number,
        Boolean,
        Dropdown,
        TextArea,
        Color,
        Sprite
    }
}