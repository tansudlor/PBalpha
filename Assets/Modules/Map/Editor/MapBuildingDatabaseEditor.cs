using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace com.playbux.map
{
    [CustomEditor(typeof(MapBuildingDatabase))]
    public class MapBuildingDatabaseEditor : Editor
    {
        private bool debugFlag;
        private string search;
        private int mapIdIndex;
        private Vector2 position;
        private MapDatabase mapDatabase;
        private MapBuildingDatabase database;
        private List<GameObject> targets = new List<GameObject>();

        private string[] mapIds;

        private void OnEnable()
        {
            search = "";
            database = (MapBuildingDatabase)target;
            string path = AssetDatabase.GetAssetPath(database);
            string mapPath = path.Replace("MapBuildingDatabase.asset", "MapDatabase.asset");
            mapDatabase = AssetDatabase.LoadAssetAtPath<MapDatabase>(mapPath);
            mapIds = mapDatabase.Maps.Select(m => m.name).ToArray();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            debugFlag = EditorGUILayout.ToggleLeft("Debug?", debugFlag);

            if (debugFlag)
            {
                base.OnInspectorGUI();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Add New Building", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            GUILayout.Label("Target Map", EditorStyles.miniLabel);
            mapIdIndex = EditorGUILayout.Popup(mapIdIndex, mapIds);
            EditorGUILayout.EndVertical();

            var draggedObject = DropZone("Drag and drop prop game objects here");

            if (draggedObject is not null)
            {
                for (int i = 0; i < draggedObject.Length; i++)
                {
                    var go = (GameObject)draggedObject[i];
                    if (targets.Contains(go))
                        continue;

                    targets.Add(go);
                }
            }

            if (GUILayout.Button("Create"))
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    var instance = targets[i];
                    var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    var wrapperData = new MapBuildingData();
                    wrapperData.position = instance.transform.position;
                    wrapperData.prefab = prefab;
                    database.Add(mapDatabase.Maps[mapIdIndex].name, wrapperData);
                }

                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();

                targets.Clear();
            }
        }

        public static Object[] DropZone(string title)
        {
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 20.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, title);

            EventType eventType = Event.current.type;
            bool isAccepted = false;

            if (eventType is EventType.DragUpdated or EventType.DragPerform){
                if (!dropArea.Contains (evt.mousePosition))
                    return null;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (eventType == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }
                Event.current.Use();
            }

            return isAccepted ? DragAndDrop.objectReferences : null;
        }
    }
}