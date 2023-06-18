using UnityEditor;

namespace EditorTools
{
    public class EditorPrefString
    {
        readonly string key;
        readonly string defaultValue;

        public EditorPrefString(string key, string defaultValue = "")
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        public string Value
        {
            get => UnityEditor.EditorPrefs.GetString(key, defaultValue);
            set => UnityEditor.EditorPrefs.SetString(key, value);
        }

        public void DrawStringField() => Value = EditorGUILayout.TextField(key, Value);

        public static implicit operator EditorPrefString(string key) => new(key);
    }
}