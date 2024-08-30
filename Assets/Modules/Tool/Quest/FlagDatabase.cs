#if UNITY_EDITOR
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.tool
{
    public class FlagDatabase 
    {
        [JsonProperty("version")]
        public int FlagVersion { get => flagVersion; set => flagVersion = value; }
        [JsonProperty("flags")]
        public List<AllFlag> AllFlags { get => allFlags; set => allFlags = value; }

        [SerializeField]
        private int flagVersion;
        [SerializeField]
        private List<AllFlag> allFlags ;
        


        [System.Serializable]
        public class AllFlag
        {
            [SerializeField]
            private string flag;
            [SerializeField]
            private string reward;
            

            [JsonProperty("flag")]
            public string Flag { get => flag; set => flag = value; }
            [JsonProperty("reward")]
            public string Reward { get => reward; set => reward = value; }
            
            
        }

        

        public static void ScriptToJson(FlagDatabase flagDatabase)
        {
            if (flagDatabase == null)
            {
                Debug.LogError("flagDataBase is not assigned!");
                return;
            }
            string jsonString = JsonUtility.ToJson(flagDatabase, true);

            // Specify the export path (you can change this)
            string exportPath = "Assets/Resources/FlagToJson.json";

            // Write the JSON string to a file
            System.IO.File.WriteAllText(exportPath, jsonString);

            Debug.Log("Exported to: " + exportPath);
        }
    }
}
#endif