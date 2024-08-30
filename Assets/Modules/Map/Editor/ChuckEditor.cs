using System.IO;
using UnityEditor;
using UnityEngine;
using com.playbux.LOD;
using com.playbux.utilis;

namespace com.playbux.map
{
    [CustomEditor(typeof(Chuck))]
    public class ChuckEditor : Editor
    {
        private bool debugMode;
        private int width;
        private int height;
        private int gridSize = 256;
        private Chuck chuck;
        private DefaultAsset lowQualityTargetTextureFolder;
        private DefaultAsset highQualityTargetTextureFolder;
        private DefaultAsset mediumQualityTargetTextureFolder;

        private void OnEnable()
        {
            chuck = (Chuck)target;
            width = chuck.Width;
            height = chuck.Height;
            gridSize = chuck.GridSize;
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

            GUILayout.Label("Width", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            width = EditorGUILayout.IntSlider(width, 1, 1024);
            if (EditorGUI.EndChangeCheck())
            {
                chuck.SetWidth(width);
                // chuck.ResizeTexture();
                EditorUtility.SetDirty(chuck);
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Label("Height", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            height = EditorGUILayout.IntSlider(height, 1, 1024);
            if (EditorGUI.EndChangeCheck())
            {
                chuck.SetHeight(height);
                // chuck.ResizeTexture();
                EditorUtility.SetDirty(chuck);
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Label("Grid Size", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            gridSize = EditorGUILayout.IntSlider(gridSize, 1, 1024);
            if (EditorGUI.EndChangeCheck())
            {
                chuck.SetGridSize(gridSize);
                EditorUtility.SetDirty(chuck);
                serializedObject.ApplyModifiedProperties();
            }

            if (chuck.Textures.Count > 0)
            {
                if (GUILayout.Button("Try Generate High Quality Chuck"))
                {
                    Create(chuck.HighQualityScale, 3);
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

                EditorUtility.SetDirty(chuck);
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndVertical();
        }

        private void Create(float scale, int quality = 0)
        {
            var parent = new GameObject();
            parent.name = chuck.name;

            int texPos = 0;

            for (int i = 0; i < chuck.Height; i++)
            {
                for (int j = 0; j < chuck.Width; j++)
                {
                    texPos++;

                    if (!chuck.Textures.ContainsKey(texPos))
                        continue;

                    var cell = new GameObject();
                    cell.name = chuck.name + "_" + texPos;
                    float x = parent.transform.position.x + (chuck.Width / 2 - j) * (chuck.GridSize * 0.01f);
                    float y = parent.transform.position.y + (chuck.Height / 2 - i) * (chuck.GridSize * 0.01f);
                    cell.transform.position = new Vector3(x, 0, y);
                    var renderer = cell.AddComponent<SpriteRenderer>();
                    cell.transform.rotation = Quaternion.Euler(90, 0 , 0);
                    cell.transform.localScale = new Vector3(-scale, scale, scale);
                    Texture2D texture = null;

                    if (quality == 0)
                        texture = chuck.Textures[texPos].DefaultQuality;
                    else if (quality == 1)
                        texture = chuck.Textures[texPos].LowQuality;
                    else if (quality == 2)
                        texture = chuck.Textures[texPos].MediumQuality;
                    else if (quality == 3)
                        texture = chuck.Textures[texPos].HighQuality;

                    if (texture == null)
                        continue;

                    renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(texture));
                    cell.transform.SetParent(parent.transform);
                    EditorUtility.SetDirty(cell);
                }
            }

            parent.transform.localScale = new Vector3(-1, 1, 2);
            parent.transform.rotation = Quaternion.Euler(-90, -90, 90);
        }

        private void SetDefaultTexture(string[] paths)
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

                if (!Directory.Exists(AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture")))
                {
                    Directory.CreateDirectory(AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture"));
                    AssetDatabase.Refresh();
                }

                string scriptablePath = AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture/" + chuck.name + "-" + position + ".asset");

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
                chuck.AddTexture(position, lodTexture);
                EditorUtility.SetDirty(lodTexture);
                EditorUtility.SetDirty(chuck);
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

                if (!Directory.Exists(AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture")))
                {
                    Directory.CreateDirectory(AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture"));
                    AssetDatabase.Refresh();
                }

                string scriptablePath = AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture/" + chuck.name + "-" + position + ".asset");

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

                lodTexture.SetLowQualityTexture(texture);
                chuck.AddTexture(position, lodTexture);
                EditorUtility.SetDirty(lodTexture);
                EditorUtility.SetDirty(chuck);
                serializedObject.ApplyModifiedProperties();
            }
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

                if (!Directory.Exists(AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture")))
                {
                    Directory.CreateDirectory(AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture"));
                    AssetDatabase.Refresh();
                }

                string scriptablePath = AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture/" + chuck.name + "-" + position + ".asset");

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
                chuck.AddTexture(position, lodTexture);
                EditorUtility.SetDirty(lodTexture);
                EditorUtility.SetDirty(chuck);
                serializedObject.ApplyModifiedProperties();
            }
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

                if (!Directory.Exists(AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture")))
                {
                    Directory.CreateDirectory(AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture"));
                    AssetDatabase.Refresh();
                }

                string scriptablePath = AssetDatabase.GetAssetPath(chuck).Replace(chuck.name + ".asset", chuck.name + "-Texture/" + chuck.name + "-" + position + ".asset");

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
                chuck.AddTexture(position, lodTexture);
                EditorUtility.SetDirty(lodTexture);
                EditorUtility.SetDirty(chuck);
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }
    }
}