# Tetris-like Alchemy System Setup Guide

## Prerequisites

- Unity 6000.0 (Unity 6) or newer
- UI Toolkit package (included by default)
- Universal Render Pipeline (recommended)

## Step 1: Core Component Setup

### 1.1 Create Grid Controller GameObject

1. Create an empty GameObject named "TetrisAlchemyController"
2. Add the `GridMinigameController` component
3. Add a `UIDocument` component
4. Configure the UIDocument with your UXML file

### 1.2 UXML Layout Structure

Create a UXML file with this basic structure:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="MainContainer" class="main-container">
        
        <!-- Recipe Selection Area -->
        <ui:VisualElement name="RecipeArea" class="recipe-area">
            <ui:Label text="Select Recipe" name="RecipeTitle" class="section-title" />
            <ui:ScrollView name="RecipeSelection" class="recipe-selection" />
        </ui:VisualElement>
        
        <!-- Grid and Palette Container -->
        <ui:VisualElement name="CraftingArea" class="crafting-area">
            
            <!-- Ingredient Palette -->
            <ui:VisualElement name="PaletteArea" class="palette-area">
                <ui:Label text="Ingredients" name="PaletteTitle" class="section-title" />
                <ui:ScrollView name="IngredientPalette" class="ingredient-palette" />
            </ui:VisualElement>
            
            <!-- Grid Container -->
            <ui:VisualElement name="GridArea" class="grid-area">
                <ui:Label text="Crafting Grid" name="GridTitle" class="section-title" />
                <ui:VisualElement name="GridContainer" class="grid-container" />
                
                <!-- Status Display -->
                <ui:VisualElement name="StatusArea" class="status-area">
                    <ui:Label name="GridStatusLabel" class="status-label" />
                    <ui:Label name="PatternMatchLabel" class="status-label" />
                </ui:VisualElement>
            </ui:VisualElement>
            
        </ui:VisualElement>
        
        <!-- Control Buttons -->
        <ui:VisualElement name="ButtonArea" class="button-area">
            <ui:Button name="ClearButton" text="Clear Grid" class="clear-button" />
            <ui:Button name="CraftButton" text="Craft" class="craft-button" />
            <ui:Button name="BackButton" text="Back" class="back-button" />
        </ui:VisualElement>
        
    </ui:VisualElement>
</ui:UXML>
```

### 1.3 Apply Stylesheet

1. Assign the `TetrisGridStyles.uss` file to your UIDocument
2. Ensure all class names match between UXML and USS

## Step 2: ScriptableObject Configuration

### 2.1 Create Ingredients

1. Right-click in Project window
2. Create → Items → Ingredient
3. Configure grid size properties:
   - `GridWidth`: How many cells wide (1-3)
   - `GridHeight`: How many cells tall (1-3)
   - `UnlocksAdditionalSpace`: Can this ingredient expand the grid?
   - `AdditionalSpaceCount`: How many cells to add (if expansion)

### 2.2 Create Recipes

1. Right-click in Project window
2. Create → AlchemyRecipes → Generic Recipe
3. Configure Tetris properties:
   - `IsKeyRecipe`: Mark as story-critical (cannot fail)
   - `MinimumEfficiency`: Required grid utilization (0.0-1.0)
   - `AllowsIngredientInteractions`: Enable synergy bonuses

### 2.3 Create Synthetic Template

1. Create → Items → Synthetic Ingredient
2. This serves as the template for failed crafting attempts
3. Set base properties for procedurally generated synthetics

## Step 3: Skill System Integration

### 3.1 Define Skills

Create these skill IDs in your skill system:

- `"enhanced_grid"`: Increases base grid size to 4x4
- `"overlap_placement"`: Allows ingredient stacking
- `"ingredient_refund"`: Chance to retain ingredients after crafting
- `"auto_craft"`: Automatic success for previously completed recipes

### 3.2 Skill Unlock Conditions

```csharp
// Example skill unlock logic
public void OnCraftingCompleted(AlchemyRecipe recipe, bool success, float efficiency)
{
    if (success && efficiency >= 0.9f)
    {
        // S-rank achievement
        string skillToUnlock = DetermineSkillToUnlock(recipe);
        AlchemySkillSystem.Instance.UnlockSkill(skillToUnlock);
    }
}
```

## Step 4: Event System Wiring

### 4.1 Set Up Event Handlers

```csharp
public class AlchemyMenuManager : MonoBehaviour
{
    [SerializeField] private GridMinigameController gridController;
    [SerializeField] private InventorySystem inventorySystem;
    
    private void Start()
    {
        // Wire up events
        gridController.OnCraftingCompleted += HandleCraftingCompleted;
        gridController.OnSyntheticIngredientCreated += HandleSyntheticCreated;
    }
    
    private void HandleCraftingCompleted(AlchemyRecipe recipe, 
        Dictionary<Vector2Int, PlacedIngredient> placement, bool success)
    {
        if (success)
        {
            // Add result to inventory
            inventorySystem.AddItem(recipe.OutputPotion, 1);
            
            // Remove consumed ingredients
            ConsumeUsedIngredients(placement);
            
            // Track for skill progression
            TrackCraftingSuccess(recipe, CalculateEfficiency(placement));
        }
    }
    
