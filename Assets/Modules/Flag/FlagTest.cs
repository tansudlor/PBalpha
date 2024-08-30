#if UNITY_EDITOR
using com.playbux.identity;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace com.playbux.flag
{
    public class FlagTest : EditorWindow
    {
        private string id;
        private string flag;
        private string value;
        FlagCollectionBase<string> flagCollection;
        IIdentitySystem identitySystem;
        SignalBus signalBus;
        public FlagTest(IIdentitySystem identitySystem, SignalBus signalBus)
        {
            flagCollection = new FlagCollectionBase<string>(identitySystem,signalBus);
        }

        [MenuItem("Test/Flag")]
        public static void OpenWindow()
        {
            var window = GetWindow<FlagTest>("FlagTest");
            window.minSize = new Vector2(400, 460);
            window.maxSize = new Vector2(400, 460);
            window.ShowUtility();
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Enter ID:", EditorStyles.boldLabel);
            id = EditorGUILayout.TextArea(id, GUILayout.Height(100));
            EditorGUILayout.LabelField("Enter Flag:", EditorStyles.boldLabel);
            flag = EditorGUILayout.TextArea(flag, GUILayout.Height(100));
            EditorGUILayout.LabelField("Enter Value:", EditorStyles.boldLabel);
            value = EditorGUILayout.TextArea(value, GUILayout.Height(100));

            if (GUILayout.Button("Calculate"))
            {
                Debug.Log(flagCollection.Report(id));
            }

            if (GUILayout.Button("Add"))
            {
                flagCollection.SetFlag(id, flag,value,"0");
            }

            if (GUILayout.Button("Remove"))
            {
                flagCollection.Remove(id,flag);
            }

            if (GUILayout.Button("Get"))
            {
                Debug.Log(flagCollection.GetFlag(id,flag));
            }
        }
    }
}
#endif