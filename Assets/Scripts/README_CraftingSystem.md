# Advanced 5x5 Crafting UI System

This system implements a comprehensive crafting interface with a 5x5 logical grid, expansion mechanics, and UI Toolkit integration as specified in your requirements.

## 🎯 Core Features Implemented

### 1. **5x5 Logical Grid System**
- **GridMinigameManager.cs**: Main controller with 5x5 logical grid
- **Free Crafting Mode**: Shows center 3x3, outer ring locked until expanded
- **Recipe Mode**: Shows full 5x5 grid
- **Expansion System**: Ingredients can unlock outer ring cells via `expansionOffsets`

### 2. **Enhanced IngredientShapeData**
- **Occupied Offsets**: Multi-cell ingredient shapes (Tetris-like)
- **Expansion Offsets**: Cells that become available when ingredient is placed adjacent
- **Rotation Support**: Both occupied and expansion offsets can rotate
- **5x5 Visual Editor**: Built into AlchemySystemEditor for shape design

### 3. **UI Toolkit Integration**
- **CraftingUI.uxml**: Complete UI structure with ornate styling
- **CraftingUI.uss**: Professional styling matching 1920x1080 target
- **Responsive Design**: Scales appropriately with different resolutions

### 4. **Advanced Editor Tools**
- **AlchemySystemEditor**: Enhanced with 5x5 grid editor
- **Visual Shape Designer**: Left/right click for occupied/expansion editing
- **Rotation Preview**: Shows ingredient at all 4 rotations
- **Export/Import**: JSON-based shape data exchange

## 🚀 Quick Setup Guide

### Step 1: Scene Setup
1. Create a new scene or open existing scene
2. Add a GameObject with `UIDocument` component
3. Assign `CraftingUI.uxml` to the UIDocument
4. Add StyleSheet reference to `CraftingUI.uss`

### Step 2: Grid Manager Setup
1. Add `GridMinigameManager` script to a GameObject
2. Assign the UIDocument reference
3. Configure recipe mode (false for free crafting, true for recipe mode)

### Step 3: UI Controller Setup
1. Add `CraftingUIController` script to a GameObject
2. Assign UIDocument and GridManager references
3. Populate `availableIngredients` list with your ingredients

### Step 4: Ingredient Design
1. Open **Tools > Alchemy System Editor**
2. Go to **Ingredients** tab
3. Select an ingredient and switch to **Alchemy** tab
4. Use the **5x5 Grid Editor** to design shapes:
   - **Left Click**: Toggle occupied cells (green)
   - **Right Click**: Toggle expansion cells (yellow)
   - **Buttons**: Rotate, clear, reset to 1x1

## 📐 Shape Design Examples

### Small Crystal (Expands Upward)
```
occupiedOffsets: [(0,0)]
expansionOffsets: [(0,2)]
```

### Large Staff (2x2 + Right Expansion)
```
occupiedOffsets: [(0,0), (1,0), (0,1), (1,1)]
expansionOffsets: [(2,0)]
```

### Cross Shape (Unlocks All Adjacent)
```
occupiedOffsets: [(0,0), (-1,0), (1,0), (0,-1), (0,1)]
expansionOffsets: [(-2,0), (2,0), (0,-2), (0,2)]
```

## 🎨 Visual States & CSS Classes

### Grid Tile States
- `.grid-tile`: Base grid cell styling
- `.outer-ring-locked`: Dimmed outer ring cells
- `.outer-ring-unlocked`: Available outer ring cells
- `.expansion-highlight`: Yellow highlight for potential expansions
- `.placement-preview-valid`: Green preview for valid placement
- `.placement-preview-invalid`: Red preview for blocked placement

### Rarity Styling
- `.rarity-common`: Gray border
- `.rarity-uncommon`: Green border with glow
- `.rarity-rare`: Blue border with glow
- `.rarity-epic`: Purple border with glow
- `.rarity-legendary`: Orange border with glow
- `.rarity-mythic`: Gold border with glow

## 🔧 Advanced Configuration

### Expansion Rules
The system validates outer ring placement using adjacency rules:

```csharp
// An outer ring cell can only be used if:
// 1. An adjacent ingredient has expansionOffsets covering that cell
// 2. The relative position matches the expansion offset
bool IsOuterRingAllowed(Vector2Int target)
{
    foreach (var neighbor in GetNeighbors(target))
    {
        var instance = placed[neighbor.x, neighbor.y];
        if (instance != null)
        {
            Vector2Int relative = target - instance.anchor;
            if (instance.shapeData.expansionOffsets.Contains(relative))
                return true;
        }
    }
    return false;
}
```

### Custom Styling
Modify `CraftingUI.uss` to customize:
- **Colors**: Change aspect-based colors and expansion highlights
- **Sizes**: Adjust grid tile dimensions and spacing
- **Effects**: Modify hover states and transitions
- **Layout**: Responsive breakpoints and container sizing

## 🎮 Interaction Flow

1. **Ingredient Selection**: Click ingredient slots to select
2. **Grid Hover**: Shows placement preview and expansion highlights
3. **Placement**: Click grid to place (if valid position)
4. **Expansion**: Adjacent placements unlock outer ring cells
5. **Crafting**: Craft button appears when exactly 3 ingredients placed

## 🐛 Testing & Debug

### Debug Features
- **Console Logging**: Detailed placement and expansion logs
- **Visual Indicators**: Color-coded cell states
- **Validation Messages**: Clear error reasons for failed placements

### Test Cases
1. Place single ingredient in center - should work
2. Try outer ring without expansion - should fail
3. Place expanding ingredient - outer cells should unlock
4. Place in unlocked outer cell - should work
5. Rotate ingredients - offsets should rotate correctly

## 🔮 Extension Points

### Adding New Features
- **Custom Obstacles**: Extend grid with obstacle system
- **Animation**: Add smooth transitions for placements
- **Sound Effects**: Hook into placement/expansion events
- **Particle Effects**: Visual feedback for successful placements
- **Recipe Validation**: Check if current ingredients match known recipes

### Performance Optimization
- **Object Pooling**: Reuse UI elements for large ingredient lists
- **Culling**: Only update visible grid tiles
- **Caching**: Store filtered ingredient lists
- **Batching**: Group UI updates for better performance

## 📚 API Reference

### Key Classes
- **`GridMinigameManager`**: Main grid logic and validation
- **`IngredientShapeData`**: Shape and expansion data
- **`CraftingUIController`**: UI management and interaction
- **`IngredientInstance`**: Runtime ingredient with placement data

### Key Methods
- **`CanPlace(shape, anchor)`**: Validates placement
- **`PlaceIngredient(instance, anchor)`**: Places ingredient
- **`UpdateUnlockedCells()`**: Recalculates available cells
- **`ToggleRecipeMode(bool)`**: Switches between free/recipe modes

This system provides a solid foundation for complex crafting mechanics while maintaining clean separation between logic, data, and presentation layers.