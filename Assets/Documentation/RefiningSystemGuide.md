# Ingredient Refining System - Complete Guide

## 📋 Overview

The Ingredient Refining System adds depth to your alchemy mechanics by allowing players to process raw ingredients into more potent refined versions. This system features three distinct refining processes, each suited to different ingredient types, with skill-based progression and meaningful failure states.

## 🔬 Refining Processes

### ⚒️ Grinding
- **Purpose**: Crushes and pulverizes solid materials
- **Valid Ingredients**: Ore archetype only
- **Examples**: Iron Ore → Iron Powder, Diamond → Diamond Dust
- **Characteristics**: 
  - Fastest processing time
  - Highest success rate
  - Minimal equipment requirements
  - Products typically more stable

### 🧪 Distilling  
- **Purpose**: Extracts essences through evaporation and condensation
- **Valid Ingredients**: Herb, Organic, Solvent archetypes
- **Examples**: Healing Herb → Healing Essence, Monster Blood → Pure Extract
- **Characteristics**:
  - Moderate processing time
  - Medium success rate
  - Requires specialized equipment
  - Products have enhanced potency

### 🔥 Roasting
- **Purpose**: Applies controlled heat to alter molecular structure
- **Valid Ingredients**: Herb, Organic archetypes
- **Examples**: Coffee Beans → Roasted Beans, Dragon Scale → Hardened Scale
- **Characteristics**:
  - Variable processing time
  - Success rate depends on timing
  - Risk of burning/destroying
  - Products gain unique properties

## 🎯 Success & Failure States

### ✅ Success
- Creates the intended refined ingredient
- Typically 1.5x potency of original
- Maintains ingredient archetype
- **Probability**: Based on skill level and ingredient difficulty

### 🌟 Critical Success
- Creates premium quality refined ingredient
- 2.0x+ potency with additional effects
- Enhanced stability and shelf life
- **Probability**: 10-20% of successful attempts

### ❌ Failed
- Ingredient returns unchanged
- No resources lost
- Player gains experience from attempt
- **Probability**: Decreases with skill progression

### 💥 Lost
- Ingredient is destroyed in the process
- Total loss of materials
- Most punishing outcome
- **Probability**: Higher for low-skill attempts on difficult ingredients

## 🎓 Skill System Integration

### Skill Levels (1-100)
- **Novice (1-20)**: Basic ingredients only, high failure/loss rates
- **Apprentice (21-40)**: Moderate ingredients, improved success rates
- **Journeyman (41-60)**: Most ingredients accessible, rare failures
- **Expert (61-80)**: High-end ingredients, critical success bonuses
- **Master (81-100)**: All ingredients, minimal risk, maximum efficiency

### Skill Benefits
- **Higher Success Rates**: +5% per 10 skill levels above minimum
- **Reduced Loss Rates**: Better technique prevents ingredient destruction
- **Critical Success Bonus**: Masters achieve critical successes more often
- **Batch Processing**: Higher skills unlock batch refining capabilities
- **Equipment Synergy**: Advanced skills work better with quality tools

## 🔧 Equipment & Bonuses

### Equipment Types
- **Basic Tools**: No bonus, available to all players
- **Quality Equipment**: +10% success rate, -5% loss rate
- **Master Crafted**: +20% success rate, +10% critical chance
- **Magical Apparatus**: +30% success rate, special effects

### Environmental Factors
- **Proper Workspace**: Clean laboratory provides bonuses
- **Temperature Control**: Critical for distilling and roasting
- **Timing**: Some processes require precise timing
- **Reagent Quality**: Pure solvents improve distillation

## 📊 Implementation Details

### Core Classes

#### `RefiningData` Struct
```csharp
public struct RefiningData
{
    public RefiningType refiningType;
    public RefiningDifficulty difficulty;
    public float baseSuccessRate;
    public float criticalSuccessRate;
    public float lossRate;
    public Ingredient successResult;
    public Ingredient criticalResult;
    public int minimumSkillLevel;
    public float processingTime;
    public string specialRequirements;
}
```

#### `RefinedIngredient` Class
- Extends base Ingredient class
- Tracks original source ingredient
- Records refining method used
- Includes purity and stability ratings
- Supports critical result variants

#### `IngredientRefiningController`
- Manages refining queue and active processes
- Handles skill checks and success calculations
- Integrates with UI and inventory systems
- Provides real-time processing updates

### Configuration Options

#### Per-Ingredient Settings
- `baseRefiningSuccessRate`: Ingredient-specific difficulty
- `stabilityRating`: Resistance to being lost on failure
- `minimumRefiningSkill`: Skill gate for attempting refining
- Explicit refined result assignments (optional)

#### System-Wide Settings
- `enableRealTimeProcessing`: Live vs. instant processing
- `allowBatchRefining`: Process multiple identical ingredients
- `maxBatchSize`: Limit concurrent batch operations
- Skill level multipliers and progression curves

## 🎮 User Experience Design

### Visual Feedback
- **Color Coding**: Different refining types have distinct themes
  - Grinding: Earth tones (browns, grays)
  - Distilling: Cool tones (blues, cyans)  
  - Roasting: Warm tones (reds, oranges)
- **Progress Indicators**: Real-time processing visualization
- **Result Animations**: Success/failure feedback with particle effects
- **Skill Gating**: Disabled buttons with tooltip explanations

