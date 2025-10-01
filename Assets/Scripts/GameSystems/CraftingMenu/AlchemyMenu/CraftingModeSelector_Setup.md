# Crafting Mode Selector Setup Guide

## Overview

The `CraftingModeSelector` provides players with two distinct crafting modes:

1. **🎨 Free Crafting** - Traditional 3x3 grid layout with complete freedom
2. **📜 Recipe Crafting** - Select from available recipes with pre-designed grid patterns and obstacles

## Setup Instructions

### 1. Scene Setup

Create a GameObject in your scene with the following structure:

```
CraftingModeSelector
├── UIDocument (Component)
│   ├── Source Asset: CraftingModeSelector.uxml
│   └── Style Sheet: CraftingModeSelector.uss
└── CraftingModeSelector (Script Component)
```

### 2. UI Document Configuration

1. **UXML File**: Assign `CraftingModeSelector.uxml` to the UIDocument's Source Asset
2. **USS File**: Add `CraftingModeSelector.uss` to the UIDocument's Style Sheets list

### 3. Component Configuration

#### CraftingModeSelector Component Settings:

- **UI Document**: Auto-assigned if on same GameObject
- **Recipe Scroll Item Template**: Optional - Leave null for default styling
- **Enable Debug Logging**: Toggle for development/debugging
- **Free Crafting Grid Size**: Default Vector2Int(3, 3) for traditional crafting

### 4. Navigation Integration

The component automatically integrates with the existing `CraftingNavigationController`. Ensure your navigation controller discovers the "CraftingModeSelector" GameObject.

## How It Works

### Free Crafting Mode

When selected:
1. Configures GridGameManager to 3x3 grid
2. Disables obstacles
3. Launches traditional crafting experience
4. Perfect for experimentation and custom recipes

### Recipe Crafting Mode

When selected:
1. Shows scroll view of available recipes with custom grid patterns
2. Player selects a recipe and sees preview
3. Confirms selection to start crafting
4. GridGameManager loads recipe's saved grid pattern and obstacles
5. Perfect for following established recipes with strategic challenge

## Integration with Existing Systems

### AlchemyRecipeDatabase

- Automatically loads recipes that have `HasCustomGridData() == true`
- Only recipes with saved grid patterns appear in Recipe Crafting mode
- Uses recipe's grid size and obstacle configuration

### GridGameManager

- Free Mode: 3x3 grid, no obstacles, clean slate
- Recipe Mode: Recipe-defined grid size, loads saved obstacle patterns
- Maintains all existing ingredient placement and collision systems

### Navigation Flow

```
Main Menu → Alchemy Menu → CraftingModeSelector → [Free/Recipe] → GridMinigame
```

## Recipe Requirements

For recipes to appear in Recipe Crafting mode, they must:

1. Exist in the `AlchemyRecipeDatabase`
2. Have `HasCustomGridData()` return `true`
3. Contain valid grid information via `GetGridInfo()`

## UI Customization

### Styling

Modify `CraftingModeSelector.uss` to customize:
- Button hover effects
- Recipe scroll item appearance
- Color schemes and animations
- Layout and spacing

### Content

Update `CraftingModeSelector.uxml` to modify:
- Button text and tooltips
- Layout structure
- Additional UI elements

## Debugging

Enable debug logging in the component to see:
- Recipe loading status
- Mode selection events
- Navigation flow
- Grid configuration changes

## Future Enhancements

Potential improvements:
- Recipe difficulty indicators
- Grid pattern previews in recipe list
- Custom recipe creation tools
- Crafting mode preferences saving
- Achievement integration for recipe completion