using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Unity 6.0 compilation fix for cached event signatures
    /// This temporary component forces proper recompilation
    /// </summary>
    [System.Serializable]
    public class Unity6CompilationFix : MonoBehaviour
    {
        // Reference all the corrected types to force compilation refresh
        [SerializeField] private CompactUIDesigner compactUIDesigner;
        [SerializeField] private RightPanelManager rightPanelManager;
        [SerializeField] private GridDemoUIManager gridDemoUIManager;
        
        [ContextMenu("✅ Verify All Event Signatures")]
        public void VerifyEventSignatures()
        {
            Debug.Log("🔍 Unity 6.0 - Verifying all event signatures...");
            
            if (compactUIDesigner == null)
                compactUIDesigner = FindFirstObjectByType<CompactUIDesigner>();
                
            if (rightPanelManager == null)
                rightPanelManager = FindFirstObjectByType<RightPanelManager>();
                
            if (gridDemoUIManager == null)
                gridDemoUIManager = FindFirstObjectByType<GridDemoUIManager>();
            
            // Verify CompactUIDesigner events exist with correct signatures
            if (compactUIDesigner != null)
            {
                // These should now work without errors
                System.Action<Ingredient> a1 = compactUIDesigner.OnIngredientSelected;
                System.Action a2 = compactUIDesigner.OnGridCleared;
                System.Action a3 = compactUIDesigner.OnGridRandomized;
                System.Action<int> a4 = compactUIDesigner.OnGridSizeChanged; // int, not Vector2Int
                
                Debug.Log("✅ CompactUIDesigner events verified!");
            }
            
            // Verify RightPanelManager methods exist
            if (rightPanelManager != null)
            {
                try
                {
                    rightPanelManager.ShowIngredientInfo(null, Vector2.zero);
                    rightPanelManager.HideIngredientInfo();
                    Debug.Log("✅ RightPanelManager methods verified!");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ RightPanelManager method error: {e.Message}");
                }
            }
            
            Debug.Log("🚀 Unity 6.0 event signature verification complete!");
        }
        
        [ContextMenu("🔄 Force Unity Script Refresh")]
        public void ForceUnityScriptRefresh()
        {
            Debug.Log("🔄 Forcing Unity 6.0 script database refresh...");
            
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.EditorUtility.RequestScriptReload();
            Debug.Log("📜 Unity script database and assembly reload requested!");
            #endif
        }
    }
}