    private void HandleSyntheticCreated(SyntheticIngredient synthetic)
    {
        // Add synthetic to inventory
        inventorySystem.AddItem(synthetic, 1);
        
        // Show discovery notification
        ShowDiscoveryNotification(synthetic);
    }
}
```

## Step 5: Ingredient Interaction Setup

### 5.1 Define Known Interactions

```csharp
// In your game setup or data initialization
private void SetupIngredientInteractions()
{
    var gridController = FindObjectOfType<GridMinigameController>();
    
    // Fire + Ice = Steam effect
    gridController.AddIngredientInteraction(
        fireIngredient, 
        iceIngredient, 
        1.5f, 
        "Creates steam effect - enhanced potion potency"
    );
    
    // Divine + Arc = Lightning enhancement
    gridController.AddIngredientInteraction(
        divineIngredient, 
        arcIngredient, 
        2.0f, 
        "Divine lightning - dramatically increased effects"
    );
    
    // Add more interactions based on your game's alchemy system
}
```

### 5.2 Dynamic Interaction Discovery

```csharp
// Players can discover new interactions through experimentation
private void OnInteractionDiscovered(Ingredient ingredient1, Ingredient ingredient2)
{
    // Save to persistent data
    SaveDiscoveredInteraction(ingredient1, ingredient2);
    
    // Show discovery notification
    ShowInteractionDiscovery(ingredient1, ingredient2);
    
    // Add to controller for future use
    gridController.AddIngredientInteraction(ingredient1, ingredient2, 1.3f, "Player discovered");
}
```

## Step 6: Balance Configuration

### 6.1 Grid Size Progression

- **Novice**: 3x3 base grid
- **Apprentice**: 4x4 with enhanced_grid skill
- **Expert**: Up to 5x5 with expansion ingredients
- **Master**: Unlimited expansion with multiple expansion sources

### 6.2 Success Criteria Tuning

```csharp
// Recommended efficiency thresholds
public static class EfficiencyThresholds
{
    public const float NOVICE_RECIPE = 0.4f;      // 40% grid utilization
    public const float STANDARD_RECIPE = 0.6f;    // 60% grid utilization  
    public const float EXPERT_RECIPE = 0.8f;      // 80% grid utilization
    public const float MASTER_RECIPE = 0.9f;      // 90% grid utilization
}
```

### 6.3 Overlap Limitations

- Maximum 3 overlapping tiles per ingredient
- Overlapping reduces success chance by 10% per overlap
- Skill bonuses can offset overlap penalties

## Step 7: Visual Polish

### 7.1 Animation Setup

Configure these animation triggers:
- Placement preview animations
- Success/failure feedback
- Interaction discovery effects
- Grid expansion transitions

### 7.2 Audio Integration

Add audio cues for:
- Ingredient placement
- Interaction discoveries
- Crafting success/failure
- Grid expansion sounds

## Step 8: Testing and Debugging

### 8.1 Use the Demo System

1. Add `TetrisAlchemyDemo` component to a GameObject
2. Assign test ingredients and recipes
3. Use keyboard shortcuts 1-7 to test features
4. Enable auto-demo for automated testing

### 8.2 Debug Console Commands

Add these for testing:

```csharp
[ContextMenu("Test Grid Expansion")]
private void TestGridExpansion()
{
    // Test expansion ingredient placement
}

[ContextMenu("Test Ingredient Interactions")]  
private void TestInteractions()
{
    // Test all known interactions
}

[ContextMenu("Test Synthetic Creation")]
private void TestSynthetics()
{
    // Test conflict resolution
}
```

## Step 9: Performance Optimization

### 9.1 Object Pooling

- Pool VisualElements for grid cells
- Reuse ingredient preview elements
- Cache color calculations

### 9.2 Update Optimization

- Only update UI when placement changes
- Batch interaction checks
- Lazy load ingredient data

## Step 10: Save System Integration

### 10.1 Persistent Data

Save these data types:
- Discovered ingredient interactions
- Unlocked alchemy skills
- Recipe completion history
- Synthetic ingredient collection

### 10.2 Session Data

Track during gameplay:
- Current grid state
- Selected recipe
- Available ingredients
- Skill bonuses active

## Troubleshooting

### Common Issues

1. **Grid not displaying**: Check UIDocument assignment and UXML structure
2. **Ingredients not placing**: Verify grid size calculations and bounds checking  
3. **Interactions not triggering**: Ensure interaction definitions are loaded correctly
4. **Styles not applying**: Check USS file assignment and class name matching

### Debug Tools

- Enable debug logging in GridMinigameController
- Use Unity Profiler for performance analysis
- Test with various ingredient combinations
- Validate ScriptableObject configurations

## Advanced Features

### Custom Grid Shapes

Extend the system to support non-rectangular grids:
- Circular alchemical circles
- Hexagonal patterns
- Asymmetric layouts

### Time-Based Mechanics

Add temporal elements:
- Ingredient decay over time
- Timed placement challenges
- Catalyst activation delays

### Multiplayer Support

Extend for cooperative crafting:
- Shared grid spaces
- Collaborative recipes
- Competitive efficiency challenges

This setup guide should provide everything needed to implement the Tetris-like alchemy system in your Unity project. Adjust the configuration values based on your specific game balance requirements.