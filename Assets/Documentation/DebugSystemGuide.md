# 🔧 Debug System Guide

## 🎯 Overview

The Grid Demo now features a comprehensive, toggleable debug system that provides detailed logging and analytics for all major systems. This system allows developers to selectively enable/disable debug output for specific categories, reducing console noise while maintaining detailed debugging capabilities.

## 🏗️ Architecture

### **Core Components**

1. **DebugSystemConfig**: Central configuration system for all debug categories
2. **ObstacleSpawnDebugger**: Specialized debugger for obstacle spawn mechanics
3. **DebugStatusDisplay**: Optional UI component for real-time debug status

### **Debug Categories**

| Category | Purpose | Default |
|----------|---------|---------|
| **🚧 Obstacle Spawn** | Obstacle spawn calculations and type distribution | ✅ ON |
| **🚧 Obstacle Interaction** | Ingredient-obstacle interactions | ✅ ON |
| **🎯 Ingredient Placement** | Ingredient placement operations | ❌ OFF |
| **🎯 Collision Detection** | Detailed collision checking | ❌ OFF |
| **🎯 Shape Collision** | Shape-based collision analysis | ❌ OFF |
| **🔄 Grid State** | Grid state changes and updates | ❌ OFF |
| **🔄 Cell Verification** | Cell occupancy verification | ❌ OFF |
| **🎨 Particle Effects** | Particle system operations | ✅ ON |
| **🎨 Visualizer** | Grid visualizer operations | ❌ OFF |
| **📊 Proficiency Grading** | Grade calculations and scoring | ✅ ON |
| **🧪 Testing** | Test operations and development tools | ✅ ON |
| **🧪 Input** | Mouse input and user interactions | ❌ OFF |
| **🚨 Error Recovery** | Error recovery and system repairs | ✅ ON |
| **🚨 Performance** | Performance monitoring | ❌ OFF |

## 🚧 Obstacle Spawn Debugging

### **Features**

- **Real-time Spawn Tracking**: Every spawn attempt is logged with roll vs chance
- **Success Rate Analytics**: Running statistics on spawn success rates
- **Type Distribution Analysis**: Tracks which obstacle types spawn most often
- **Probability Testing**: Built-in tools to test spawn probability accuracy

### **Key Metrics Tracked**

```
🚧 Spawn Stats Example:
Spawns: 7/25 (28.0%) | Last: 0.847 vs 0.150 = ❌

📊 Distribution:
🪨 Corporeal: 2 (28.6%)
❄️ Frigid: 1 (14.3%)
🔥 Scorch: 2 (28.6%)
☢️ Caustic: 1 (14.3%)
⚡ Arc: 1 (14.3%)
✨ Divine: 0 (0.0%)
```

### **Context Menu Actions**

- **Add Random Obstacle**: Spawn a test obstacle with full debugging
- **Test Current Obstacle Spawn Chance**: Run 100 probability tests
- **Show Obstacle Spawn Statistics**: Generate comprehensive analytics report
- **Respawn All Obstacles**: Clear and regenerate all obstacles
- **Generate Spawn Analytics Report**: Detailed spawn pattern analysis

## 📋 Usage Instructions

### **For Developers**

1. **Setup**: Add `DebugSystemConfig` to your GridGameManager GameObject
2. **Configure**: Use the Inspector to toggle debug categories on/off
3. **Monitor**: Check console output filtered by category icons
4. **Analyze**: Use context menu actions for detailed reports

### **Inspector Configuration**

```csharp
// In GridGameManager Inspector:
[Header("Debug Systems")]
[SerializeField] private DebugSystemConfig debugSystemConfig;
[SerializeField] private ObstacleSpawnDebugger obstacleSpawnDebugger;
```

### **Quick Setup Commands**

- **Essential Debug Only**: Enables only critical debug categories
- **Enable All Debug Categories**: Turn on all debug output (verbose)
- **Disable All Debug Categories**: Silent mode for production testing

## 🎮 Runtime Controls

### **Context Menu Commands**

Right-click on GridGameManager in the hierarchy:

