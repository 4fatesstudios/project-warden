# Crafting System Demo Setup Guide

## 🚀 Quick Start (Automated)

1. **Create a new scene**: `File → New Scene` and save as `CraftingMenuDemo.unity`
2. **Add setup script**: Create empty GameObject named "CraftingSystemSetup"
3. **Attach script**: Add `CraftingSystemSetupGuide` component to the GameObject
4. **Run automated setup**: Right-click the script → `Perform Full Setup`
5. **Press Play** and test with number keys 1-4!

## 🛠 Manual Setup (Step by Step)

### Step 1: Scene Structure
Create the following GameObject hierarchy:

```
CraftingMenuDemo (Scene)
├── Main Camera
├── CraftingSystemSetup (GameObject + CraftingSystemSetupGuide)
└── CraftingMenuSystem (Auto-created)
    ├── PotionCraftingUI (GameObject + UIDocument + PotionCraftingController)
    ├── GridMinigameUI (GameObject + UIDocument + GridMinigameController)
    ├── BulkCraftingUI (GameObject + UIDocument + BulkCraftingController)
    ├── RefinementUI (GameObject + RefinementMinigameManager)
    │   ├── RoastingMinigameUI (GameObject + UIDocument + RoastingMinigameController)
    │   ├── DistillationMinigameUI (GameObject + UIDocument + DistillationMinigameController)
    │   └── GrindingMinigameUI (GameObject + UIDocument + GrindingMinigameController)
    └── DemoInventory (GameObject + ItemSlotContainerHolder)
```

### Step 2: Assign UXML Files
For each UIDocument component, assign the corresponding UXML file:

- **PotionCraftingUI**: `Assets/Scripts/UI/CraftingSystem/AlchemySystem/PotionCrafting.uxml`
- **GridMinigameUI**: `Assets/Scripts/UI/CraftingSystem/GridMinigame.uxml`
- **BulkCraftingUI**: `Assets/Scripts/UI/CraftingSystem/AlchemySystem/BulkCrafting.uxml`
- **RoastingMinigameUI**: `Assets/Scripts/UI/CraftingSystem/RefinementSystem/RoastingMinigame.uxml`
- **DistillationMinigameUI**: `Assets/Scripts/UI/CraftingSystem/RefinementSystem/DistillationMinigame.uxml`
- **GrindingMinigameUI**: `Assets/Scripts/UI/CraftingSystem/RefinementSystem/GrindingMinigame.uxml`

### Step 3: Create Demo Ingredients
1. **Add DemoIngredientCreator**: Create GameObject with `DemoIngredientCreator` script
2. **Generate ingredients**: Right-click script → `Create Demo Ingredients`
3. **Verify creation**: Check `Assets/Resources/Demo/Ingredients/` folder

### Step 4: Setup Inventory
1. **Find DemoInventory GameObject**
2. **Add demo ingredients**: Run `CraftingSystemSetupGuide` → `Assign Demo Ingredients`
3. **Verify inventory**: Check that ItemSlotContainerHolder has demo items

### Step 5: Connect References
In the `CraftingMenuDemoSetup` component, assign:
- UI Document references (should auto-assign)
- Demo ingredients array
- Controller references (should auto-assign)

## 🎮 Testing the Demo

### Controls
- **1**: Switch to Potion Crafting
- **2**: Switch to Refinement
- **3**: Switch to Bulk Crafting  
- **4**: Switch to Alchemy Book (placeholder)

### Features to Test

#### Potion Crafting
1. Click ingredient slots (+) to select ingredients
2. Click "Manual Craft" to launch grid minigame
3. Try "Auto Craft" (requires S-rank unlock)
4. Test "Bulk Craft" for mass production

#### Refinement System
1. Select ingredients that can be refined
2. Choose refinement type (Grinding/Distillation/Roasting)
3. Play the animated minigames
4. Collect refined results

#### Grid Minigame
1. Place ingredients on Tetris-style grid
2. Optimize placement for higher potency
3. Aim for S-rank for auto-craft unlock

#### Skill System
1. Achieve S-ranks to unlock skills
2. Auto-craft becomes available after S-rank
3. Bulk crafting for efficient production

## 🔧 Troubleshooting

### Common Issues

**"UIDocument not assigned"**
- Assign UXML files to UIDocument components in inspector
- Check that UXML files exist at specified paths

