#if UNITY_EDITOR
using FireworksMania.Core.Attributes;
using FireworksMania.Core.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FireworksMania.Core.Editor.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Events;

namespace FireworksMania.Core.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(GameSoundAttribute))]
    public class GameSoundDrawer : PropertyDrawer
    {
        private int selectedIndex = 0;
        private List<string> _selectableSoundItems;
        private HashSet<string> _soundOptions = new HashSet<string>();
        private string temp;
        private UnityEvent<string> callBack = new UnityEvent<string>();
        private SerializedProperty property1;

        public GameSoundDrawer()
        {
            PopulateFromGameSoundCollections();
            callBack.AddListener(WriteChanges);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property1 = property;
            //if (temp != null)
            //{
            //    property.stringValue = temp;
            //}
            Rect button = new Rect(position.x + 200, position.y, position.width - 200, position.height);

            GUI.Label(position, label);

            //Debug.Log("Current Property = " + property.stringValue);

            if (GUI.Button(button, property.stringValue, EditorStyles.popup))
            {
                StringListSearchProvider provider = ScriptableObject.CreateInstance<StringListSearchProvider>();
                provider.setItems(_selectableSoundItems.ToArray());
                provider.setCallback(callBack);
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
            }
            EditorGUI.EndProperty();
            //property.serializedObject.ApplyModifiedProperties();
            //write to undo finsihed
        }

        private void WriteChanges(string value)
        {
            property1.stringValue = value;
            property1.serializedObject.Update();
            property1.serializedObject.ApplyModifiedProperties();
        }


        private void PopulateFromGameSoundCollections()
        {
            _soundOptions.Clear();

            var soundCollections = AssetDatabaseHelper.FindAssetsByType<SoundCollection>();

            foreach (var foundGameSoundCollectionItem in soundCollections)
            {
                foreach (var soundItem in foundGameSoundCollectionItem.Sounds)
                {
                    if (_soundOptions.Contains(soundItem) == false)
                        _soundOptions.Add(soundItem);
                }
            }

            var gameSoundDefinitions = AssetDatabaseHelper.FindAssetsByType<GameSoundDefinition>();
            foreach (var gameSoundDef in gameSoundDefinitions)
            {
                if (gameSoundDef != null)
                {
                    if (_soundOptions.Contains(gameSoundDef.name) == false)
                        _soundOptions.Add(gameSoundDef.name);
                }
            }

            _selectableSoundItems = _soundOptions.ToList();
            _selectableSoundItems.Sort(StringComparer.OrdinalIgnoreCase);
        }
    }
}
#endif