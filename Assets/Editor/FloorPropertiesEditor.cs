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
        
        // Starting Room
        SerializedProperty startingRoom = serializedObject.FindProperty("startingRoom");
        EditorGUILayout.PropertyField(startingRoom);

        // =======================
        // Auto-populate Free Rooms + Hallways from Global DB
        // =======================
        if (floorProps.GlobalAreasDatabase != null) {
            var db = floorProps.GlobalAreasDatabase.GetDatabaseByArea(floorProps.Area);

            // Sync Free Rooms list
            if (db != null) {
                floorProps.FreeRooms ??= new List<FloorProperties.FreeRoomProperties>();
                SyncPropertiesList(db.Rooms, floorProps.FreeRooms, true);
                
                floorProps.Hallways ??= new List<FloorProperties.HallwayProperties>();
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
                DrawFreeRoomProperty(room);
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
                DrawHallwayProperty(hall);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // Helper: ensure the ScriptableObject lists match DB prefabs
    private void SyncPropertiesList<T>(
        IReadOnlyList<SpaceData> dbSpaces,
        List<T> targetList,
        bool isFreeRoom
    ) {
        if (dbSpaces == null)
            return;

        if (targetList == null)
            return;

        foreach (var space in dbSpaces) {
            if (space == null || space.SpacePrefab == null) continue;
            
            bool exists = false;
            foreach (var entry in targetList) {
                if (isFreeRoom) {
                    var pr = (FloorProperties.FreeRoomProperties)(object)entry;
                    if (pr.spaceData == space) { exists = true; break; }
                } else {
                    var pr = (FloorProperties.HallwayProperties)(object)entry;
                    if (pr.spaceData == space) { exists = true; break; }
                }
            }

            if (!exists) {
                if (isFreeRoom) {
                    targetList.Add((T)(object)new FloorProperties.FreeRoomProperties {
                        spaceData = space,
                        enabled = true
                    });
                } else {
                    targetList.Add((T)(object)new FloorProperties.HallwayProperties {
                        spaceData = space,
                        enabled = true
                    });
                }
            }
        }
    }

    // Draw prefab with preview and enabled toggle
    private void DrawFreeRoomProperty(FloorProperties.FreeRoomProperties room) {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        room.enabled = EditorGUILayout.Toggle(room.enabled, GUILayout.Width(20));
        if (room.spaceData != null) {
            Texture2D preview = AssetPreview.GetAssetPreview(room.spaceData.SpacePrefab);
            GUILayout.Label(preview, GUILayout.Width(50), GUILayout.Height(50));
            EditorGUILayout.LabelField(room.spaceData.SpacePrefab.name);
        }
        EditorGUILayout.EndHorizontal();

        // Only show extra options if enabled
        if (room.enabled) {
            EditorGUI.indentLevel++;
            room.MaxInstances = EditorGUILayout.IntField(
                new GUIContent("Max Instances", "0 = no max instances"), 
                room.MaxInstances
            );

            room.MinimumDepthFromStartingRoom = EditorGUILayout.IntField(
                new GUIContent("Min Depth From Start", "0 = no minimum depth"), 
                room.MinimumDepthFromStartingRoom
            );
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawHallwayProperty(FloorProperties.HallwayProperties hall) {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        hall.enabled = EditorGUILayout.Toggle(hall.enabled, GUILayout.Width(20));
        if (hall.spaceData != null) {
            Texture2D preview = AssetPreview.GetAssetPreview(hall.spaceData.SpacePrefab);
            GUILayout.Label(preview, GUILayout.Width(50), GUILayout.Height(50));
            EditorGUILayout.LabelField(hall.spaceData.SpacePrefab.name);
        }
        EditorGUILayout.EndHorizontal();

        // Only show extra options if enabled
        if (hall.enabled) {
            EditorGUI.indentLevel++;
            // Example: if you add more fields later
            // hall.maxInstances = EditorGUILayout.IntField("Max Instances", hall.maxInstances);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
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
