using FourFatesStudios.ProjectWarden.Enums;
using GameSystems.CraftingMenu.AlchemyBookMenu.Sections;
using UnityEditor;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class AlchemyBookEntryCreator : EditorWindow
    {
        [MenuItem("Tools/Alchemy Book/Create Demo Entries")]
        public static void CreateDemoEntries()
        {
            CreateDemoIngredientEntries();
            CreateDemoRecipeEntries();
            CreateDemoBestiaryEntries();
            CreateDemoHelpEntries();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Demo Alchemy Book entries created successfully!");
        }

        private static void CreateDemoIngredientEntries()
        {
            // Fire Claw Entry
            var fireClawEntry = ScriptableObject.CreateInstance<IngredientEntry>();
            fireClawEntry.title = "Fire Claw";
            fireClawEntry.description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. A sharp claw infused with fire magic, commonly found on flame-touched beasts. The claw retains its heat even after extraction, making it a valuable component for fire-based potions. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris.";
            fireClawEntry.isSeen = true;
            fireClawEntry.aspect = Aspect.Scorch;
            fireClawEntry.dropSources.AddRange(new DropSource[]
            {
                new DropSource
                {
                    sourceName = "Flame Wolf",
                    sourceType = SourceType.Enemy,
                    dropChance = "15%",
                    location = "Scorched Plains"
                },
                new DropSource
                {
                    sourceName = "Fire Salamander",
                    sourceType = SourceType.Monster,
                    dropChance = "25%",
                    location = "Volcanic Caves"
                }
            });
            fireClawEntry.availableRefinements.AddRange(new string[] { "Grinding", "Roasting" });
            fireClawEntry.discoveredInfusions.AddRange(new string[] { "Heat Enhancement", "Burn Effect" });

            AssetDatabase.CreateAsset(fireClawEntry, "Assets/Resources/AlchemyBook/Ingredients/FireClawEntry.asset");

            // Shadow Herb Entry
            var shadowHerbEntry = ScriptableObject.CreateInstance<IngredientEntry>();
            shadowHerbEntry.title = "Shadow Herb";
            shadowHerbEntry.description = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium. A mysterious herb that grows in darkness, thriving in areas devoid of light. Its leaves seem to absorb surrounding shadows, making it essential for stealth and darkness-based alchemical preparations.";
            shadowHerbEntry.isSeen = true;
            shadowHerbEntry.aspect = Aspect.Divine;
            shadowHerbEntry.dropSources.AddRange(new DropSource[]
            {
                new DropSource
                {
                    sourceName = "Dark Forest",
                    sourceType = SourceType.Herb,
                    dropChance = "60%",
                    location = "Whispering Woods"
                },
                new DropSource
                {
                    sourceName = "Shadow Sprite",
                    sourceType = SourceType.Monster,
                    dropChance = "30%",
                    location = "Umbral Caverns"
                }
            });
            shadowHerbEntry.availableRefinements.AddRange(new string[] { "Distillation", "Grinding" });
            shadowHerbEntry.hasCorruptedVariant = true;
            shadowHerbEntry.discoveredInfusions.AddRange(new string[] { "Stealth Enhancement", "Shadow Manipulation" });

            AssetDatabase.CreateAsset(shadowHerbEntry, "Assets/Resources/AlchemyBook/Ingredients/ShadowHerbEntry.asset");

            // Frozen Dew Entry
            var frozenDewEntry = ScriptableObject.CreateInstance<IngredientEntry>();
            frozenDewEntry.title = "Frozen Dew";
            frozenDewEntry.description = "Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit. Crystallized droplets of morning dew from the highest mountain peaks. These delicate crystals contain the essence of pure cold and are prized for their cooling properties in alchemical mixtures.";
            frozenDewEntry.isSeen = true;
            frozenDewEntry.aspect = Aspect.Frigid;
            frozenDewEntry.dropSources.AddRange(new DropSource[]
            {
                new DropSource
                {
                    sourceName = "Mountain Peaks",
                    sourceType = SourceType.Farmable,
                    dropChance = "80%",
                    location = "Frostpeak Mountains"
                },
                new DropSource
                {
                    sourceName = "Ice Elemental",
                    sourceType = SourceType.Monster,
                    dropChance = "45%",
                    location = "Frozen Wastes"
                }
            });
            frozenDewEntry.availableRefinements.AddRange(new string[] { "Distillation" });
            frozenDewEntry.discoveredInfusions.AddRange(new string[] { "Frost Effect", "Temperature Regulation" });

            AssetDatabase.CreateAsset(frozenDewEntry, "Assets/Resources/AlchemyBook/Ingredients/FrozenDewEntry.asset");

            // Wind Essence Entry
            var windEssenceEntry = ScriptableObject.CreateInstance<IngredientEntry>();
            windEssenceEntry.title = "Wind Essence";
            windEssenceEntry.description = "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis. A concentrated form of elemental air magic, captured in crystalline form. This essence pulses with the power of storms and gentle breezes alike, making it invaluable for mobility and weather-based enchantments.";
            windEssenceEntry.isSeen = true;
            windEssenceEntry.aspect = Aspect.Arc;
            windEssenceEntry.dropSources.AddRange(new DropSource[]
            {
                new DropSource
                {
                    sourceName = "Storm Elemental",
                    sourceType = SourceType.Monster,
                    dropChance = "35%",
                    location = "Tempest Cliffs"
                },
                new DropSource
                {
                    sourceName = "Sky Crystals",
                    sourceType = SourceType.Ore,
                    dropChance = "20%",
                    location = "Floating Islands"
                }
            });
            windEssenceEntry.availableRefinements.AddRange(new string[] { "Grinding", "Distillation", "Roasting" });
            windEssenceEntry.discoveredInfusions.AddRange(new string[] { "Speed Enhancement", "Levitation" });

            AssetDatabase.CreateAsset(windEssenceEntry, "Assets/Resources/AlchemyBook/Ingredients/WindEssenceEntry.asset");

            // Earth Shard Entry
            var earthShardEntry = ScriptableObject.CreateInstance<IngredientEntry>();
            earthShardEntry.title = "Earth Shard";
            earthShardEntry.description = "Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis. A fragment of pure earth magic crystallized into solid form. These shards are incredibly durable and are often used in potions that enhance physical strength and provide defensive benefits.";
            earthShardEntry.isSeen = true;
            earthShardEntry.aspect = Aspect.Corporeal;
            earthShardEntry.dropSources.AddRange(new DropSource[]
            {
                new DropSource
                {
                    sourceName = "Rock Golem",
                    sourceType = SourceType.Monster,
                    dropChance = "40%",
                    location = "Crystal Caves"
                },
                new DropSource
                {
                    sourceName = "Earth Nodes",
                    sourceType = SourceType.Ore,
                    dropChance = "60%",
                    location = "Underground Tunnels"
                }
            });
            earthShardEntry.availableRefinements.AddRange(new string[] { "Grinding" });
            earthShardEntry.discoveredInfusions.AddRange(new string[] { "Stone Skin", "Strength Enhancement" });

            AssetDatabase.CreateAsset(earthShardEntry, "Assets/Resources/AlchemyBook/Ingredients/EarthShardEntry.asset");

            // Fire Talon Entry
            var fireTalonEntry = ScriptableObject.CreateInstance<IngredientEntry>();
            fireTalonEntry.title = "Fire Talon";
            fireTalonEntry.description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt. A larger, more potent version of fire claws, typically harvested from apex flame predators. The talon burns with intense heat and is prized for creating powerful fire-based enchantments.";
            fireTalonEntry.isSeen = true;
            fireTalonEntry.aspect = Aspect.Scorch;
            fireTalonEntry.dropSources.AddRange(new DropSource[]
            {
                new DropSource
                {
                    sourceName = "Fire Dragon",
                    sourceType = SourceType.Monster,
                    dropChance = "5%",
                    location = "Dragon's Lair"
                },
                new DropSource
                {
                    sourceName = "Phoenix",
                    sourceType = SourceType.Monster,
                    dropChance = "10%",
                    location = "Elemental Plane"
                }
            });
            fireTalonEntry.availableRefinements.AddRange(new string[] { "Grinding", "Roasting", "Distillation" });
            fireTalonEntry.discoveredInfusions.AddRange(new string[] { "Inferno", "Fire Immunity", "Flame Aura" });

            AssetDatabase.CreateAsset(fireTalonEntry, "Assets/Resources/AlchemyBook/Ingredients/FireTalonEntry.asset");
        }

        private static void CreateDemoRecipeEntries()
        {
            // Serpent's Dew Recipe Entry
            var serpentsDewRecipeEntry = ScriptableObject.CreateInstance<RecipeEntry>();
            serpentsDewRecipeEntry.title = "Serpent's Dew Recipe";
            serpentsDewRecipeEntry.description = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque. An ancient recipe for brewing the legendary Serpent's Dew potion. This complex recipe requires fire-based and cold-based ingredients to achieve the perfect balance of agility enhancement and poison resistance.";
            serpentsDewRecipeEntry.isSeen = true;
            serpentsDewRecipeEntry.recipeType = RecipeType.UniquePotion;
            serpentsDewRecipeEntry.isUniquePotionRecipe = true;
            serpentsDewRecipeEntry.requiredIngredients.AddRange(new IngredientRequirement[]
            {
                new IngredientRequirement
                {
                    ingredientName = "Fire Claw",
                    quantity = 1,
                    isDiscovered = true
                },
                new IngredientRequirement
                {
                    ingredientName = "Fire Talon",
                    quantity = 1,
                    isDiscovered = true
                },
                new IngredientRequirement
                {
                    ingredientName = "Frozen Dew",
                    quantity = 1,
                    isDiscovered = true
                }
            });
            serpentsDewRecipeEntry.discoveredInfusions.AddRange(new string[] { "Poison Resistance", "Enhanced Agility", "Thermal Balance" });
            serpentsDewRecipeEntry.minAlchemyLevel = 5;
            serpentsDewRecipeEntry.successRate = 75f;
            serpentsDewRecipeEntry.brewingTime = 180;

            AssetDatabase.CreateAsset(serpentsDewRecipeEntry, "Assets/Resources/AlchemyBook/Recipes/SerpentsDewRecipeEntry.asset");

            // Fire Potion Recipe Entry
            var firePotionEntry = ScriptableObject.CreateInstance<RecipeEntry>();
            firePotionEntry.title = "Basic Fire Potion";
            firePotionEntry.description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore. A fundamental recipe for creating fire-based potions. Perfect for beginners learning the art of alchemy, this recipe teaches the basics of combining fire-aligned ingredients.";
            firePotionEntry.isSeen = true;
            firePotionEntry.recipeType = RecipeType.CustomPotion;
            firePotionEntry.isUniquePotionRecipe = false;
            firePotionEntry.requiredIngredients.AddRange(new IngredientRequirement[]
            {
                new IngredientRequirement
                {
                    ingredientName = "Fire Claw",
                    quantity = 2,
                    isDiscovered = true
                },
                new IngredientRequirement
                {
                    ingredientName = "Wind Essence",
                    quantity = 1,
                    isDiscovered = true
                }
            });
            firePotionEntry.discoveredInfusions.AddRange(new string[] { "Fire Damage", "Heat Generation" });
            firePotionEntry.minAlchemyLevel = 1;
            firePotionEntry.successRate = 95f;
            firePotionEntry.brewingTime = 60;

            AssetDatabase.CreateAsset(firePotionEntry, "Assets/Resources/AlchemyBook/Recipes/FirePotionEntry.asset");

            // Earth Shield Potion Entry
            var earthShieldEntry = ScriptableObject.CreateInstance<RecipeEntry>();
            earthShieldEntry.title = "Earth Shield Elixir";
            earthShieldEntry.description = "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium. A protective potion that harnesses the power of earth magic to create a defensive barrier around the drinker. The combination of Earth Shard and Shadow Herb creates a unique protective enchantment.";
            earthShieldEntry.isSeen = true;
            earthShieldEntry.recipeType = RecipeType.UniquePotion;
            earthShieldEntry.isUniquePotionRecipe = true;
            earthShieldEntry.requiredIngredients.AddRange(new IngredientRequirement[]
            {
                new IngredientRequirement
                {
                    ingredientName = "Earth Shard",
                    quantity = 2,
                    isDiscovered = true
                },
                new IngredientRequirement
                {
                    ingredientName = "Shadow Herb",
                    quantity = 1,
                    isDiscovered = true
                }
            });
            earthShieldEntry.discoveredInfusions.AddRange(new string[] { "Stone Skin", "Damage Resistance", "Stability" });
            earthShieldEntry.minAlchemyLevel = 3;
            earthShieldEntry.successRate = 80f;
            earthShieldEntry.brewingTime = 120;

            AssetDatabase.CreateAsset(earthShieldEntry, "Assets/Resources/AlchemyBook/Recipes/EarthShieldEntry.asset");

            // Unknown Recipe Entry (for mystery/exploration)
            var unknownRecipeEntry = ScriptableObject.CreateInstance<RecipeEntry>();
            unknownRecipeEntry.title = "Mysterious Ancient Recipe";
            unknownRecipeEntry.description = "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt. Ancient runes describe a powerful potion, but the exact ingredients remain unclear. Scholars believe it requires rare ingredients from different elemental sources.";
            unknownRecipeEntry.isSeen = true;
            unknownRecipeEntry.recipeType = RecipeType.UniquePotion;
            unknownRecipeEntry.isUniquePotionRecipe = true;
            unknownRecipeEntry.requiredIngredients.AddRange(new IngredientRequirement[]
            {
                new IngredientRequirement
                {
                    ingredientName = "Unknown Ingredient",
                    quantity = 1,
                    isDiscovered = false
                },
                new IngredientRequirement
                {
                    ingredientName = "Rare Catalyst",
                    quantity = 1,
                    isDiscovered = false
                },
                new IngredientRequirement
                {
                    ingredientName = "Elemental Core",
                    quantity = 1,
                    isDiscovered = false
                }
            });
            unknownRecipeEntry.minAlchemyLevel = 10;
            unknownRecipeEntry.successRate = 25f;
            unknownRecipeEntry.brewingTime = 300;

            AssetDatabase.CreateAsset(unknownRecipeEntry, "Assets/Resources/AlchemyBook/Recipes/MysteriousRecipeEntry.asset");
        }

        private static void CreateDemoBestiaryEntries()
        {
            // Fire Fox Entry
            var fireFoxEntry = ScriptableObject.CreateInstance<BestiaryEntry>();
            fireFoxEntry.title = "Fire Fox";
            fireFoxEntry.description = "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt. A magical fox whose fur burns with eternal flames. These cunning creatures are known for their intelligence and their ability to manipulate fire magic. Despite their fearsome appearance, they are generally peaceful unless threatened.";
            fireFoxEntry.isSeen = true;
            fireFoxEntry.creatureType = CreatureType.Beast;
            fireFoxEntry.habitat = "Volcanic forests and warm grasslands";
            fireFoxEntry.behavior = "Lorem ipsum dolor sit amet, generally peaceful and curious. Fire Foxes are intelligent creatures that often approach travelers out of curiosity. They communicate through melodic yips and can understand basic human speech. When threatened, they create walls of fire to escape rather than fight.";
            fireFoxEntry.dangerLevel = DangerLevel.Low;
            fireFoxEntry.weaknesses = new string[] { "Water Magic", "Ice Attacks", "Cold Environments" };
            fireFoxEntry.resistances = new string[] { "Fire Magic", "Heat Damage", "Burn Effects" };
            fireFoxEntry.droppedIngredients = new string[] { "Fire Fox Fur", "Flame Essence", "Magic Spark" };
            fireFoxEntry.hasBeenEncountered = true;
            fireFoxEntry.hasBeenDefeated = false;
            fireFoxEntry.encounterCount = 3;

            AssetDatabase.CreateAsset(fireFoxEntry, "Assets/Resources/AlchemyBook/Bestiary/FireFoxEntry.asset");

            // Shadow Sprite Entry
            var shadowSpriteEntry = ScriptableObject.CreateInstance<BestiaryEntry>();
            shadowSpriteEntry.title = "Shadow Sprite";
            shadowSpriteEntry.description = "Mollit anim id est laborum. Sed ut perspiciatis unde omnis. Small ethereal beings that exist partially in the shadow realm. These mischievous creatures delight in playing pranks on travelers but are generally harmless. They are drawn to dark magic and shadow-infused areas.";
            shadowSpriteEntry.isSeen = true;
            shadowSpriteEntry.creatureType = CreatureType.Fey;
            shadowSpriteEntry.habitat = "Dark forests, caves, and shadowy areas";
            shadowSpriteEntry.behavior = "Playful and mischievous, but not malicious. Shadow Sprites enjoy hiding objects and creating illusions to confuse travelers. They are highly curious about alchemical work and may steal ingredients to examine them.";
            shadowSpriteEntry.dangerLevel = DangerLevel.Harmless;
            shadowSpriteEntry.weaknesses = new string[] { "Bright Light", "Holy Magic", "Loud Noises" };
            shadowSpriteEntry.resistances = new string[] { "Shadow Magic", "Illusion Effects", "Dark Energy" };
            shadowSpriteEntry.droppedIngredients = new string[] { "Shadow Essence", "Sprite Dust", "Whisper Stone" };
            shadowSpriteEntry.hasBeenEncountered = true;
            shadowSpriteEntry.hasBeenDefeated = false;
            shadowSpriteEntry.encounterCount = 7;

            AssetDatabase.CreateAsset(shadowSpriteEntry, "Assets/Resources/AlchemyBook/Bestiary/ShadowSpriteEntry.asset");
        }

        private static void CreateDemoHelpEntries()
        {
            // Getting Started Entry
            var gettingStartedEntry = ScriptableObject.CreateInstance<HelpEntry>();
            gettingStartedEntry.title = "Getting Started with Alchemy";
            gettingStartedEntry.description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. This guide will help you take your first steps into the world of alchemical brewing. Learn the basics of ingredient preparation, potion crafting, and the fundamental principles that govern all alchemical practices.";
            gettingStartedEntry.category = HelpCategory.GettingStarted;
            gettingStartedEntry.isBasicTutorial = true;
            gettingStartedEntry.steps = new string[]
            {
                "Gather basic ingredients from the surrounding area",
                "Learn to identify ingredient aspects and properties",
                "Practice basic grinding and preparation techniques",
                "Attempt your first simple potion recipe",
                "Document your results in the alchemy book"
            };
            gettingStartedEntry.tips = new string[]
            {
                "Start with common ingredients before attempting rare ones",
                "Always prepare ingredients properly before brewing",
                "Keep detailed notes of successful and failed attempts",
                "Practice the grid minigame to improve your brewing skills"
            };
            gettingStartedEntry.relatedTopics = new string[]
            {
                "Ingredient Harvesting",
                "Recipe Crafting",
                "Grid Minigame Basics"
            };

            AssetDatabase.CreateAsset(gettingStartedEntry, "Assets/Resources/AlchemyBook/Help/GettingStartedEntry.asset");

            // Grid Minigame Entry
            var gridGameEntry = ScriptableObject.CreateInstance<HelpEntry>();
            gridGameEntry.title = "Mastering the Grid Minigame";
            gridGameEntry.description = "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. The grid minigame is the heart of potion brewing. Understanding how to efficiently place ingredients and manage your brewing space is crucial for creating powerful potions.";
            gridGameEntry.category = HelpCategory.GridMinigame;
            gridGameEntry.isBasicTutorial = false;
            gridGameEntry.steps = new string[]
            {
                "Study the ingredient shapes before starting",
                "Plan your placement strategy in advance",
                "Consider ingredient interactions and synergies",
                "Use rotation and flipping to optimize space usage",
                "Watch for special ingredient effects during placement"
            };
            gridGameEntry.tips = new string[]
            {
                "Lorem ipsum: larger ingredients often provide better effects",
                "Corner pieces are valuable for maximizing space",
                "Some ingredients work better when placed adjacent to specific others",
                "Don't rush - planning saves time in the long run"
            };
            gridGameEntry.relatedTopics = new string[]
            {
                "Advanced Brewing Techniques",
                "Ingredient Synergies",
                "Potion Optimization"
            };

            AssetDatabase.CreateAsset(gridGameEntry, "Assets/Resources/AlchemyBook/Help/GridMinigameEntry.asset");
        }
    }
}