# Alchemy System Implementation Guide

## Overview
The Project Warden alchemy system has been completely revamped from a rhythm-based minigame to a Tetris-style grid placement system with ranking, auto-crafting, and skill progression.

## Key Features Implemented

### 1. Grid-Based Crafting System
- **Tetris-style placement**: Ingredients have width/height properties (1x1 to 4x4)
- **Space unlocking**: Special ingredients can expand the crafting grid
- **Interactive placement**: Click and drag ingredients onto a 4x4 base grid
- **Preview system**: Visual feedback shows valid/invalid placements

### 2. Ranking System
- **7-tier ranking**: F, E, D, C, B, A, S (40% to 120% potency)
- **Performance-based**: Calculated from grid efficiency, ingredient potency, and utilization
- **Auto-craft unlock**: S-rank achievement unlocks auto-crafting for that recipe
- **Potency modifiers**: Ranks affect final potion strength

### 3. Ingredient Potency System
- **Potency levels**: 1-5 scale for ingredient strength
- **Ratio importance**: Higher potency ingredients create stronger effects
- **Component inheritance**: AlchemyComponents average base ingredient potency
- **Grid size scaling**: Components inherit larger dimensions from base ingredients

### 4. Auto-Crafting System
- **S-rank requirement**: Must achieve S-rank manually first
- **Guaranteed quality**: Auto-craft produces A-rank (100% potency) by default
- **Skill bonuses**: Chance for S-rank auto-crafts with skill upgrades
- **Ingredient tracking**: Shows S-rank count for each recipe

### 5. Bulk Crafting
- **Mass production**: Craft multiple potions at once
- **Inventory checking**: Validates sufficient ingredients before crafting
- **Unlocked recipes only**: Only recipes with S-rank achievements available
- **Progress tracking**: Shows successful vs failed crafts

### 6. Skill Progression System
- **Recipe tracking**: Individual S-rank counts per recipe/ingredient combination
- **Skill tree hooks**: Infrastructure for future skill unlocks
- **Auto-craft improvements**: Skills can increase S-rank chance on auto-crafts
- **Grid enhancements**: Skills for larger grids, ingredient overlap, etc.

## Technical Implementation

### Core Classes

#### `CraftingGrid`
- Manages grid state and ingredient placement
- Validates placement rules and space availability
- Handles grid expansion from special ingredients
- Calculates efficiency and potency metrics

#### `GridMinigameController`
- UI controller for the grid placement interface
- Handles ingredient selection and placement
- Provides visual feedback and previews
- Calculates final crafting rank based on performance

#### `AlchemySkillSystem`
- Persistent skill and achievement tracking
- Auto-craft unlock management
- Skill tree integration points
- Save/load functionality via PlayerPrefs

#### `CraftingRank` (Enum)
- Defines the 7-tier ranking system
- Extension methods for potency multipliers
- Display name formatting
- Auto-craft unlock checking

### Updated Components

#### `Ingredient` Class Additions
```csharp
public virtual int Potency => potency;           // 1-5 strength level
public virtual int GridWidth => gridWidth;       // 1-4 grid width
public virtual int GridHeight => gridHeight;     // 1-4 grid height
public virtual bool UnlocksAdditionalSpace;      // Can expand grid
public virtual int AdditionalSpaceCount;         // How many cells to add
```

#### `PotionCraftingController` Updates
- Replaced rhythm minigame with grid system
- Added auto-craft and bulk craft buttons
- Integrated skill system for unlock tracking
- Enhanced UI feedback for crafting status

## Usage Instructions

### For Designers
1. **Set ingredient properties**: Configure potency (1-5) and grid size (1x1 to 4x4)
2. **Mark special ingredients**: Set `UnlocksAdditionalSpace` for grid expanders
3. **Create recipes**: Use existing AlchemyRecipe system
4. **Test progression**: Use AlchemySystemSetup component for testing

### For Players
1. **Manual crafting**: Place ingredients on grid for hands-on control
2. **Achieve S-ranks**: Master recipes for 95%+ efficiency/utilization
3. **Unlock auto-craft**: S-rank unlocks automatic crafting at A-rank quality
4. **Bulk production**: Mass-craft unlocked recipes efficiently

### Integration with Existing Systems
- **Inventory system**: Full compatibility with existing ItemSlotContainerHolder
- **Recipe database**: Works with current AlchemyRecipeDatabase
- **Component system**: Enhanced AlchemyComponent with inherited properties
- **Effect system**: Compatible with existing PotionEffect system

## Configuration Examples

### Basic Ingredient Setup
```csharp
Fire Claw:
- Potency: 2
- Grid Size: 1x1
- Unlocks Space: false

Fire Talon:
- Potency: 1
- Grid Size: 1x1
- Unlocks Space: false

Rare Catalyst:
- Potency: 4
- Grid Size: 2x2
- Unlocks Space: true
- Additional Space: 4 cells
```

### Testing Setup
1. Add `AlchemySystemSetup` component to a GameObject
2. Enable `setupOnStart` and `createTestSkillData`
3. Run scene to initialize the skill system
4. Use context menu options to test different states

## Future Enhancements
- Visual ingredient shapes beyond rectangles
- Rotation and mirroring of ingredients
- More complex grid unlocking patterns
- Advanced skill tree with branching paths
- Multiplayer collaborative crafting
- Seasonal ingredient events with special properties

## Troubleshooting
- **No auto-craft options**: Ensure S-rank achievements on recipes
- **Grid not expanding**: Check ingredient `UnlocksAdditionalSpace` setting
- **Skill data lost**: Skills save to PlayerPrefs automatically
- **Compilation errors**: Ensure all virtual properties are properly overridden