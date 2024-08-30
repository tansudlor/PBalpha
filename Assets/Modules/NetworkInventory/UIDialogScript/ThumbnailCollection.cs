using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace com.playbux.networking.networkinventory
{
    [System.Serializable]
    public class PartPathMaping
    {
        public string Name;
        public System.Type Type;
        public string Path;
        public Object Data;
        public string Info;
        public string BaseFolder;
        public PartPathMaping(string name, System.Type type, string path, Object data, string info, string baseFolder)
        {
            this.Name = name;
            this.Type = type;
            this.Path = path;
            this.Data = data;
            this.Info = info;
            this.BaseFolder = baseFolder;
        }
    }

    [System.Serializable]
    public class DisplayName
    {
        public string Name;
        public DisplayName(string name)
        {
            this.Name = name;
        }
    }


    [CreateAssetMenu(menuName = "Playbux/Inventory/ThumbnailCollection", fileName = "ThumbnailCollection")]
    [System.Serializable]
    public class ThumbnailCollection : ScriptableObject
    {
        public string BaseFolder;
        public string TargetFolder;
        public string TargetFilter;
        public List<PartPathMaping> FileList;
        public Dictionary<string, PartPathMaping> Map
        {
            get
            {
                Dictionary<string, PartPathMaping> map = new Dictionary<string, PartPathMaping>();
                foreach (var item in FileList)
                {
                    map[item.Path.Replace(@"\", @"/").ToLower()] = item;
                }
                return map;
            }
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(ThumbnailCollection))]
    public class ThumbnailCollectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ThumbnailCollection collection = (ThumbnailCollection)target;
            if (GUILayout.Button("Reassign Thumbnail Collection"))
            {

                collection.FileList = new List<PartPathMaping>();
                // Find all asset GUIDs in the folder.
                string[] guids = AssetDatabase.FindAssets("", new[] { collection.BaseFolder + collection.TargetFolder });
                foreach (string guid in guids)
                {
                    // Convert the GUID to an asset path.
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    // Load the asset.
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    if (assetPath.IndexOf(collection.TargetFilter) > -1)
                    {
                        var split = assetPath.Split('/');
                        string assetPathWithoutExtension = Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath));
                        PartPathMaping assetpath = new PartPathMaping(split[split.Length - 2] + " : <" + asset.GetType().ToString() + ">" + asset.name, asset.GetType(), assetPathWithoutExtension, asset, asset.GetType().ToString(), collection.BaseFolder);
                        collection.FileList.Add(assetpath);
                    }
                }
                EditorUtility.SetDirty(collection);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            DrawDefaultInspector();
        }

    }
#endif


}
