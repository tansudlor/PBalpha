using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using com.playbux.LOD;
using com.playbux.utilis;

namespace com.playbux.map.editor
{
    [CustomEditor(typeof(MapObject))]
    public class MapObjectEditor : Editor
    {
        private bool debugFlag;
        private int width;
        private int height;
        private int gridSize = 256;
        private MapObject mapObject;
        private DefaultAsset lowQualityTargetTextureFolder;
        private DefaultAsset highQualityTargetTextureFolder;
        private DefaultAsset mediumQualityTargetTextureFolder;

        private void OnEnable()
        {
            mapObject ??= (MapObject)target;
            width = mapObject.Width;
            height = mapObject.Height;
            gridSize = mapObject.GridSize;
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

            GUILayout.Label("Width", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            width = EditorGUILayout.IntSlider(width, 1, 512);
            if (EditorGUI.EndChangeCheck())
            {
                mapObject.SetWidth(width);
                // chuck.ResizeTexture();
                EditorUtility.SetDirty(mapObject);
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Label("Height", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            height = EditorGUILayout.IntSlider(height, 1, 512);
            if (EditorGUI.EndChangeCheck())
            {
                mapObject.SetHeight(height);
                // chuck.ResizeTexture();
                EditorUtility.SetDirty(mapObject);
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Label("Grid Size", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            gridSize = EditorGUILayout.IntSlider(gridSize, 1, 512);
            if (EditorGUI.EndChangeCheck())
            {
                mapObject.SetGridSize(gridSize);
                EditorUtility.SetDirty(mapObject);
                serializedObject.ApplyModifiedProperties();
            }

            if (mapObject.Textures.Count > 0)
            {
                if (GUILayout.Button("Try Generate High Quality Chuck"))
                {
                    Create(mapObject.HighQualityScale, 3);
                    // PrefabUtility.SaveAsPrefabAssetAndConnect(parent, AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + ".prefab"), InteractionMode.AutomatedAction);
                }
            }

            GUILayout.Label("Low Quality Texture Folder", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            lowQualityTargetTextureFolder = (DefaultAsset)EditorGUILayout.ObjectField(lowQualityTargetTextureFolder, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (!Directory.Exists(AssetDatabase.GetAssetPath(lowQualityTargetTextureFolder)))
                    lowQualityTargetTextureFolder = null;
            }

            GUILayout.Label("Medium Quality Texture Folder", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            mediumQualityTargetTextureFolder = (DefaultAsset)EditorGUILayout.ObjectField(mediumQualityTargetTextureFolder, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (!Directory.Exists(AssetDatabase.GetAssetPath(mediumQualityTargetTextureFolder)))
                    mediumQualityTargetTextureFolder = null;
            }

            GUILayout.Label("High Quality Texture Folder", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            highQualityTargetTextureFolder = (DefaultAsset)EditorGUILayout.ObjectField(highQualityTargetTextureFolder, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (!Directory.Exists(AssetDatabase.GetAssetPath(highQualityTargetTextureFolder)))
                    highQualityTargetTextureFolder = null;
            }

            if (GUILayout.Button("Load Texture"))
            {
                string[] files;

                if (lowQualityTargetTextureFolder != null)
                {
                    files = Directory.GetFiles(AssetDatabase.GetAssetPath(lowQualityTargetTextureFolder), "*.png", SearchOption.TopDirectoryOnly);
                    SetLowQualityTexture(files);
                }

                if (mediumQualityTargetTextureFolder != null)
                {
                    files = Directory.GetFiles(AssetDatabase.GetAssetPath(mediumQualityTargetTextureFolder), "*.png", SearchOption.TopDirectoryOnly);
                    SetMediumQualityTexture(files);
                }

                if (highQualityTargetTextureFolder != null)
                {
                    files = Directory.GetFiles(AssetDatabase.GetAssetPath(highQualityTargetTextureFolder), "*.png", SearchOption.TopDirectoryOnly);
                    SetHighQualityTexture(files);
                }

                EditorUtility.SetDirty(mapObject);
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndVertical();
        }

        private void Create(float scale, int quality = 0)
        {
            var parent = mapObject;
            parent.name = mapObject.name;

            int texPos = 0;

            for (int i = 0; i < mapObject.Height; i++)
            {
                for (int j = 0; j < mapObject.Width; j++)
                {
                    texPos++;

                    if (!mapObject.Textures.ContainsKey(texPos))
                        continue;

                    Debug.Log(texPos);
                    var cell = new GameObject();
                    cell.name = mapObject.name + "_" + texPos;
                    float x = parent.transform.position.x + (mapObject.Width / 2 - j) * (mapObject.GridSize * 0.01f);
                    float y = parent.transform.position.y + (mapObject.Height / 2 - i) * (mapObject.GridSize * 0.01f);
                    cell.transform.position = new Vector3(x, 0, y);
                    var renderer = cell.AddComponent<SpriteRenderer>();
                    cell.transform.rotation = Quaternion.Euler(90, 0 , 0);
                    cell.transform.localScale = new Vector3(-scale, scale, scale);
                    Texture2D texture = null;

                    if (quality == 0)
                        texture = mapObject.Textures[texPos].DefaultQuality;
                    else if (quality == 1)
                        texture = mapObject.Textures[texPos].LowQuality;
                    else if (quality == 2)
                        texture = mapObject.Textures[texPos].MediumQuality;
                    else if (quality == 3)
                        texture = mapObject.Textures[texPos].HighQuality;

                    if (texture == null)
                        continue;

                    renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(texture));
                    cell.transform.SetParent(parent.transform);
                    EditorUtility.SetDirty(cell);
                }
            }

            parent.transform.localScale = new Vector3(1, 1, 2);
            parent.transform.rotation = Quaternion.Euler(-90, -90, 90);
        }

        private void SetDefaultTexture(string[] paths)
        {
            foreach (var file in paths)
            {
                var texture = (Texture2D)AssetDatabase.LoadAssetAtPath(file, typeof(Texture2D));

                if (texture is null)
                    continue;

                int position = int.Parse(texture.name.Split('_')[1]);

                if (!Directory.Exists(AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture")))
                {
                    Directory.CreateDirectory(AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture"));
                    AssetDatabase.Refresh();
                }

                string scriptablePath = AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture/" + mapObject.name + "-" + position + ".asset");

                LODTexture2D lodTexture;

                if (!File.Exists(scriptablePath))
                {
                    lodTexture = CreateInstance<LODTexture2D>();
                    lodTexture.SetDefaultTexture(texture);
                    AssetDatabase.CreateAsset(lodTexture, scriptablePath);
                    AssetDatabase.SaveAssets();
                }
                else
                    lodTexture = AssetDatabase.LoadAssetAtPath<LODTexture2D>(scriptablePath);

                lodTexture.SetDefaultTexture(texture);
                mapObject.AddTexture(position, lodTexture);
                EditorUtility.SetDirty(lodTexture);
                EditorUtility.SetDirty(mapObject);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void SetLowQualityTexture(string[] paths)
        {
            foreach (var file in paths)
            {
                var texture = (Texture2D)AssetDatabase.LoadAssetAtPath(file, typeof(Texture2D));

                if (texture is null)
                    continue;

                if (texture.IsEmpty())
                {
                    File.Delete(Application.dataPath.Replace("Assets", AssetDatabase.GetAssetPath(texture)));
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    continue;
                }

                int position = int.Parse(texture.name.Split('_')[1]);

                if (!Directory.Exists(AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture")))
                {
                    Directory.CreateDirectory(AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture"));
                    AssetDatabase.Refresh();
                }

                string scriptablePath = AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture/" + mapObject.name + "-" + position + ".asset");

                LODTexture2D lodTexture;

                if (!File.Exists(scriptablePath))
                {
                    lodTexture = CreateInstance<LODTexture2D>();
                    lodTexture.SetLowQualityTexture(texture);
                    AssetDatabase.CreateAsset(lodTexture, scriptablePath);
                    AssetDatabase.SaveAssets();
                }
                else
                    lodTexture = AssetDatabase.LoadAssetAtPath<LODTexture2D>(scriptablePath);

                lodTexture.SetLowQualityTexture(texture);
                mapObject.AddTexture(position, lodTexture);
                EditorUtility.SetDirty(lodTexture);
                EditorUtility.SetDirty(mapObject);
                serializedObject.ApplyModifiedProperties();
            }

            mapObject.Sort();
            EditorUtility.SetDirty(mapObject);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private void SetMediumQualityTexture(string[] paths)
        {
            foreach (var file in paths)
            {
                var texture = (Texture2D)AssetDatabase.LoadAssetAtPath(file, typeof(Texture2D));

                if (texture is null)
                    continue;

                if (texture.IsEmpty())
                {
                    File.Delete(Application.dataPath.Replace("Assets", AssetDatabase.GetAssetPath(texture)));
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    continue;
                }

                int position = int.Parse(texture.name.Split('_')[1]);
                if (!Directory.Exists(AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture")))
                {
                    Directory.CreateDirectory(AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture"));
                    AssetDatabase.Refresh();
                }

                string scriptablePath = AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture/" + mapObject.name + "-" + position + ".asset");

                LODTexture2D lodTexture;

                if (!File.Exists(scriptablePath))
                {
                    lodTexture = CreateInstance<LODTexture2D>();
                    lodTexture.SetMediumQualityTexture(texture);
                    AssetDatabase.CreateAsset(lodTexture, scriptablePath);
                    AssetDatabase.SaveAssets();
                }
                else
                    lodTexture = AssetDatabase.LoadAssetAtPath<LODTexture2D>(scriptablePath);

                lodTexture.SetMediumQualityTexture(texture);
                mapObject.AddTexture(position, lodTexture);
                EditorUtility.SetDirty(lodTexture);
                EditorUtility.SetDirty(mapObject);
                serializedObject.ApplyModifiedProperties();
            }

            mapObject.Sort();
            EditorUtility.SetDirty(mapObject);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private void SetHighQualityTexture(string[] paths)
        {
            foreach (var file in paths)
            {
                var texture = (Texture2D)AssetDatabase.LoadAssetAtPath(file, typeof(Texture2D));

                if (texture is null)
                    continue;

                if (texture.IsEmpty())
                {
                    File.Delete(Application.dataPath.Replace("Assets", AssetDatabase.GetAssetPath(texture)));
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    continue;
                }

                int position = int.Parse(texture.name.Split('_')[1]);

                if (!Directory.Exists(AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture")))
                {
                    Directory.CreateDirectory(AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture"));
                    AssetDatabase.Refresh();
                }

                string scriptablePath = AssetDatabase.GetAssetPath(mapObject).Replace(mapObject.name + ".prefab", mapObject.name + "-Texture/" + mapObject.name + "-" + position + ".asset");

                LODTexture2D lodTexture;

                if (!File.Exists(scriptablePath))
                {
                    lodTexture = CreateInstance<LODTexture2D>();
                    lodTexture.SetHighQualityTexture(texture);
                    AssetDatabase.CreateAsset(lodTexture, scriptablePath);
                }
                else
                    lodTexture = AssetDatabase.LoadAssetAtPath<LODTexture2D>(scriptablePath);

                lodTexture.SetHighQualityTexture(texture);
                mapObject.AddTexture(position, lodTexture);
                EditorUtility.SetDirty(lodTexture);
                EditorUtility.SetDirty(mapObject);
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }

            mapObject.Sort();
            EditorUtility.SetDirty(mapObject);
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}