# Enhanced Tetris-like Alchemy System

## Overview

This enhanced alchemy system transforms the crafting experience into a strategic Tetris-like grid puzzle where players place ingredients with different shapes and sizes to create potions. The system includes advanced features like ingredient interactions, grid expansion, overlapping mechanics, and failure handling.

## Key Features

### 🧩 Tetris-Style Grid System
- **Dynamic Grid Size**: Base 3x3 grid that can expand through ingredients or skills
- **Ingredient Shapes**: Each ingredient occupies different grid sizes (1x1, 1x2, 2x2, etc.)
- **Strategic Placement**: Players must efficiently arrange ingredients like Tetris blocks
- **Visual Feedback**: Real-time placement preview with valid/invalid indicators

### 🔬 Ingredient Interactions
- **Adjacent Effects**: Ingredients next to each other can interact
- **Synergy Bonuses**: Compatible ingredients create enhanced effects
- **Visual Highlights**: Interactive pairs are highlighted during placement
- **Custom Interactions**: Developers can define specific ingredient combinations

### 🔧 Grid Expansion
- **Special Ingredients**: Some ingredients unlock additional grid space
- **Dynamic Growth**: Grid expands in real-time when expansion ingredients are placed
- **Skill-Based Expansion**: Alchemy skills can also increase grid size
- **Temporary Expansion**: Grid contracts when expansion ingredients are removed

### 🎪 Advanced Placement Mechanics
- **Overlapping Mode**: With skills, ingredients can overlap on the same tiles
- **Limited Overlap**: Maximum 3 overlapping tiles per ingredient for balance
- **Overlap Bonuses**: Overlapping creates unique effect combinations

### 🔑 Story Recipe Protection
- **Key Recipes**: Important story recipes cannot fail
- **Failure Prevention**: System warns players if arrangement "feels off"
- **Locked Positions**: Key recipes can have required ingredient positions
- **Narrative Safety**: Prevents player frustration during critical story moments

### ⚗️ Synthetic Ingredient Creation
- **Failure Recovery**: Failed crafting attempts create synthetic ingredients
- **Aspect Conflicts**: Conflicting ingredients (fire + ice) generate synthetics
- **Stability System**: Synthetics have varying stability ratings
- **Reusability**: Synthetic ingredients can be used in other recipes

### 🎓 Skill Integration
- **Progression Rewards**: Skills unlock new grid capabilities
- **Enhanced Grid**: Increases base grid size
- **Overlap Placement**: Enables ingredient stacking
- **Ingredient Refund**: Chance to retain ingredients after crafting
- **Auto-Craft Improvements**: Better success rates for repeated recipes

## Technical Implementation

### Core Classes

#### `GridMinigameController`
- Main controller for the Tetris-like interface
- Handles ingredient placement, removal, and validation
- Manages grid expansion and contraction
- Processes ingredient interactions

#### `PlacedIngredient`
- Represents an ingredient placed on the grid
- Tracks occupied cells and interaction status
- Handles overlapping mechanics

#### `AlchemyRecipe` (Enhanced)
- Extended with Tetris-specific properties
- Key recipe configuration
- Minimum efficiency requirements
- Interaction allowances

#### `SyntheticIngredient`
- Special ingredient type for failed crafts
- Dynamic generation based on source ingredients
- Stability and effect systems

### Grid System Features

```csharp
// Example: Setting up grid expansion
var expansionIngredient = GetExpansionIngredient();
gridController.SetAvailableIngredients(new[] { expansionIngredient });

// Example: Adding ingredient interactions
gridController.AddIngredientInteraction(
    fireIngredient, 
    iceIngredient, 
    1.5f, 
    "Creates steam effect"
);

// Example: Enabling key recipe mode
gridController.SetRecipeAsKeyRecipe(true);
```

### Skill System Integration

```csharp
// Example: Unlocking advanced features
skillSystem.UnlockSkill("enhanced_grid");    // 4x4 instead of 3x3
skillSystem.UnlockSkill("overlap_placement"); // Allow stacking
skillSystem.UnlockSkill("ingredient_refund"); // Keep ingredients

// Apply skills to grid
gridController.ApplySkillBonuses();
```

## Gameplay Flow

### 1. Recipe Selection
- Player chooses from available recipes
- Grid configures for recipe requirements
- Key recipes enable failure protection

### 2. Ingredient Placement
- Select ingredients from palette
- Place on grid like Tetris blocks
- Visual feedback shows valid positions
- Interactions highlight automatically

### 3. Grid Management
- Expansion ingredients grow available space
- Overlapping requires skill unlock
- Efficient space usage improves outcomes

### 4. Crafting Resolution
- System evaluates pattern efficiency
- Checks for conflicting aspects
- Success creates intended potion
- Failure generates synthetic ingredient

### 5. Skill Progression
- S-rank results unlock auto-crafting
- Multiple S-ranks unlock advanced skills
- Skills permanently improve capabilities

## Balance Considerations

### Success Factors
- **Required Ingredients**: Must include recipe components
- **Space Efficiency**: Minimum 60% grid utilization
- **Aspect Compatibility**: No major conflicts (fire + ice)
- **Interaction Bonuses**: Adjacent compatible ingredients

### Failure Prevention
- **Key Recipe Protection**: Story-critical recipes cannot fail
- **Warning System**: Players warned before problematic attempts
- **Synthetic Creation**: Failures still produce useful materials
- **Skill Bonuses**: Progression makes success easier

## Visual Design

### Grid Appearance
- **Empty Cells**: Light grid pattern
- **Filled Cells**: Colored by ingredient aspect
- **Interaction Highlights**: Glowing borders for synergies
- **Expansion Areas**: Distinct styling for unlocked space
- **Overlap Indicators**: Semi-transparent overlay effects

### Ingredient Palette
- **Tetris Previews**: Mini-grid showing ingredient shape
- **Relevance Sorting**: Recipe ingredients shown first
- **Count Display**: Available quantity per ingredient
- **Selection Highlight**: Clear visual selection state

## Demo System

The `TetrisAlchemyDemo` script provides comprehensive testing:

### Demo Controls
- **1**: Basic Tetris placement
- **2**: Ingredient interactions
- **3**: Grid expansion
- **4**: Overlapping mechanics
- **5**: Key recipe protection
- **6**: Synthetic creation
- **7**: Skill progression
- **R**: Reset demo

### Setup Requirements
- Assign test ingredients with varying grid sizes
- Create recipes with different difficulty levels
- Configure some ingredients as expansion types
- Set up ingredient interaction definitions

## Future Enhancements

### Potential Features
- **Rotation Mechanics**: Rotate ingredients like Tetris pieces
- **Combo System**: Chain reactions for multiple interactions
- **Timed Challenges**: Speed-based crafting modes
- **Grid Modifiers**: Environmental effects on the grid
- **Recipe Variants**: Multiple solutions for single recipes

### Advanced Interactions
- **Catalyst Ingredients**: Special ingredients that enhance others
- **Volatile Components**: Ingredients that change over time
- **Aspect Transmutation**: Converting one aspect to another
- **Grid Memory**: Patterns that persist between sessions

## Conclusion

This enhanced Tetris-like alchemy system transforms traditional crafting into an engaging spatial puzzle. The combination of strategic placement, ingredient synergies, and progression rewards creates depth while maintaining accessibility. The failure handling ensures frustration-free gameplay, while the skill system provides long-term engagement goals.

The system successfully bridges the gap between puzzle mechanics and RPG progression, offering both immediate tactical decisions and strategic planning for character development.