using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using UnityEngine;

[CreateAssetMenu(fileName = "EditorConfig", menuName = "Configs/Editor Config")]
public class EditorConfig : ScriptableObject {
    public GameObject floorGeneratorPrefab;
    public GlobalAreasDatabase globalAreasDatabase;

    [Header("Scene Setup")] 
    public GameObject playerPrefab;
    public GameObject mainCameraPrefab;
    public GameObject vCamExplorationPrefab;
    public GameObject vCamCombatPrefab;
}