**"Inventory not found"**
- Ensure ItemSlotContainerHolder is attached to DemoInventory GameObject
- Run "Assign Demo Ingredients" to populate inventory

**"No ingredients available"**
- Run DemoIngredientCreator → "Create Demo Ingredients"
- Check Resources/Demo/Ingredients folder for created assets

**Compilation Errors**
- Wait for Unity to finish compiling after script changes
- ItemIcon property should resolve after Unity recompiles

**UI Elements Not Found**
- Check UXML files are properly assigned
- Verify element names match between UXML and C# code
- Look for typos in element names

### Console Messages
The system provides helpful debug messages:
- ✓ Success messages show working features
- ⚠ Warnings indicate missing but non-critical components  
- ✗ Errors indicate required components that need fixing

## 📁 File Structure

```
Assets/
├── Scripts/
│   ├── Demo/
│   │   ├── CraftingSystemSetupGuide.cs
│   │   └── DemoIngredientCreator.cs
│   ├── Setup/
│   │   └── CraftingMenuDemoSetup.cs
│   ├── PlaceholderClasses/
│   │   └── ItemSlotContainerHolder.cs
│   ├── GameSystems/CraftingMenu/
│   │   ├── AlchemyMenu/
│   │   │   ├── GridMinigameController.cs
│   │   │   ├── PotionCraftingController.cs
│   │   │   └── BulkCraftingController.cs
│   │   └── RefinementMenu/
│   │       ├── RefinementMinigameManager.cs
│   │       ├── RoastingMinigameController.cs
│   │       ├── DistillationMinigameController.cs
│   │       └── GrindingMinigameController.cs
│   ├── UI/CraftingSystem/
│   │   ├── AlchemySystem/
│   │   │   ├── PotionCrafting.uxml
│   │   │   ├── PotionCraftingStyles.uss
│   │   │   ├── BulkCrafting.uxml
│   │   │   └── BulkCraftingStyles.uss
│   │   ├── RefinementSystem/
│   │   │   ├── RoastingMinigame.uxml
│   │   │   ├── RoastingMinigameStyles.uss
│   │   │   ├── DistillationMinigame.uxml
│   │   │   ├── DistillationMinigameStyles.uss
│   │   │   ├── GrindingMinigame.uxml
│   │   │   └── GrindingMinigameStyles.uss
│   │   ├── GridMinigame.uxml
│   │   └── GridMinigameStyles.uss
│   └── Editor/
│       ├── AlchemySystemEditor.cs
│       └── IngredientPropertyDrawer.cs
└── Resources/Demo/Ingredients/
    ├── FireClaw.asset
    ├── FireTalon.asset
    ├── WaterDroplet.asset
    ├── EarthShard.asset
    ├── WindEssence.asset
    └── ShadowHerb.asset
```

## 🎯 What's Working

### ✅ Fully Implemented
- **Ingredient system** with potency, grid sizes, refinement options
- **Grid-based crafting** with Tetris-style placement
- **Animated refinement minigames** (roasting, distillation, grinding)
- **Ranking system** (F through S ranks) with potency multipliers
- **Auto-crafting** unlocked by S-rank achievements
- **Bulk crafting** for mass production
- **Skill progression** framework
- **Designer-friendly editors** for content creation
- **Complete UI system** with modern styling

### 🔧 Placeholder Systems (TODO)
- **Actual inventory integration** (currently using mock system)
- **Recipe database** (framework exists, needs content)
- **Sound effects** (hooks exist, needs audio clips)
- **Particle effects** (simulated with UI, could use real particles)
- **Save/load system** (skills use PlayerPrefs currently)

## 🚀 Next Steps

1. **Test the demo** to ensure everything works
2. **Create more ingredients** using the ingredient editor
3. **Add recipe definitions** using the alchemy system editor
4. **Integrate with your existing inventory** system
5. **Add sound effects** to the minigames
6. **Customize UI styling** to match your game's theme
7. **Expand skill tree** with more progression options

## 💡 Pro Tips

- Use the **Alchemy System Editor** (`Tools → Alchemy System Editor`) for content creation
- **Enhanced ingredient inspector** provides visual grid previews
- **Context menu options** on controllers for quick testing
- **Number keys 1-4** for rapid menu switching during testing
- **Console messages** provide helpful debugging information
- **Auto-setup script** handles most configuration automatically

---

**🎉 You now have a fully functional crafting system demo!**

The system is designed to be modular and extensible. Start with the basic demo, then customize and expand based on your game's specific needs.