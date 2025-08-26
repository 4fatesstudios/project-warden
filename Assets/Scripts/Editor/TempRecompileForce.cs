using UnityEditor;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Temporary script to force Unity recompilation
    /// </summary>
    public class TempRecompileForce
    {
        [MenuItem("Tools/Force Recompile")]
        public static void ForceRecompile()
        {
            AssetDatabase.Refresh();
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }
}
