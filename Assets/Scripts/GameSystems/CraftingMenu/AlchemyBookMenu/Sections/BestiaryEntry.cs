using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using GameSystems.CraftingMenu.AlchemyBookMenu.Sections;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyBookMenu.Sections
{
    [System.Serializable]
    public class BestiaryEntry : BaseEntry
    {
        [Header("Bestiary Properties")]
        public int level;
        public int hp;
        public int attack;
        public int defense;
        public int speed;
        
        [Header("Resistances & Weaknesses")]
        public List<Aspect> resistances = new List<Aspect>();
        public List<Aspect> weaknesses = new List<Aspect>();
        public List<Aspect> immunities = new List<Aspect>();
        
        [Header("Locations")]
        public List<string> encounterLocations = new List<string>();
        
        [Header("Abilities")]
        public List<string> skills = new List<string>();

        public override EntryType GetEntryType() => EntryType.Bestiary;

        public override VisualElement CreateEntryVisual()
        {
            var container = CreateBaseVisual();
            if (!isSeen) return container;

            // Stats section
            var statsContainer = new VisualElement();
            statsContainer.AddToClassList("bestiary-stats");
            
            var levelLabel = new Label($"Level: {level}");
            levelLabel.AddToClassList("stat-label");
            statsContainer.Add(levelLabel);
            
            var hpLabel = new Label($"HP: {hp}");
            hpLabel.AddToClassList("stat-label");
            statsContainer.Add(hpLabel);
            
            var attackLabel = new Label($"ATK: {attack}");
            attackLabel.AddToClassList("stat-label");
            statsContainer.Add(attackLabel);
            
            var defenseLabel = new Label($"DEF: {defense}");
            defenseLabel.AddToClassList("stat-label");
            statsContainer.Add(defenseLabel);
            
            var speedLabel = new Label($"SPD: {speed}");
            speedLabel.AddToClassList("stat-label");
            statsContainer.Add(speedLabel);
            
            container.Add(statsContainer);

            // Resistances section
            if (resistances.Count > 0 || weaknesses.Count > 0 || immunities.Count > 0)
            {
                var aspectContainer = new VisualElement();
                aspectContainer.AddToClassList("bestiary-aspects");
                
                if (weaknesses.Count > 0)
                {
                    var weakLabel = new Label($"Weaknesses: {string.Join(", ", weaknesses)}");
                    weakLabel.AddToClassList("weakness-label");
                    aspectContainer.Add(weakLabel);
                }
                
                if (resistances.Count > 0)
                {
                    var resLabel = new Label($"Resistances: {string.Join(", ", resistances)}");
                    resLabel.AddToClassList("resistance-label");
                    aspectContainer.Add(resLabel);
                }
                
                if (immunities.Count > 0)
                {
                    var immLabel = new Label($"Immunities: {string.Join(", ", immunities)}");
                    immLabel.AddToClassList("immunity-label");
                    aspectContainer.Add(immLabel);
                }
                
                container.Add(aspectContainer);
            }

            // Locations section
            if (encounterLocations.Count > 0)
            {
                var locationContainer = new VisualElement();
                locationContainer.AddToClassList("bestiary-locations");
                
                var locationsLabel = new Label($"Found in: {string.Join(", ", encounterLocations)}");
                locationsLabel.AddToClassList("location-label");
                locationContainer.Add(locationsLabel);
                
                container.Add(locationContainer);
            }

            // Skills section
            if (skills.Count > 0)
            {
                var skillsContainer = new VisualElement();
                skillsContainer.AddToClassList("bestiary-skills");
                
                var skillsTitle = new Label("Abilities:");
                skillsTitle.AddToClassList("skills-title");
                skillsContainer.Add(skillsTitle);
                
                foreach (var skill in skills)
                {
                    var skillLabel = new Label($"• {skill}");
                    skillLabel.AddToClassList("skill-label");
                    skillsContainer.Add(skillLabel);
                }
                
                container.Add(skillsContainer);
            }

            return container;
        }
    }
}