```
🔧 Debug System Controls:
├── Enable All Debug Categories
├── Disable All Debug Categories
├── Enable Essential Debug Only
└── Show Current Debug Status

🚧 Obstacle Debug:
├── Add Random Obstacle
├── Test Current Obstacle Spawn Chance
├── Show Obstacle Spawn Statistics
├── Respawn All Obstacles
└── Generate Spawn Analytics Report
```

### **Code Examples**

```csharp
// Using the debug system in your code:
DebugSystemConfig.LogObstacleSpawn("Custom obstacle spawn message");
DebugSystemConfig.LogIngredientPlacement("Placement operation details");
DebugSystemConfig.LogCollisionDetection("Collision analysis");

// Check if a category is enabled:
if (DebugSystemConfig.ObstacleSpawnDebug)
{
    // Only execute expensive debug code if category is enabled
    PerformDetailedAnalysis();
}
```

## 📊 Obstacle Spawn Analytics

### **Probability Testing**

The system includes built-in probability testing:

```
🚧 === TESTING SPAWN PROBABILITY ===
🎲 Running 100 spawn chance tests with 15.0% chance

📊 TEST RESULTS:
   • Expected success rate: 15.00%
   • Actual success rate: 17.00% (17/100)
   • Deviation: 2.0 percentage points
   ✅ Random generation is working correctly
```

### **Distribution Analysis**

Tracks obstacle type distribution for balance verification:

```
🎲 OBSTACLE TYPE DISTRIBUTION (12 total):
   🪨 Corporeal: 3 (25.0%)
   ❄️ Frigid: 2 (16.7%)
   🔥 Scorch: 2 (16.7%)
   ☢️ Caustic: 2 (16.7%)
   ⚡ Arc: 2 (16.7%)
   ✨ Divine: 1 (8.3%)

📊 Distribution Analysis:
   • Expected per type: 16.7%
   • Average deviation: 2.8%
   ✅ Distribution is well-balanced
```

## 🎨 UI Integration

### **DebugStatusDisplay Component**

Optional UI component for real-time debug status:

```csharp
// Features:
- Real-time debug category status
- Live obstacle spawn statistics
- Toggle debug visibility
- Auto-refresh with configurable interval
- Manual refresh button
```

### **Setup Instructions**

1. Create a Canvas in your scene
2. Add `DebugStatusDisplay` component
3. Assign text components or let it create default ones
4. Configure update interval and visibility

## 🔍 Troubleshooting

### **Common Issues**

1. **Debug not appearing**: Check if the specific category is enabled
2. **Too much console spam**: Disable verbose categories like Input or Grid State
3. **Missing spawn stats**: Ensure ObstacleSpawnDebugger is added to GridGameManager
4. **UI not updating**: Check DebugStatusDisplay auto-update settings

### **Performance Considerations**

- Disable debug categories in production builds
- Use `#if UNITY_EDITOR` for development-only debug code
- Heavy debug categories (Collision, Grid State) should be used sparingly
- The system automatically prevents expensive operations when categories are disabled

## 🛠️ Customization

### **Adding New Debug Categories**

```csharp
// In DebugSystemConfig.cs:
[SerializeField] private bool enableMyNewCategory = false;
public static bool MyNewCategoryDebug => Instance?.enableMyNewCategory ?? false;

public static void LogMyNewCategory(string message) 
{
    if (MyNewCategoryDebug) Debug.Log($"🆕 MY CATEGORY: {message}");
}
```

### **Extending Obstacle Analytics**

```csharp
// In ObstacleSpawnDebugger.cs:
// Add new metrics, analysis methods, or visualization tools
// All metrics are automatically tracked and displayed
```

## 📝 Migration from Old Debug System

### **Before (Old System)**
```csharp
Debug.Log("🚧 Spawned obstacle at position");
if (enableCollisionDebugLogging)
    Debug.Log("Collision check details");
```

### **After (New System)**
```csharp
DebugSystemConfig.LogObstacleSpawn("Spawned obstacle at position");
DebugSystemConfig.LogCollisionDetection("Collision check details");
```

### **Benefits**
- ✅ Centralized control
- ✅ Clear categorization
- ✅ Toggleable at runtime
- ✅ Performance-friendly
- ✅ Consistent formatting
- ✅ Advanced analytics

---

*This debug system provides comprehensive insight into the Grid Demo's operations while maintaining clean, organized output that can be easily filtered and analyzed.*