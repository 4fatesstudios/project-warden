using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystems.CraftingMenu.AlchemyBookMenu.Sections
{
    [System.Serializable]
    public class IngredientEntry : BaseEntry
    {
        [Header("Ingredient Properties")]
        public Aspect aspect;
        public List<DropSource> dropSources = new List<DropSource>();
        public List<string> availableRefinements = new List<string>();
        public bool hasCorruptedVariant;
        public List<string> discoveredInfusions = new List<string>(); // Only show if discovered

        public override EntryType GetEntryType() => EntryType.Ingredient;

        public override VisualElement CreateEntryVisual()
        {
            var container = CreateBaseVisual();
            if (!isSeen) return container;

            // Aspect section
            var aspectContainer = new VisualElement();
            aspectContainer.AddToClassList("ingredient-aspect");
            
            var aspectLabel = new Label($"Aspect: {aspect}");
            aspectLabel.AddToClassList("aspect-label");
            aspectContainer.Add(aspectLabel);
            container.Add(aspectContainer);

            // Drop sources section
            if (dropSources.Count > 0)
            {
                var sourcesContainer = new VisualElement();
                sourcesContainer.AddToClassList("ingredient-sources");
                
                var sourcesTitle = new Label("Sources:");
                sourcesTitle.AddToClassList("sources-title");
                sourcesContainer.Add(sourcesTitle);
                
                foreach (var source in dropSources)
                {
                    var sourceLabel = new Label($"• {source.sourceName} ({source.sourceType})");
                    if (!string.IsNullOrEmpty(source.location))
                        sourceLabel.text += $" - {source.location}";
                    sourceLabel.AddToClassList("source-label");
                    sourcesContainer.Add(sourceLabel);
                }
                
                container.Add(sourcesContainer);
            }

            // Refinements section
            if (availableRefinements.Count > 0)
            {
                var refineContainer = new VisualElement();
                refineContainer.AddToClassList("ingredient-refinements");
                
                var refineTitle = new Label("Available Refinements:");
                refineTitle.AddToClassList("refinements-title");
                refineContainer.Add(refineTitle);
                
                foreach (var refinement in availableRefinements)
                {
                    var refineLabel = new Label($"• {refinement}");
                    refineLabel.AddToClassList("refinement-label");
                    refineContainer.Add(refineLabel);
                }
                
                container.Add(refineContainer);
            }

            // Corrupted variant indicator
            if (hasCorruptedVariant)
            {
                var corruptedContainer = new VisualElement();
                corruptedContainer.AddToClassList("ingredient-corrupted");
                
                var corruptedLabel = new Label("⚠ Corrupted Variant Available");
                corruptedLabel.AddToClassList("corrupted-label");
                corruptedContainer.Add(corruptedLabel);
                container.Add(corruptedContainer);
            }

            // Discovered an infusions section
            if (discoveredInfusions.Count > 0)
            {
                var infusionsContainer = new VisualElement();
                infusionsContainer.AddToClassList("ingredient-infusions");
                
                var infusionsTitle = new Label("Discovered Infusions:");
                infusionsTitle.AddToClassList("infusions-title");
                infusionsContainer.Add(infusionsTitle);
                
                foreach (var infusion in discoveredInfusions)
                {
                    var infusionLabel = new Label($"• {infusion}");
                    infusionLabel.AddToClassList("infusion-label");
                    infusionsContainer.Add(infusionLabel);
                }
                
                container.Add(infusionsContainer);
            }

            return container;
        }
    }

    [System.Serializable]
    public class DropSource
    {
        public string sourceName;
        public SourceType sourceType;
        public string location; // Not implemented yet, but planned
        public Sprite sourceImage; // Not implemented yet, but planned
    }

    public enum SourceType
    {
        Monster,
        Enemy,
        Herb,
        Plant,
        Ore,
        Solvent,
        Farmable
    }
}
