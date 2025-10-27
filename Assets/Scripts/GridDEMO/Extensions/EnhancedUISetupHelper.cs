using UnityEngine;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo.Extensions
{
    /// <summary>
    /// Helper class to provide public access to EnhancedUISetup configuration
    /// </summary>
    public static class EnhancedUISetupHelper
    {
        /// <summary>
        /// Configure EnhancedUISetup component with public access
        /// </summary>
        public static void ConfigureUISetup(EnhancedUISetup setupComponent, 
            bool autoSetupOnStart = false, 
            bool preserveExistingUI = true, 
            bool createDebugConsole = true, 
            bool enableKeyboardShortcuts = true)
        {
            if (setupComponent == null) return;
            
            // Use reflection to set private fields since they're not accessible
            var setupType = typeof(EnhancedUISetup);
            
            SetPrivateField(setupComponent, setupType, "autoSetupOnStart", autoSetupOnStart);
            SetPrivateField(setupComponent, setupType, "preserveExistingUI", preserveExistingUI);
            SetPrivateField(setupComponent, setupType, "createDebugConsole", createDebugConsole);
            SetPrivateField(setupComponent, setupType, "enableKeyboardShortcuts", enableKeyboardShortcuts);
            
            Debug.Log($"✅ Configured EnhancedUISetup: autoStart={autoSetupOnStart}, preserve={preserveExistingUI}, debug={createDebugConsole}, shortcuts={enableKeyboardShortcuts}");
        }
        
        private static void SetPrivateField(object instance, System.Type type, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(instance, value);
            }
            else
            {
                Debug.LogWarning($"Could not find private field: {fieldName} in {type.Name}");
            }
        }
        
        /// <summary>
        /// Get private field value from EnhancedUISetup
        /// </summary>
        public static T GetPrivateField<T>(EnhancedUISetup setupComponent, string fieldName)
        {
            if (setupComponent == null) return default(T);
            
            var setupType = typeof(EnhancedUISetup);
            var field = setupType.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return (T)field.GetValue(setupComponent);
            }
            
            return default(T);
        }
    }
}