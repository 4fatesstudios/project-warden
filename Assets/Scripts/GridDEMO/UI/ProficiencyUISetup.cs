using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// One-time setup script to create proficiency UI in the scene
    /// This script will create all necessary UI elements if they don't exist
    /// </summary>
    public class ProficiencyUISetup : MonoBehaviour
    {
        [Header("Setup Settings")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool debugMode = true;
        
        [ContextMenu("Setup Proficiency UI")]
        public void SetupProficiencyUI()
        {
            if (debugMode)
                Debug.Log("🚀 Starting Proficiency UI Setup...");
                
            // Step 1: Create or find UI Canvas
            Canvas uiCanvas = FindOrCreateCanvas();
            
            // Step 2: Create proficiency display at top (disabled for text display)
            CreateProficiencyDisplay(uiCanvas);
            
            // Step 3: Create or ensure GridDemoUIIntegration exists
            EnsureUIIntegration();
            
            if (debugMode)
                Debug.Log("✅ Proficiency UI Setup Complete! (Text display disabled, colored display preserved)");
        }
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupProficiencyUI();
            }
        }
        
        private Canvas FindOrCreateCanvas()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Proficiency UI Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                if (debugMode)
                    Debug.Log("📱 Created new UI Canvas");
            }
            else if (debugMode)
            {
                Debug.Log("📱 Found existing UI Canvas");
            }
            
            return canvas;
        }
        
        private void CreateProficiencyDisplay(Canvas canvas)
        {
            // Disabled: User prefers colored proficiency display over text
            if (debugMode)
                Debug.Log("🎯 Proficiency text display disabled - user prefers colored visual display");
            return;
        }
        
        private void EnsureUIIntegration()
        {
            GridDemoUIIntegration integration = FindFirstObjectByType<GridDemoUIIntegration>();
            
            if (integration == null)
            {
                GameObject integrationObj = new GameObject("UI Integration Manager");
                integration = integrationObj.AddComponent<GridDemoUIIntegration>();
                
                if (debugMode)
                    Debug.Log("🔧 Created GridDemoUIIntegration component");
            }
            else if (debugMode)
            {
                Debug.Log("🔧 Found existing GridDemoUIIntegration");
            }
            
            // Force setup
            integration.ForceSetupIntegration();
        }
        
        [ContextMenu("Clean Up and Recreate")]
        public void CleanUpAndRecreate()
        {
            // Remove existing proficiency displays
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                Transform proficiencyDisplay = canvas.transform.Find("Proficiency Display");
                if (proficiencyDisplay != null)
                {
                    DestroyImmediate(proficiencyDisplay.gameObject);
                }
            }
            
            // Remove existing integration components
            GridDemoUIIntegration[] integrations = FindObjectsByType<GridDemoUIIntegration>(FindObjectsSortMode.None);
            foreach (GridDemoUIIntegration integration in integrations)
            {
                DestroyImmediate(integration.gameObject);
            }
            
            // Recreate everything
            SetupProficiencyUI();
            
            if (debugMode)
                Debug.Log("🔄 Cleaned up and recreated proficiency UI");
        }
    }
}