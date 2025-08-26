using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FloorProperties))]
public class FloorPropertiesEditor : BaseDataSOEditor {
    private bool freeRoomsFoldout = true;
    private bool hallwaysFoldout = true;

    public override void OnInspectorGUI() {
        DrawDataBaseInspector();
        serializedObject.Update();

        FloorProperties floorProps = (FloorProperties)target;

        // Floor number (readonly)
        GUI.enabled = false;
        SerializedProperty floorNumber = serializedObject.FindProperty("floorNumber");
        EditorGUILayout.PropertyField(floorNumber);

        // Area
        SerializedProperty area = serializedObject.FindProperty("area");
        EditorGUILayout.PropertyField(area);
        GUI.enabled = true;

        // Min/Max Free Rooms Dual Slider
        SerializedProperty minFree = serializedObject.FindProperty("minimumFreeRooms");
        SerializedProperty maxFree = serializedObject.FindProperty("maximumFreeRooms");

        EditorGUILayout.LabelField("Free Rooms Range");
        EditorGUI.indentLevel++;
        float minVal = minFree.intValue;
        float maxVal = maxFree.intValue;

        EditorGUILayout.MinMaxSlider(new GUIContent("Min / Max"), ref minVal, ref maxVal, 0, FloorProperties.MaxFreeRooms);

        EditorGUILayout.BeginHorizontal();
        minFree.intValue = EditorGUILayout.IntField("Min", Mathf.RoundToInt(minVal));
        maxFree.intValue = EditorGUILayout.IntField("Max", Mathf.RoundToInt(maxVal));
        EditorGUILayout.EndHorizontal();

        minFree.intValue = Mathf.Clamp(minFree.intValue, 0, FloorProperties.MaxFreeRooms);
        maxFree.intValue = Mathf.Clamp(maxFree.intValue, minFree.intValue, FloorProperties.MaxFreeRooms);
        EditorGUI.indentLevel--;

        // Seed RNG
        SerializedProperty seedRNG = serializedObject.FindProperty("seedRNG");
        EditorGUILayout.PropertyField(seedRNG);

        // =======================
        // Auto-populate Free Rooms + Hallways from Global DB
        // =======================
        if (floorProps.GlobalAreasDatabase != null) {
            var db = floorProps.GlobalAreasDatabase.GetDatabaseByArea(floorProps.Area);

            // Sync Free Rooms list
            if (db != null) {
                SyncPropertiesList(db.Rooms, floorProps.FreeRooms, true);
                SyncPropertiesList(db.Hallways, floorProps.Hallways, false);
            }
        }

        // =======================
        // Draw Free Rooms
        // =======================
        freeRoomsFoldout = EditorGUILayout.Foldout(freeRoomsFoldout, "Free Rooms");
        if (freeRoomsFoldout) {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable All")) {
                SetAllEnabled(floorProps.FreeRooms, true);
            }
            if (GUILayout.Button("Disable All")) {
                SetAllEnabled(floorProps.FreeRooms, false);
            }
            EditorGUILayout.EndHorizontal();

            foreach (var room in floorProps.FreeRooms) {
                DrawPrefabProperty(room.freeRoomPrefab, ref room.enabled);
            }
        }

        // =======================
        // Draw Hallways
        // =======================
        hallwaysFoldout = EditorGUILayout.Foldout(hallwaysFoldout, "Hallways");
        if (hallwaysFoldout) {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable All")) {
                SetAllEnabled(floorProps.Hallways, true);
            }
            if (GUILayout.Button("Disable All")) {
                SetAllEnabled(floorProps.Hallways, false);
            }
            EditorGUILayout.EndHorizontal();

            foreach (var hall in floorProps.Hallways) {
                DrawPrefabProperty(hall.hallwayPrefab, ref hall.enabled);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // Helper: ensure the ScriptableObject lists match DB prefabs
    private void SyncPropertiesList<T>(
        IReadOnlyList<SpaceData> dbSpaces,
        System.Collections.Generic.List<T> targetList,
        bool isFreeRoom
    ) {
        if (targetList == null)
            return;

        foreach (var space in dbSpaces) {
            if (space == null || space.SpacePrefab == null) continue;

            GameObject prefab = space.SpacePrefab;

            bool exists = false;
            foreach (var entry in targetList) {
                if (isFreeRoom) {
                    var pr = (FloorProperties.FreeRoomProperties)(object)entry;
                    if (pr.freeRoomPrefab == prefab) { exists = true; break; }
                } else {
                    var pr = (FloorProperties.HallwayProperties)(object)entry;
                    if (pr.hallwayPrefab == prefab) { exists = true; break; }
                }
            }

            if (!exists) {
                if (isFreeRoom) {
                    targetList.Add((T)(object)new FloorProperties.FreeRoomProperties {
                        freeRoomPrefab = prefab,
                        enabled = true
                    });
                } else {
                    targetList.Add((T)(object)new FloorProperties.HallwayProperties {
                        hallwayPrefab = prefab,
                        enabled = true
                    });
                }
            }
        }
    }

    // Draw prefab with preview and enabled toggle
    private void DrawPrefabProperty(GameObject prefab, ref bool enabled) {
        EditorGUILayout.BeginHorizontal();
        enabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(20));
        if (prefab != null) {
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);
            GUILayout.Label(preview, GUILayout.Width(50), GUILayout.Height(50));
            EditorGUILayout.LabelField(prefab.name);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SetAllEnabled<T>(System.Collections.Generic.List<T> list, bool state) {
        if (list == null) return;
        foreach (var entry in list) {
            if (entry is FloorProperties.FreeRoomProperties fr) {
                fr.enabled = state;
            } else if (entry is FloorProperties.HallwayProperties hr) {
                hr.enabled = state;
            }
        }
    }
}
