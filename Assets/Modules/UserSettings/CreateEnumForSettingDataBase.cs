#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace com.playbux.settings
{
    public class CreateEnumForSettingDataBase : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                string field = "";
                if (asset.EndsWith("SettingDataBase.cs"))
                {
                    Debug.Log(asset);
                    var settingDataBase = new SettingDataBase();
                    for (int i = 0; i < settingDataBase.GetType().GetProperties().Length; i++)
                    {
                        field += (settingDataBase.GetType().GetProperties()[i].Name) + ",\n";
                    }
                    settingDataBase = null;
                    string directoryPath = Path.GetDirectoryName(asset);

                    string fileName = "SettingVariable";
                    string filePath = Path.Combine(directoryPath, fileName);
                    Debug.Log(filePath);
                    string codeTemplate = File.ReadAllText(filePath + ".txt");
                    string codeProcess = codeTemplate.Replace("{0}", field);
                    Debug.Log(codeProcess);
                    File.WriteAllText(filePath + ".cs", codeProcess);

                    AssetDatabase.Refresh();

                    Debug.Log("Created file: " + filePath);
                }
            }

        }
    }

}
#endif
