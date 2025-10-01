using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystems.CraftingMenu.AlchemyBookMenu.Sections
{
    [System.Serializable]
    public class BestiaryEntryUI : BaseEntry
    {
        [Header("Bestiary Properties")]
        public CreatureType creatureType;
        public string habitat;
        public string behavior;
        public DangerLevel dangerLevel;
        public string[] weaknesses;
        public string[] resistances;
        public string[] droppedIngredients;
        public bool hasBeenEncountered;
        public bool hasBeenDefeated;
        public int encounterCount;

        public override EntryType GetEntryType() => EntryType.Bestiary;

        public override VisualElement CreateEntryVisual()
        {
            var container = CreateBaseVisual();
            if (!isSeen) return container;

            // Creature type section
            var typeContainer = new VisualElement();
            typeContainer.AddToClassList("bestiary-type");
            
            var typeLabel = new Label($"Type: {creatureType}");
            typeLabel.AddToClassList("type-label");
            typeContainer.Add(typeLabel);
            container.Add(typeContainer);

            // Danger level indicator
            var dangerContainer = new VisualElement();
            dangerContainer.AddToClassList("bestiary-danger");
            
            var dangerLabel = new Label($"Danger Level: {dangerLevel}");
            dangerLabel.AddToClassList($"danger-{dangerLevel.ToString().ToLower()}");
            dangerContainer.Add(dangerLabel);
            container.Add(dangerContainer);

            // Habitat section
            if (!string.IsNullOrEmpty(habitat))
            {
                var habitatContainer = new VisualElement();
                habitatContainer.AddToClassList("bestiary-habitat");
                
                var habitatLabel = new Label($"Habitat: {habitat}");
                habitatLabel.AddToClassList("habitat-label");
                habitatContainer.Add(habitatLabel);
                container.Add(habitatContainer);
            }

            // Behavior section
            if (!string.IsNullOrEmpty(behavior))
            {
                var behaviorContainer = new VisualElement();
                behaviorContainer.AddToClassList("bestiary-behavior");
                
                var behaviorTitle = new Label("Behavior:");
                behaviorTitle.AddToClassList("behavior-title");
                behaviorContainer.Add(behaviorTitle);
                
                var behaviorLabel = new Label(behavior);
                behaviorLabel.AddToClassList("behavior-description");
                behaviorContainer.Add(behaviorLabel);
                container.Add(behaviorContainer);
            }

            // Weaknesses section
            if (weaknesses != null && weaknesses.Length > 0)
            {
                var weaknessContainer = new VisualElement();
                weaknessContainer.AddToClassList("bestiary-weaknesses");
                
                var weaknessTitle = new Label("Weaknesses:");
                weaknessTitle.AddToClassList("weakness-title");
                weaknessContainer.Add(weaknessTitle);
                
                foreach (var weakness in weaknesses)
                {
                    var weaknessLabel = new Label($"• {weakness}");
                    weaknessLabel.AddToClassList("weakness-label");
                    weaknessContainer.Add(weaknessLabel);
                }
                
                container.Add(weaknessContainer);
            }

            // Resistances section
            if (resistances != null && resistances.Length > 0)
            {
                var resistanceContainer = new VisualElement();
                resistanceContainer.AddToClassList("bestiary-resistances");
                
                var resistanceTitle = new Label("Resistances:");
                resistanceTitle.AddToClassList("resistance-title");
                resistanceContainer.Add(resistanceTitle);
                
                foreach (var resistance in resistances)
                {
                    var resistanceLabel = new Label($"• {resistance}");
                    resistanceLabel.AddToClassList("resistance-label");
                    resistanceContainer.Add(resistanceLabel);
                }
                
                container.Add(resistanceContainer);
            }

            // Dropped ingredients section
            if (droppedIngredients != null && droppedIngredients.Length > 0)
            {
                var dropsContainer = new VisualElement();
                dropsContainer.AddToClassList("bestiary-drops");
                
                var dropsTitle = new Label("Known Drops:");
                dropsTitle.AddToClassList("drops-title");
                dropsContainer.Add(dropsTitle);
                
                foreach (var ingredient in droppedIngredients)
                {
                    var dropLabel = new Label($"• {ingredient}");
                    dropLabel.AddToClassList("drop-label");
                    dropsContainer.Add(dropLabel);
                }
                
                container.Add(dropsContainer);
            }

            // Encounter statistics
            if (hasBeenEncountered)
            {
                var statsContainer = new VisualElement();
                statsContainer.AddToClassList("bestiary-stats");
                
                var encountersLabel = new Label($"Encounters: {encounterCount}");
                encountersLabel.AddToClassList("encounters-label");
                statsContainer.Add(encountersLabel);
                
                if (hasBeenDefeated)
                {
                    var defeatedLabel = new Label("✓ Defeated");
                    defeatedLabel.AddToClassList("defeated-label");
                    statsContainer.Add(defeatedLabel);
                }
                
                container.Add(statsContainer);
            }

            return container;
        }
    }
}