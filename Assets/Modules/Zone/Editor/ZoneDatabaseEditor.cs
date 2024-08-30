using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace com.playbux.zone.editor
{
    [CustomEditor(typeof(ZoneDatabase))]
    public class ZoneDatabaseEditor : Editor
    {
        private bool debugFlag;
        private string search;

        private ZoneDatabase database;
        private List<GameObject> targets = new List<GameObject>();

        private void OnEnable()
        {
            search = "";
            database = (ZoneDatabase)target;
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

            var draggedObject = DropZone("Drag and drop zone game objects here");

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

            EditorGUI.BeginDisabledGroup(targets == null || targets.Count <= 0 || targets.Contains(null));

            if (GUILayout.Button("Create"))
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    GameObject prefab = null;

                    bool isPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(targets[i]) && PrefabUtility.IsPartOfAnyPrefab(targets[i]);

                    var collider = targets[i].GetComponentInChildren<CompositeCollider2D>();
                    var colPrefab = PrefabUtility.SaveAsPrefabAsset(collider.gameObject, $"{database.path}/{targets[i].name}_collider.prefab").GetComponent<CompositeCollider2D>();

                    if (isPrefab)
                        prefab = PrefabUtility.SavePrefabAsset(targets[i]);
                    else
                        prefab = PrefabUtility.SaveAsPrefabAsset(targets[i], $"{database.path}/{targets[i].name}.prefab");

                    var key = new ZoneKey();
                    var asset = new ZoneAsset();
                    key.name = targets[i].name;
                    key.position = targets[i].transform.position;
                    asset.prefab = prefab;
                    asset.collider = colPrefab;
                    database.Add(key, asset);
                }

                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();

                targets.Clear();
            }

            EditorGUILayout.EndVertical();
        }

        private static Object[] DropZone(string title)
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