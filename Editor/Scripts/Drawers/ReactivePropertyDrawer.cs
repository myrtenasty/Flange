using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    [CustomPropertyDrawer(typeof(IProperty), true)]
    public class ReactivePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            var valueField = new PropertyField(property.FindPropertyRelative("_value"));
            container.Add(valueField);
            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("_value"), label);
        }
    }
}
