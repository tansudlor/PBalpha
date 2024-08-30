#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.avatar
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


    [CreateAssetMenu(menuName = "Playbux/Avatar/AvatarPartsCollection", fileName = "AvatarPartsCollection")]
    [System.Serializable]
    public class AvatarPartsCollection : ScriptableObject
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
    [CustomEditor(typeof(AvatarPartsCollection))]
    public class AvatarPartsCollectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AvatarPartsCollection collection = (AvatarPartsCollection)target;
            if (GUILayout.Button("Rename(for {_1-_4} - {_0 -_3 }"))
            {
                DirectoryInfo dir = new DirectoryInfo(collection.BaseFolder + collection.TargetFolder);
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (var d in dirs)
                {
                    FileInfo[] files = d.GetFiles();
                    foreach (var f in files)
                    {
                        if (f.Name.IndexOf("_4") > -1)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                var oldFile = Path.Combine(f.DirectoryName, f.Name.Replace("_4", "_" + (i + 1)));
                                var newFile = Path.Combine(f.DirectoryName, f.Name.Replace("_4", "_" + (i)));
                                Debug.Log(oldFile + " --> " + newFile);
                                File.Move(oldFile, newFile);
                            }
                        }
                    }
                }
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Reassign from Folder [Bux Parts]"))
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
