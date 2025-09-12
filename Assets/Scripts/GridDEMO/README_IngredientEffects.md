# Ingredient Effects System

## Overview
This system adds effects to ingredients (similar to how potions have EffectBundles) and creates visual particle effects when ingredients with similar or different effects are placed adjacent to each other in the grid.

## Key Components

### 1. Ingredient.cs Updates
- Added `EffectBundle effectBundle` field to store ingredient effects
- Added methods to check for effects and compare with other ingredients:
  - `HasEffects()` - Check if ingredient has any effects
  - `HasSimilarEffectsTo(Ingredient other)` - Check if two ingredients share effect types
  - `GetEffectTypes()` - Get all effect types in this ingredient
  - `HasEffectOfType<T>()` - Check for specific effect type

### 2. IngredientEffectVisualizer.cs (New)
- Handles all visual particle effects for ingredient interactions
- Creates different particle effects based on interaction type:
  - **Sparkle effects** for ingredients with similar effects
  - **Reaction effects** for ingredients with different effects  
  - **Neutral effects** for mixed interactions

### 3. Integration with IngredientPlacer.cs
- Automatically checks for adjacent ingredient interactions when placing new ingredients
- Clears effects when grid is reset
- Uses the existing grid system to find adjacent ingredients

## Particle Effect Types

### Similar Effects (Sparkles)
- **Trigger**: When adjacent ingredients share at least one effect type
- **Visual**: Golden sparkles distributed along the shared border(s) between ingredients
- **Shape**: Box-shaped particle emission oriented along border direction (horizontal/vertical)
- **Intensity**: Increases based on number of shared effect types, distributed across all shared borders
- **Duration**: 3 seconds

### Different Effects (Reactions) 
- **Trigger**: When adjacent ingredients have different effect types
- **Visual**: Burst reactions with contrasting colors along shared border(s)
- **Shape**: Smaller box bursts positioned along the border
- **Distribution**: Multiple bursts along longer borders for better coverage
- **Duration**: 2 seconds (shorter, more dramatic)

### Neutral Interactions
- **Trigger**: When only one ingredient has effects
- **Visual**: Subtle white sparkles at border centers
- **Duration**: 1.5 seconds

## Usage Instructions

### Setting Up Ingredient Effects
1. Select an ingredient asset in the Inspector
2. Expand the "Ingredient Effects" section
3. Add effects to the `effectBundle` using the EffectBundle drawer
4. Effects will automatically be detected when ingredients are placed adjacent to each other

### Testing the System
1. In Play mode, right-click on the GridGameManager component
2. Select "Test Ingredient Effect Interactions" from the context menu
3. This will place two ingredients next to each other and show their interaction effects

### Viewing Current Interactions
1. Right-click on the GridGameManager component in Play mode
2. Select "Refresh All Ingredient Interactions" to update all current effect visualizations

## Technical Details

### Effect Comparison Logic
- Ingredients are considered to have "similar effects" if they share any effect type (class)
- The system uses reflection to compare `IEffect` implementations
- Effect intensity and values don't matter - only the type of effect

### Adjacency Detection
- Uses 4-directional adjacency (up, down, left, right)
- Detects all cells of multi-cell ingredients that are adjacent to other ingredients
- **Border-based Effects**: Particle effects appear along the actual shared borders, not at single midpoints
- Calculates shared border segments between ingredient shapes
- Distributes particle effects along the length of shared borders for realistic visual coverage

### Performance Considerations
- Effects are created dynamically and destroyed automatically
- Active effects are tracked to prevent memory leaks
- Effect checking only occurs when ingredients are placed, not continuously

## Customization

### Particle Effect Prefabs
You can assign custom particle effect prefabs in the IngredientEffectVisualizer component:
- `similarEffectsParticlePrefab` - For similar effects
- `differentEffectsParticlePrefab` - For different effects

### Colors and Duration
Adjust visual properties in the IngredientEffectVisualizer:
- `effectDuration` - How long effects last
- `similarEffectsColor` - Color for sparkle effects
- `differentEffectsColor` - Color for reaction effects
- `effectHeightOffset` - Height above grid for effects

## Integration with Existing Systems

### Potion System
- Potions already have EffectBundles, so this extends the same concept to ingredients
- This creates consistency between ingredients and their resulting potions
- Ingredient effects can influence the final potion's effect bundle

### Grid System
- Fully integrated with the existing grid placement system
- Uses existing adjacency logic and grid coordinate system
- Effects automatically clear when grid is reset

### Visual System
- Works alongside existing GridVisualizer effects
- Does not interfere with ingredient placement highlights
- Particle effects appear above the grid to avoid visual conflicts