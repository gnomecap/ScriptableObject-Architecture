using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace ScriptableObjectArchitecture.Editor
{
    public abstract class BaseGameEventEditor : UnityEditor.Editor
    {
        private IStackTraceObject Target { get { return (IStackTraceObject)target; } }

        private StackTrace _stackTrace;
        private List<MonoBehaviour> _referencingGameObjects = new List<MonoBehaviour>();

        protected abstract void DrawRaiseButton();

        protected virtual void OnEnable()
        {
            _stackTrace = new StackTrace(Target);
            _stackTrace.OnRepaint.AddListener(Repaint);
            FindReferencingGameObjects();
        }

        public override void OnInspectorGUI()
        {
            DrawRaiseButton();

            if (!SOArchitecturePreferences.IsDebugEnabled)
                EditorGUILayout.HelpBox("Debug mode disabled\nStack traces will not be filed on raise!", MessageType.Warning);

            _stackTrace.Draw();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Referencing Game Objects", EditorStyles.boldLabel);
            foreach (var go in _referencingGameObjects)
            {
                EditorGUILayout.ObjectField(go, typeof(MonoBehaviour), true);
            }
        }

        private void FindReferencingGameObjects()
        {
            _referencingGameObjects.Clear();
            GameEventBase gameEvent = target as GameEventBase;

            if (gameEvent == null)
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
                        if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == gameEvent)
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