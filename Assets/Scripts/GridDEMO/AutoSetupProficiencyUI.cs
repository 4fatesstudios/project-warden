using UnityEngine;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

/// <summary>
/// Runtime executor for proficiency UI setup
/// This will automatically run when the scene loads
/// </summary>
[System.Serializable]
public class AutoSetupProficiencyUI 
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void AutoSetup()
    {
        // Only setup in GridDemo scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "GridDemo")
            return;
            
        // Check if setup is already done
        if (Object.FindFirstObjectByType<ProficiencyUISetup>() != null)
            return;
            
        // Create setup GameObject and run setup
        GameObject setupObj = new GameObject("Proficiency UI Setup (Auto)");
        ProficiencyUISetup setup = setupObj.AddComponent<ProficiencyUISetup>();
        
        // Run setup immediately
        setup.SetupProficiencyUI();
        
        // Optionally destroy the setup object after use
        Object.Destroy(setupObj, 1f);
        
        Debug.Log("🚀 Auto-setup proficiency UI completed!");
    }
}