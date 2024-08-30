using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace com.playbux.map
{
    [CustomEditor(typeof(MapDatabase))]
    public class MapDatabaseEditor : Editor
    {
        private bool debugMode;
        private string newMapName;
        private string searchMapName;
        private MapDatabase database;

        private void OnEnable()
        {
            database = (MapDatabase)target;
            newMapName = "";
            searchMapName = "";
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("Box");

            debugMode = EditorGUILayout.ToggleLeft("Debug", debugMode);

            EditorGUILayout.EndVertical();

            if (debugMode)
            {
                base.OnInspectorGUI();
                return;
            }

            EditorGUILayout.BeginVertical("Box");

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Create Map Data", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Map Name", EditorStyles.miniLabel);
            newMapName = EditorGUILayout.TextField(newMapName);

            if (GUILayout.Button("Create Map", EditorStyles.toolbarButton))
            {
                database.CreateMap(newMapName);
                newMapName = "";
                EditorUtility.SetDirty(database);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label("Maps", EditorStyles.largeLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Search For Maps", EditorStyles.miniLabel);
            searchMapName = EditorGUILayout.TextField(searchMapName);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");

            if (database.Maps.Equals(null))
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                return;
            }

            for (int i = 0; i < database.Maps.Length; i++)
            {
                if (searchMapName.Length < 3)
                    continue;

                if (!database.Maps[i].name.ToLower().Contains(searchMapName.ToLower()))
                    continue;

                EditorGUILayout.BeginVertical("Button");
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("Map: " + database.Maps[i].name, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("Size", EditorStyles.miniLabel);
                GUILayout.Label("Width", EditorStyles.miniLabel);
                database.Maps[i].width = EditorGUILayout.IntSlider(database.Maps[i].width, 1, 64);
                GUILayout.Label("Height", EditorStyles.miniLabel);
                database.Maps[i].height = EditorGUILayout.IntSlider(database.Maps[i].height, 1, 64);
                EditorGUILayout.EndVertical();
                int chuckCount = database.Maps[i].chucks.Equals(null) ? 0 : database.Maps[i].chucks.Length;
                GUILayout.Label("Chucks: " + chuckCount, EditorStyles.miniLabel);

                if (GUILayout.Button("Add Chuck", EditorStyles.toolbarButton))
                {
                    var temp = new List<Chuck>();

                    if (database.Maps[i].chucks.Equals(null) || database.Maps[i].chucks.Length <= 0)
                    {
                        temp.Add(null);
                        database.Maps[i].chucks = temp.ToArray();
                    }
                    else
                    {
                        temp = database.Maps[i].chucks.ToList();
                        temp.Add(null);
                        database.Maps[i].chucks = temp.ToArray();
                    }

                    EditorUtility.SetDirty(database);
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                if (GUILayout.Button("Generate Map", EditorStyles.toolbarButton))
                {
                    int totalX = 0;
                    int totalY = 0;
                    int count = 0;

                    for (int h = 0; h < database.Maps[i].height; h++)
                    {
                        totalY += (database.Maps[i].chucks[count].GridSize * database.Maps[i].chucks[count].Height) * (h + 1);
                        for (int w = 0; w < database.Maps[i].width; w++)
                        {
                            totalX += (database.Maps[i].chucks[count].GridSize * database.Maps[i].chucks[count].Width) * h;
                            count++;
                        }
                    }
                    var map = new GameObject();

                    for (int h = 0; h < database.Maps[i].height; h++)
                    {
                        for (int w = 0; w < database.Maps[i].width; w++)
                        {
                            int index = h * database.Maps[i].width + w;
                            var parent = new GameObject();
                            parent.name = database.Maps[i].chucks[index].name;
                            // Position Calculation (Fitting each chuck)
                            float x = (-w * database.Maps[i].chucks[index].Width) * (database.Maps[i].chucks[index].GridSize * 0.01f) + ((totalX * 0.01f) * (1 / database.Maps[i].width));
                            // Unity's coordinate system starts from bottom-left, but we want top-left
                            // float y = (database.Maps[i].height / 2 - h) * database.Maps[i].chucks[index].Height * (database.Maps[i].chucks[index].GridSize * 0.01f);
                            float y = -h * (database.Maps[i].chucks[index].Height) * (database.Maps[i].chucks[index].GridSize * 0.01f) + ((totalY * 0.01f) * (1 / database.Maps[i].height));
                            float scale = database.Maps[i].chucks[index].HighQualityScale;
                            parent.transform.position = new Vector3(x, y, 0);

                            int texPos = 0;


                            for (int chuckHeight = 0; chuckHeight < database.Maps[i].chucks[index].Height; chuckHeight++)
                            {
                                for (int chuckWidth = 0; chuckWidth < database.Maps[i].chucks[index].Width; chuckWidth++)
                                {
                                    texPos++;

                                    if (!database.Maps[i].chucks[index].Textures.ContainsKey(texPos))
                                        continue;

                                    var cell = new GameObject();
                                    cell.name = database.Maps[i].chucks[index].name + "_" + texPos;
                                    float textureX = x + (database.Maps[i].chucks[index].Width / 2 - chuckWidth) * (database.Maps[i].chucks[index].GridSize * 0.01f);
                                    float textureY = (database.Maps[i].chucks[index].Height / 2 - chuckHeight) * (database.Maps[i].chucks[index].GridSize * 0.01f);
                                    cell.transform.position = new Vector3(textureX, 0, textureY);
                                    cell.transform.rotation = Quaternion.Euler(90, 0 , 0);
                                    cell.transform.localScale = new Vector3(-scale, scale, scale);
                                    var renderer = cell.AddComponent<SpriteRenderer>();
                                    cell.transform.SetParent(parent.transform);
                                    cell.transform.localPosition = new Vector3(cell.transform.localPosition.x, 0, cell.transform.localPosition.z);
                                    Texture2D texture = database.Maps[i].chucks[index].Textures[texPos].HighQuality;
                                    renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(texture));
                                    EditorUtility.SetDirty(cell);
                                }
                            }

                            parent.transform.rotation = Quaternion.Euler(-90, 0 , 0);
                            parent.transform.SetParent(map.transform);
                        }
                    }


                    map.transform.localScale = new Vector3(1, 2, 1);
                    map.name = database.Maps[i].name;
                    Debug.Log($"{totalX} {totalY} {count}");
                }

                for (int j = 0; j < database.Maps[i].chucks.Length; j++)
                {
                    database.Maps[i].chucks[j] = (Chuck)EditorGUILayout.ObjectField(database.Maps[i].chucks[j], typeof(Chuck));
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        private void CleanUp(int mapIndex, int chuckIndex, int textureIndex)
        {
            if (!database.Maps[mapIndex].chucks[chuckIndex].Textures[textureIndex].IsEmpty)
                return;

            File.Delete(Application.dataPath.Replace("Assets", AssetDatabase.GetAssetPath(database.Maps[mapIndex].chucks[chuckIndex].Textures[textureIndex])));

            // database.RemoveEmpty(mapIndex, chuckIndex);
            EditorUtility.SetDirty(database);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}