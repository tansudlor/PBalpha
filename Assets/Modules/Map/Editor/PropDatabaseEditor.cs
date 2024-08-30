using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace com.playbux.map.editor
{

    [CustomEditor(typeof(PropDatabase))]
    public class PropDatabaseEditor : Editor
    {
        private bool debugFlag;
        private string search;
        private string newName;
        private PropDatabase database;
        private List<bool> nameEditToggles = new List<bool>();
        private List<GameObject> targets = new List<GameObject>();

        private void OnEnable()
        {
            search = "";
            database = (PropDatabase)target;
            RefreshNameEdit();
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

            for (int i = 0; i < targets.Count; i++)
            {
                targets[i] = (GameObject)EditorGUILayout.ObjectField(targets[i], typeof(GameObject), true);
            }

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


            EditorGUI.BeginDisabledGroup(targets == null || targets.Count <= 0 || targets.Contains(null));

            if (GUILayout.Button("Create"))
            {
                for (int i = 0; i < targets.Count; i++)
                    database.Create(targets[i]);

                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedProperties();

                targets.Clear();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(2.5f);

            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Search", EditorStyles.miniLabel);
            search = EditorGUILayout.TextField(search);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            int count = 0;
            for (int i = 0; i < database.Keys.Length; i++)
            {
                count++;
                if (search.Length < 3)
                    break;

                if (!database.Keys[i].ToLower().Contains(search.ToLower()))
                    continue;

                EditorGUILayout.Space(1.5f);
                EditorGUILayout.BeginVertical("Button");
                EditorGUILayout.Space(1.5f);
                var key = database.Keys[i];
                GameObject gameObject = database.Data[i].propObject;
                EditorGUILayout.BeginVertical("Button");
                EditorGUILayout.BeginHorizontal();
                if (!nameEditToggles[count - 1])
                    GUILayout.Label($"{database.Keys[i]}", EditorStyles.largeLabel);
                else
                {
                    EditorGUI.BeginChangeCheck();
                    key = EditorGUILayout.TextField("", key);
                }

                if (GUILayout.Button(nameEditToggles[count - 1] ? "Save" : "Edit"))
                {
                    if (nameEditToggles[count - 1] && EditorGUI.EndChangeCheck())
                    {
                        database.Edit(key, database.Data[i].propObject, database.Data[i].propCollider);
                        database.Remove(key);
                        EditorUtility.SetDirty(database);
                        serializedObject.ApplyModifiedProperties();
                    }

                    nameEditToggles[count - 1] = !nameEditToggles[count - 1];
                    RefreshNameEdit();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    return;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.Label("Prefab", EditorStyles.miniLabel);
                EditorGUI.BeginChangeCheck();
                gameObject = (GameObject)EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true);

                if (EditorGUI.EndChangeCheck())
                {
                    database.Edit(key, gameObject, database.Data[i].propCollider);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();

                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();

                    return;
                }

                GUILayout.Label("Colliders", EditorStyles.miniLabel);
                EditorGUI.BeginChangeCheck();
                for (int j = 0; j < database.Data[i].propCollider.Length; j++)
                {
                    database.Data[i].propCollider[j] = (Collider2D)EditorGUILayout.ObjectField(database.Data[i].propCollider[j], typeof(Collider2D), true);
                }

                EditorGUILayout.Space(1.25f);

                if (EditorGUI.EndChangeCheck())
                {
                    database.Edit(key, gameObject, database.Data[i].propCollider);
                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();
                    EditorGUILayout.EndVertical();
                    break;
                }

                if (GUILayout.Button("Remove"))
                {
                    database.Remove(database.Keys[i]);
                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }

                EditorGUILayout.Space(1.5f);

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void RefreshNameEdit()
        {
            for (int i = 0; i < database.Keys.Length; i++)
            {
                nameEditToggles.Add(false);
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