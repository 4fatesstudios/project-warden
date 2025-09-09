# Grid Game Demo System

A comprehensive grid-based ingredient placement and interaction system for Project Warden's alchemy system.

## 🎯 Overview

This system provides a Tetris-like grid game where players can place ingredients from your existing alchemy system. It includes visual feedback, element interactions, scoring, and a complete UI framework.

## 📁 System Architecture

```
GridDEMO/
├── Core System
│   ├── GridGameManager.cs      # Main controller and game logic
│   ├── GridCell.cs             # Individual cell state and properties
│   ├── GridVisualizer.cs       # Visual rendering and animations
│   └── IngredientPlacer.cs     # Ingredient placement logic
├── Interaction
│   ├── IngredientInteraction.cs # Mouse interaction with placed ingredients
│   └── InputActions.cs         # Unity Input System wrapper
├── UI System
│   ├── GridDemoUI.cs           # Complete UI interface
│   └── GridDemoGameplay.cs     # Game rules and scoring
└── Tools
    └── Editor/
        └── GridDemoSetupTool.cs # Editor tool for quick setup
```

## 🚀 Quick Start

### Method 1: Using the Setup Tool (Recommended)

1. **Open the Setup Tool**: `Tools > Grid Demo > Setup Tool`
2. **Configure Grid**: Set desired grid size (4x4 to 16x16)
3. **Find Ingredients**: Click "Auto-Find All Ingredients" to populate from your project
4. **Create Scene**: Click "Create Grid Demo Scene"
5. **Play**: Hit Play and start placing ingredients!

### Method 2: Manual Setup

1. **Create Empty GameObject**: Name it "GridGameManager"
2. **Add Components**:
   - `GridGameManager`
   - `GridVisualizer`
   - `IngredientPlacer`
   - `GridDemoGameplay` (optional)
3. **Configure GridGameManager**:
   - Set `gridWidth` and `gridHeight` (recommended: 8x8)
   - Set `cellSize` (recommended: 1.0)
   - Populate `availableIngredients` list
4. **Setup Camera**: Position to view the grid from above
5. **Create UI**: Add GridDemoUI component to a Canvas

## 🎮 Features

### Core Gameplay
- **Grid-based placement**: Drag and drop ingredients onto a customizable grid
- **Multi-cell ingredients**: Support for ingredients that occupy multiple grid cells
- **Visual feedback**: Hover highlights, placement validation, and cell animations
- **Element interactions**: Visual and audio feedback for ingredient reactions

### Visual System
- **Aspect-based coloring**: Each element aspect has distinct colors
  - Scorch: Red
  - Frigid: Cyan
  - Arc: Yellow
  - Caustic: Brown
  - Corporeal: Gray
  - Divine: White
- **Intensity effects**: Ingredient potency affects brightness and cell height
- **Particle effects**: Reactions create visual particle effects
- **Grid animations**: Smooth height transitions and hover effects

### Interaction System
- **Mouse controls**:
  - Left-click: Place ingredient / Show detailed info
  - Right-click: Remove ingredient
  - Hover: Show tooltip and placement preview
- **Touch support**: Ready for mobile/tablet implementation

### Scoring & Game Rules
- **Base scoring**: Points for placing ingredients
- **Reaction bonuses**: Extra points for adjacent element reactions
- **Pattern recognition**: Symmetry and cluster bonuses
- **Win conditions**: Configurable completion requirements

## 🔧 Integration with Your Alchemy System

The system seamlessly integrates with your existing alchemy system:

### Ingredient Properties Used
- `ItemName`: Display name
- `ItemDescription`: Tooltip information
- `IngredientAspect`: Color coding and reactions
- `IngredientArchetype`: Gameplay classification
- `Potency`: Visual intensity (1-5)
- `GridWidth` & `GridHeight`: Tetris-like sizing
- `IsCorrupted`: Special visual indication
- `StabilityRating`: Gameplay balance

### Supported Aspects
- **Scorch** ↔ **Frigid**: Temperature reactions
- **Arc** ↔ **Caustic**: Energy reactions  
- **Corporeal** ↔ **Divine**: Spiritual reactions

## 🎨 Customization

### Visual Customization
```csharp
// In GridVisualizer
public Material baseCellMaterial;
public Material highlightMaterial;
public Material occupiedMaterial;
public Color gridLineColor;
```

### Gameplay Customization
```csharp
// In GridDemoGameplay
public int baseScorePerIngredient = 10;
public int aspectMatchBonus = 25;
public float reactionBonus = 1.5f;
```

### Grid Configuration
```csharp
// In GridGameManager
public int gridWidth = 8;
public int gridHeight = 8;
public float cellSize = 1f;
```

## 🎯 Example Reaction System

The system includes a simple but extensible reaction system:

```csharp
// Thermal reactions
Scorch + Frigid = Steam reaction (thermal neutralization)

// Energy reactions  
Arc + Caustic = Explosive reaction (energy discharge)

// Spiritual reactions
Corporeal + Divine = Transcendence reaction (spiritual transformation)
```

## 🛠️ Extending the System

### Adding New Reactions
```csharp
private bool HasReaction(Ingredient ingredient1, Ingredient ingredient2)
{
    // Add your custom reaction logic here
    if (ingredient1.IngredientAspect == Aspect.YourAspect && 
        ingredient2.IngredientAspect == Aspect.AnotherAspect)
    {
        return true;
    }
    return false;
}
```

### Custom Scoring Rules
```csharp
private int CalculatePlacementScore(Ingredient ingredient, Vector2Int position)
{
    int score = baseScorePerIngredient * ingredient.Potency;
    
    // Add your custom scoring logic
    if (IsSpecialPosition(position))
        score *= 2;
        
    return score;
}
```

### UI Extensions
The UI system is modular and can be extended with:
- Custom ingredient filters
- Advanced statistics panels
- Achievement systems
- Multiplayer score comparisons

## 🐛 Troubleshooting

### Common Issues

**Grid not appearing:**
- Check that GridVisualizer is attached to the same GameObject as GridGameManager
- Ensure camera is positioned to view the grid
- Verify grid start position is at world origin or camera view

**Ingredients not placing:**
- Check that ingredients are assigned to the `availableIngredients` list
- Verify ingredient GridWidth and GridHeight are set correctly
- Ensure the target position is within grid bounds

**No visual feedback:**
- Check that materials are assigned in GridVisualizer
- Verify URP renderer is properly configured
- Check that the grid cells have colliders for mouse interaction

### Performance Tips
- For large grids (>12x12), consider object pooling for cell visuals
- Limit particle effects for mobile platforms
- Use LOD system for complex ingredient models

## 📝 Future Enhancements

### Planned Features
- **Save/Load functionality**: Persistent grid layouts
- **Multiplayer support**: Collaborative ingredient placement
- **Advanced animations**: Smooth ingredient transitions
- **Mobile UI**: Touch-optimized interface
- **Achievement system**: Goals and progression tracking
- **Ingredient crafting**: Combine placed ingredients into new ones

### Integration Opportunities
- **Main alchemy system**: Export successful combinations
- **Inventory management**: Direct integration with player inventory
- **Quest system**: Grid puzzles as quest objectives
- **Tutorial system**: Interactive learning experience

## 🎉 Ready to Use!

The Grid Game Demo system is now fully integrated with your Project Warden alchemy system and ready for testing and expansion. Use the Setup Tool to quickly create demo scenes, or manually configure components for custom implementations.

Have fun experimenting with ingredient combinations and creating engaging grid-based gameplay!