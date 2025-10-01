# Potion System Refactoring Guide

## Overview
This document outlines the refactoring of the potion system from using individual `PotionEffect` ScriptableObjects to the new unified `IEffect` system. This change provides better flexibility, performance, and easier integration with combat systems.

## Changes Made

### 1. Updated Potion Class (`/Assets/Scripts/ScriptableObjects/Items/Potion.cs`)

#### New Fields
- **EffectBundle**: New field to store effects using the modern `IEffect` system
- **Backward Compatibility**: Maintained existing `PotionEffects` field for legacy support

#### New Methods
- `InitializeFromCraftingWithEffects()`: Initialize potion with new effect system
- `AddEffect()`: Add individual `IEffect` to the potion
- `MigrateToNewEffectSystem()`: Convert old effects to new format
- `HasEffectOfType<T>()`: Check if potion contains specific effect type
- `UpdatePropertiesFromNewEffects()`: Update potion properties based on new effects

#### Migration Support
- Context menu option to migrate effects in editor
- Automatic migration detection in `OnValidate()`
- Preservation of old effects during migration for safety

### 2. Enhanced Effect Classes

All effect classes now have proper `Apply()` method implementations:

#### Core Effects
- **HealEffect**: Applies healing to targets
- **DamageEffect**: Applies damage with aspect and stagger
- **ShieldEffect**: Applies shield protection
- **BuffStatEffect**: Applies positive stat modifications
- **DebuffStatEffect**: Applies negative stat modifications
- **BuffHealEffect**: Applies heal over time buffs
- **DebuffDOTEffect**: Applies damage over time debuffs
- **BuffShieldEffect**: Applies shield buffs over time

### 3. New Utility Classes

#### PotionEffectApplicator (`/Assets/Scripts/Utilities/PotionEffectApplicator.cs`)
- **Purpose**: Handle applying potion effects to targets
- **Key Methods**:
  - `ApplyPotionEffects()`: Apply all effects from a potion
  - `ConsumePotionSelf()`: Self-consume a potion
  - `CanUsePotionInContext()`: Check if potion can be used in context

#### PotionMigrationUtility (`/Assets/Scripts/Utilities/PotionMigrationUtility.cs`)
- **Purpose**: Migrate existing potions to new system
- **Key Features**:
  - Batch migration of all potions in project
  - Migration status validation
  - Safe old effect cleanup
- **Editor Tools**:
  - "Migrate All Potions" menu item
  - "Validate Migrations" menu item
  - "Clear Old Effects" menu item (use with caution!)

### 4. Example Usage (`/Assets/Scripts/Examples/PotionConsumerExample.cs`)
Demonstrates how to:
- Consume potions using new system
- Migrate individual potions
- Analyze potion properties
- Use potions on different targets

## Migration Process

### Automatic Migration
1. **Detection**: Potions with old effects but no new effects are automatically detected
2. **Warning**: `OnValidate()` logs migration suggestions
3. **Context Menu**: Right-click potion → "Migrate to New Effect System"

### Manual Migration
```csharp
// Get migration status
var status = PotionMigrationUtility.GetMigrationStatus(potion);

// Migrate single potion
bool success = PotionMigrationUtility.MigratePotion(potion, preserveOldEffects: true);

// Migrate all potions (Editor only)
PotionMigrationUtility.MigrateAllPotions();
```

### Batch Migration (Editor)
1. Open "Tools" → "Potion Migration" → "Migrate All Potions"
2. System will process all potions in the project
3. Review results in console
4. Use "Validate Migrations" to check completion

## Effect Type Mapping

| Old ItemPotionType | New IEffect Type |
|-------------------|------------------|
| Healing           | HealEffect       |
| Buffing           | BuffStatEffect   |
| Debuffing         | DebuffStatEffect |
| Utility           | ShieldEffect     |

## Usage Examples

### Basic Potion Consumption
```csharp
// Apply potion to self
PotionEffectApplicator.ConsumePotionSelf(potion, combatController);

// Apply to specific target
PotionEffectApplicator.ApplyPotionToTarget(potion, source, target);

// Apply to multiple targets
PotionEffectApplicator.ApplyPotionEffects(potion, source, targetList);
```

### Creating Potions with New Effects
```csharp
// Create effect bundle
var effectBundle = new EffectBundle();
effectBundle.Effects.Add(new HealEffect { BaseHeal = 50 });
effectBundle.Effects.Add(new BuffStatEffect { /* stat modifiers */ });

// Initialize potion with new effects
potion.InitializeFromCraftingWithEffects(rarity, tone, bottle, effectBundle, 
    quality, crafter, ingredients);
```

### Adding Effects to Existing Potions
```csharp
// Add individual effects
potion.AddEffect(new HealEffect { BaseHeal = 25 });
potion.AddEffect(new ShieldEffect { BaseShield = 15 });

// Check for specific effect types
bool hasHealing = potion.HasEffectOfType<HealEffect>();
```

## Benefits of New System

### Performance
- **Reduced Memory**: No need for separate ScriptableObject instances
- **Better Serialization**: Effects stored directly in potion data
- **Faster Processing**: Direct method calls instead of ScriptableObject lookups

### Flexibility
- **Type Safety**: Generic effect type checking
- **Extensibility**: Easy to add new effect types
- **Composition**: Mix and match effects freely

### Integration
- **Combat System**: Direct integration with `CombatController`
- **Scaling**: Built-in quality and scale modifiers
- **Context Awareness**: Usage validation for different game contexts

## Backward Compatibility

The refactoring maintains full backward compatibility:
- **Existing Potions**: Continue to work with old `PotionEffect` system
- **Legacy Support**: Old effects automatically converted when applied
- **Gradual Migration**: Can migrate potions one by one or in batches
- **Safety**: Old effects preserved during migration process

## Best Practices

### For New Potions
1. Use `InitializeFromCraftingWithEffects()` for new potions
2. Add effects using `AddEffect()` method
3. Use `EffectBundle` for complex multi-effect potions

### For Existing Potions
1. Use migration utility to convert existing potions
2. Validate migrations before clearing old effects
3. Keep backups before batch operations

### For Development
1. Test effects using `PotionConsumerExample`
2. Use context menu migration for individual potions
3. Monitor console for migration warnings and status

## Troubleshooting

### Common Issues
1. **Missing Effects**: Ensure `EffectBundle` is initialized
2. **Migration Errors**: Check enum mappings for custom effect types
3. **Compilation Errors**: Verify all enum references use correct values

### Validation Tools
- Use `PotionMigrationUtility.GetMigrationStatus()` to check status
- Use `PotionMigrationUtility.GetEffectSummary()` for detailed analysis
- Use editor tools for project-wide validation

## Future Considerations

### Planned Enhancements
- **Effect Stacking**: Rules for how similar effects combine
- **Duration Management**: Better timing system for temporary effects
- **Visual Effects**: Automatic particle system assignment based on effect types
- **Networking**: Multiplayer synchronization for effect application

### Extensibility Points
- **Custom Effects**: Inherit from `IEffect` for custom behaviors
- **Effect Modifiers**: Add multiplicative or additive effect modifiers
- **Condition Systems**: Add prerequisites for effect application
- **Event System**: Hooks for effect application events