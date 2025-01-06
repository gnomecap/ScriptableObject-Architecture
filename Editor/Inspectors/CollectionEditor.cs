using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

namespace ScriptableObjectArchitecture.Editor
{
    [CustomEditor(typeof(BaseCollection), true)]
    public class CollectionEditor : UnityEditor.Editor
    {
        private BaseCollection Target { get { return (BaseCollection)target; } }
        private SerializedProperty CollectionItemsProperty
        {
            get { return serializedObject.FindProperty(LIST_PROPERTY_NAME);}
        }

        private ReorderableList _reorderableList;
        private List<MonoBehaviour> _referencingGameObjects = new List<MonoBehaviour>();

        // UI
        private const bool DISABLE_ELEMENTS = false;
        private const bool ELEMENT_DRAGGABLE = true;
        private const bool LIST_DISPLAY_HEADER = true;
        private const bool LIST_DISPLAY_ADD_BUTTON = true;
        private const bool LIST_DISPLAY_REMOVE_BUTTON = true;

        private GUIContent _titleGUIContent;
        private GUIContent _noPropertyDrawerWarningGUIContent;

        private const string TITLE_FORMAT = "List ({0})";
        private const string NO_PROPERTY_WARNING_FORMAT = "No PropertyDrawer for type [{0}]";

        // Property Names
        private const string LIST_PROPERTY_NAME = "_list";

        private void OnEnable()
        {
            _titleGUIContent = new GUIContent(string.Format(TITLE_FORMAT, Target.Type));
            _noPropertyDrawerWarningGUIContent = new GUIContent(string.Format(NO_PROPERTY_WARNING_FORMAT, Target.Type));

            _reorderableList = new ReorderableList(
                serializedObject,
                CollectionItemsProperty,
                ELEMENT_DRAGGABLE,
                LIST_DISPLAY_HEADER,
                LIST_DISPLAY_ADD_BUTTON,
                LIST_DISPLAY_REMOVE_BUTTON)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                elementHeightCallback = GetHeight,
            };

            FindReferencingGameObjects();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            _reorderableList.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Referencing Game Objects", EditorStyles.boldLabel);
            foreach (var go in _referencingGameObjects)
            {
                EditorGUILayout.ObjectField(go, typeof(GameObject), true);
            }
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, _titleGUIContent);
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect = SOArchitecture_EditorUtility.GetReorderableListElementFieldRect(rect);
            SerializedProperty property = CollectionItemsProperty.GetArrayElementAtIndex(index);

            EditorGUI.BeginDisabledGroup(DISABLE_ELEMENTS);

            GenericPropertyDrawer.DrawPropertyDrawer(rect, property, Target.Type);

            EditorGUI.EndDisabledGroup();
        }

        private float GetHeight(int index)
        {
            SerializedProperty property = CollectionItemsProperty.GetArrayElementAtIndex(index);

            return GenericPropertyDrawer.GetHeight(property, Target.Type) + EditorGUIUtility.standardVerticalSpacing;
        }

        private void FindReferencingGameObjects()
        {
            _referencingGameObjects.Clear();
            BaseCollection baseCollection = target as BaseCollection;

            if (baseCollection == null)
                return;

            foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
            {
                MonoBehaviour[] components = go.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    SerializedObject so = new SerializedObject(component);
                    SerializedProperty prop = so.GetIterator();
                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == baseCollection)
                        {
                            _referencingGameObjects.Add(component);
                            break;
                        }
                    }
                }
            }
        }
    }
}