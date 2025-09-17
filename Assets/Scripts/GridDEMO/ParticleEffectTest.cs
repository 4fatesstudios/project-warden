using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Test script to verify particle effects compilation and functionality
    /// </summary>
    public class ParticleEffectTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool enableTestLogging = true;
        
        private IngredientEffectVisualizer effectVisualizer;
        
        private void Start()
        {
            if (enableTestLogging)
            {
                Debug.Log("🧪 ParticleEffectTest: Mixed particle effects system initialized");
                Debug.Log("   ✨ Similar infusions = CONTINUOUS gentle sparkles");
                Debug.Log("   💥 Different infusions = PERIODIC dramatic bursts");
                Debug.Log("   🌟 Neutral interactions = CONTINUOUS subtle effects");
            }
            
            effectVisualizer = FindObjectOfType<IngredientEffectVisualizer>();
            if (effectVisualizer == null && enableTestLogging)
            {
                Debug.LogWarning("🧪 ParticleEffectTest: No IngredientEffectVisualizer found in scene");
            }
        }
        
        [ContextMenu("Test Mixed Effects")]
        public void TestMixedEffects()
        {
            if (effectVisualizer != null)
            {
                Debug.Log("🧪 ParticleEffectTest: Testing mixed particle effects (continuous + periodic)");
                effectVisualizer.RefreshAllIngredientInteractions();
            }
            else
            {
                Debug.LogError("🧪 ParticleEffectTest: Cannot test - no IngredientEffectVisualizer found");
            }
        }
        
        [ContextMenu("Clear All Effects")]
        public void ClearAllEffects()
        {
            if (effectVisualizer != null)
            {
                Debug.Log("🧪 ParticleEffectTest: Clearing all effects");
                effectVisualizer.ClearAllEffects();
            }
            else
            {
                Debug.LogError("🧪 ParticleEffectTest: Cannot clear - no IngredientEffectVisualizer found");
            }
        }
        
        [ContextMenu("Log Effect Behavior")]
        public void LogEffectBehavior()
        {
            Debug.Log("🧪 Current Effect Behavior:");
            Debug.Log("   ✨ SIMILAR infusions → Continuous gentle sparkles (5-20 particles/sec)");
            Debug.Log("   💥 DIFFERENT infusions → Periodic bursts (15-30 particles every 1-6 seconds)");
            Debug.Log("   🌟 NEUTRAL interactions → Continuous subtle sparkles (3 particles/sec)");
        }
    }
}