### Audio Design
- **Grinding**: Mechanical crushing and pulverizing sounds
- **Distilling**: Bubbling, steaming, and droplet sounds
- **Roasting**: Crackling fire and sizzling effects
- **Success/Failure**: Distinct audio cues for each outcome

### Accessibility
- **Colorblind Support**: Icons and patterns supplement color coding
- **Skill Level Display**: Clear numerical indicators
- **Tooltip Information**: Detailed explanations for all elements
- **Keyboard Navigation**: Full functionality without mouse

## 🔄 Integration with Existing Systems

### Tetris Alchemy System
- Refined ingredients maintain grid placement properties
- Enhanced potency affects crafting success rates
- Critical refined ingredients unlock special interactions
- Refining can modify ingredient grid shapes

### Inventory Management
- Refined ingredients stack separately from base ingredients
- Stability system affects item degradation over time
- Quality indicators distinguish critical successes
- Batch operations respect inventory space limits

### Progression Systems
- Skill experience gained from all refining attempts
- Achievement unlocks for mastering refining types
- Recipe discoveries through experimentation
- Equipment upgrades unlock through progression

## 📈 Balancing Considerations

### Economic Impact
- **Resource Sink**: Failed attempts consume ingredients
- **Value Creation**: Successful refining increases ingredient worth
- **Risk/Reward**: Higher difficulty = greater potential payoff
- **Time Investment**: Real-time processing creates planning decisions

### Progression Pacing
- **Early Game**: Focus on basic ingredients, frequent failures teach system
- **Mid Game**: Unlock valuable ingredients, batch processing efficiency
- **Late Game**: Master-level ingredients, critical success farming
- **End Game**: Unique refining combinations, legendary ingredients

### Difficulty Scaling
- **Potency Scaling**: Higher potency = exponentially harder
- **Archetype Variations**: Some ingredient types naturally more difficult
- **Critical Thresholds**: Minimum skill requirements prevent grinding
- **Equipment Gates**: Best results require investment in tools

## 🛠️ Setup Instructions

### 1. Basic Scene Setup
```csharp
// Create refining controller
GameObject refiningSystem = new GameObject("RefiningSystem");
refiningSystem.AddComponent<IngredientRefiningController>();
refiningSystem.AddComponent<UIDocument>();
refiningSystem.AddComponent<RefiningSystemDemo>();
```

### 2. Configure Ingredients
For each ingredient that can be refined:
1. Set appropriate archetype (Ore, Herb, Organic, Solvent)
2. Enable desired refining methods (`canGrind`, `canDistill`, `canRoast`)
3. Create or assign refined result ingredients
4. Set difficulty parameters (`baseRefiningSuccessRate`, `minimumRefiningSkill`)

### 3. Create Refined Ingredients
```csharp
// Method 1: Create explicit refined ingredients
var groundIronOre = ScriptableObject.CreateInstance<RefinedIngredient>();
groundIronOre.Initialize(ironOre, RefiningType.Grinding, false, 1.0f);

// Method 2: Auto-generation during refining process
// System creates refined ingredients dynamically based on source
```

### 4. Skill System Integration
```csharp
// Connect to your skill system
refiningController.SetPlayerSkillLevels(
    playerSkills.GrindingLevel,
    playerSkills.DistillingLevel, 
    playerSkills.RoastingLevel
);

// Update equipment bonuses
refiningController.SetEquipmentBonus(equipment.GetRefiningBonus());
```

### 5. UI Integration
1. Assign the `IngredientRefiningSystem.uxml` to UIDocument
2. Apply `RefiningSystemStyles.uss` stylesheet
3. Connect button events and list updates
4. Implement result display and queue management

## 🔮 Advanced Features

### Catalyst System
- Special ingredients that enhance refining processes
- Temporary bonuses to success rates
- Unique interaction combinations
- Consumed during use

### Seasonal Effects
- Time-of-day bonuses for certain processes
- Weather impacts on success rates
- Lunar cycles affecting magical ingredients
- Festival periods with enhanced results

### Guild/Social Features
- Shared refining laboratories
- Collaborative batch processing
- Knowledge sharing systems
- Competitive leaderboards

### Automation Unlocks
- High-skill players can queue multiple processes
- Equipment upgrades enable unattended refining
- Batch processing scales with progression
- Quality control systems prevent waste

## 🎯 Success Metrics

Track these metrics to evaluate system effectiveness:
- **Engagement**: How often players use refining vs. raw ingredients
- **Progression**: Skill advancement rates and milestone achievements  
- **Economics**: Impact on ingredient market and value distribution
- **Retention**: Does refining complexity enhance or hinder enjoyment?

## 🐛 Troubleshooting

### Common Issues
1. **Low Success Rates**: Check skill levels match ingredient requirements
2. **Missing Results**: Ensure refined ingredients are created/assigned
3. **UI Not Updating**: Verify event subscriptions and element references
4. **Performance Issues**: Implement object pooling for frequent operations

### Debug Tools
- Enable detailed logging in `IngredientRefiningController`
- Use `RefiningSystemDemo` for isolated testing
- Monitor skill progression and equipment bonuses
- Validate ingredient archetype/refining method compatibility

This refining system adds strategic depth while maintaining the engaging, hands-on feel of your alchemy mechanics. Players must balance risk, reward, and resource management while progressing through increasingly complex refining challenges.