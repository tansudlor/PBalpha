using UnityEditor;
using UnityEngine;

namespace com.playbux.utilis.scriptable
{
    public static class ScriptableEditorDrawer
    {
        public static void DrawToggleButton(this ref bool toggleFlag, string label)
        {
            EditorGUILayout.BeginVertical("Box");
            toggleFlag = EditorGUILayout.ToggleLeft(label, toggleFlag, EditorStyles.toolbarButton);
            EditorGUILayout.EndVertical();
        }

        public static void Space(this int size)
        {
            EditorGUILayout.Space(size);
        }

        public static void DrawHeader(this string value, GUIStyle styles = null)
        {
            styles ??= EditorStyles.largeLabel;
            EditorGUILayout.BeginVertical("Button");
            GUILayout.Label(value, styles);
            EditorGUILayout.EndVertical();
        }

        public static void DrawSubHeader(this string value, GUIStyle styles = null)
        {
            styles ??= EditorStyles.miniLabel;
            EditorGUILayout.BeginVertical("Toolbar");
            GUILayout.Label(value, styles);
            EditorGUILayout.EndVertical();
        }

        public static int DrawIntField(this ref int number)
        {
            return EditorGUILayout.IntSlider(number, 1, 100);
        }

        public static Vector2 DrawVector2Field(this Vector2 value)
        {
            return EditorGUILayout.Vector2Field("", value);
        }

        public static Vector3 DrawVector3Field(this Vector3 value)
        {
            return EditorGUILayout.Vector3Field("", value);
        }

        public static T DrawObjectField<T>(this T value, bool allowSceneObjects = false) where T : Object
        {
            return (T)EditorGUILayout.ObjectField(value, typeof(T), allowSceneObjects);
        }

        public static string DrawTextField(this string value, GUIStyle styles = null)
        {
            styles ??= EditorStyles.textField;
            return EditorGUILayout.TextField(value, styles);
        }

        public static void DrawButton(this ref bool value, string label, GUIStyle styles = null)
        {
            styles ??= EditorStyles.miniButtonMid;
            value = GUILayout.Button(label, styles);
        }

        public static string DrawSearchBox(this string keyword, GUIStyle styles = null)
        {
            styles ??= EditorStyles.toolbarSearchField;
            return EditorGUILayout.TextField(keyword, styles);
        }
    }
}
