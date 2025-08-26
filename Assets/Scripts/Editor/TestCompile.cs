using UnityEditor;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class TestCompile
    {
        [MenuItem("Tools/Test")]
        public static void Test()
        {
            UnityEngine.Debug.Log("Test successful");
        }
    }
}