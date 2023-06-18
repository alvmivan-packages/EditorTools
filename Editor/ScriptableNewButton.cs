using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    /// <summary>
    /// This class will add a "New" button to create a new ScriptableObject of type TScriptableType,
    /// set to the property, and persist it to the asset database.
    /// </summary>
    public abstract class ScriptableNewButton<TScriptableType> : PropertyDrawer where TScriptableType : ScriptableObject
    {
        protected virtual int ButtonWidth => 60;
        protected virtual int ButtonPadding => 2;
        protected virtual string ButtonText => "New";
        protected virtual bool PingNewObject => false;
        protected virtual bool ShowButtonWhenNotNull => false;

        const string SavePromptTitleFormat = "Save New {0}"; // 0 = Scriptable Type Name
        const string SavePromptMessageFormat = "Save new {0} to:"; // 0 = Scriptable Type Name
        const string SavePromptDefaultNameFormat = "New {0}"; // 0 = Scriptable Type Name
        const string Extension = "asset";

        static readonly EditorPrefString LastPath = $"ScriptableNewButton_{typeof(TScriptableType).Name}_LastPath";

        static string SavePromptTitle => string.Format(SavePromptTitleFormat, typeof(TScriptableType).Name);
        static string SavePromptMessage => string.Format(SavePromptMessageFormat, typeof(TScriptableType).Name);
        static string SavePromptDefaultName => string.Format(SavePromptDefaultNameFormat, typeof(TScriptableType).Name);

        //button will have a ButtonWidth width and the same height as the property and will be at the right of the property with a ButtonPadding padding 
        Rect GetButtonRect(Rect fullRect) => new Rect(fullRect.x + fullRect.width - ButtonWidth - ButtonPadding,
            fullRect.y, ButtonWidth, fullRect.height);

        Rect GetPropertyRect(Rect fullRect) => new Rect(fullRect.x, fullRect.y,
            fullRect.width - ButtonWidth - ButtonPadding, fullRect.height);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                //display warning if the property is not an object reference
                EditorGUI.HelpBox(position,
                    "ScriptableNewButton only works with object references (Scriptable Objects)", MessageType.Warning);
                return;
            }

            if (!ShowButtonWhenNotNull && property.objectReferenceValue != null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var buttonRect = GetButtonRect(position);
            var propertyRect = GetPropertyRect(position);

            EditorGUI.PropertyField(propertyRect, property, label);
            if (GUI.Button(buttonRect, ButtonText))
            {
                CreateObject(property);
            }
        }


        async void CreateObject(SerializedProperty property)
        {
            var asset = ScriptableObject.CreateInstance<TScriptableType>();
            property.objectReferenceValue = asset;
            property.serializedObject.ApplyModifiedProperties();


            /*
             * Important note:
             * The Task.Yield() method is used here to ensure that the property is updated before the SaveFilePanelInProject
             * and also the OnGUI method is finished before the SaveFilePanelInProject is called.
             * And Avoid Stack Errors on the drawer.
             */
            await Task.Yield();


            LastPath.Value = EditorUtility.SaveFilePanelInProject(SavePromptTitle, SavePromptDefaultName, Extension,
                SavePromptMessage, LastPath.Value);

            if (string.IsNullOrEmpty(LastPath.Value)) return;

            AssetDatabase.CreateAsset(asset, LastPath.Value);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!PingNewObject) return;
            
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            EditorUtility.FocusProjectWindow();
        }
    }
}