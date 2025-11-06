using UnityEngine;
using UnityEditor;

public class MeshMoverEditor : MonoBehaviour
{
    [MenuItem("Tools/Move Mesh Components")]
    static void MoveMeshComponents()
    {
        if (Selection.objects.Length != 2)
        {
            Debug.LogError("Select exactly 2 GameObjects: first source, then target.");
            return;
        }

        GameObject source = Selection.objects[0] as GameObject;
        GameObject target = Selection.objects[1] as GameObject;

        if (!source || !target)
        {
            Debug.LogError("Invalid selection.");
            return;
        }

        // MeshFilter
        MeshFilter sourceMF = source.GetComponent<MeshFilter>();
        if (sourceMF)
        {
            MeshFilter targetMF = target.GetComponent<MeshFilter>();
            if (!targetMF) targetMF = target.AddComponent<MeshFilter>();

            targetMF.sharedMesh = sourceMF.sharedMesh;
            DestroyImmediate(sourceMF);
        }

        // MeshRenderer
        MeshRenderer sourceMR = source.GetComponent<MeshRenderer>();
        if (sourceMR)
        {
            MeshRenderer targetMR = target.GetComponent<MeshRenderer>();
            if (!targetMR) targetMR = target.AddComponent<MeshRenderer>();

            targetMR.sharedMaterials = sourceMR.sharedMaterials;
            DestroyImmediate(sourceMR);
        }

        Debug.Log($"Moved MeshFilter and MeshRenderer from {source.name} to {target.name}");
    }
}