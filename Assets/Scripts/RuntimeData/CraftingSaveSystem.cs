using FourFatesStudios.ProjectWarden.GameSystems.Crafting;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.RuntimeData
{
    public static class CraftingSaveSystem
    {
        private const string FileName = "crafting_save.json";
        private static string FullPath => Path.Combine(Application.persistentDataPath, FileName);

        /* ---------- SAVE ---------- */
        public static void Save()
        {
            var save = new CraftingSaveData();

            // potions Å® DTO
            foreach (var p in PlayerCraftingInventory.CraftedPotions)
            {
                var dto = new SavedPotion
                {
                    name = p.Name,
                    upgraded = p.Upgraded,
                    effectNames = p.Effects.Select(e => e.name).ToList()
                };
                save.potions.Add(dto);
            }

            // components Å® DTO
            foreach (var c in PlayerCraftingInventory.CraftedComponents)
            {
                var dto = new SavedComponent
                {
                    name = c.Name,
                    base1Name = c.Base1.name,
                    base2Name = c.Base2.name
                };
                save.components.Add(dto);
            }

            File.WriteAllText(FullPath, JsonUtility.ToJson(save, true));
            Debug.Log($"Crafting data saved to {FullPath}");
        }

        /* ---------- LOAD ---------- */
        public static void Load(
            PotionEffectDatabase effectDb,
            ItemDatabase itemDb)
        {
            if (!File.Exists(FullPath))
            {
                Debug.Log("No crafting save file found.");
                return;
            }

            var json = File.ReadAllText(FullPath);
            var save = JsonUtility.FromJson<CraftingSaveData>(json);

            PlayerCraftingInventory.CraftedPotions.Clear();
            PlayerCraftingInventory.CraftedComponents.Clear();

            // rebuild potions
            foreach (var dto in save.potions)
            {
                var effects = dto.effectNames
                                 .Select(effectDb.GetEffectByName)
                                 .Where(e => e != null)
                                 .ToList();

                PlayerCraftingInventory.CraftedPotions.Add(new CraftedPotion
                {
                    Name = dto.name,
                    Upgraded = dto.upgraded,
                    Effects = effects
                });
            }

            // rebuild components
            foreach (var dto in save.components)
            {
                var b1 = itemDb.Items.OfType<Ingredient>().FirstOrDefault(i => i.name == dto.base1Name);
                var b2 = itemDb.Items.OfType<Ingredient>().FirstOrDefault(i => i.name == dto.base2Name);

                if (b1 == null || b2 == null) continue;

                PlayerCraftingInventory.CraftedComponents.Add(new CraftedAlchemyComponent
                {
                    Name = dto.name,
                    Base1 = b1,
                    Base2 = b2
                });
            }

            Debug.Log("Crafting data loaded.");
        }
    }
}
