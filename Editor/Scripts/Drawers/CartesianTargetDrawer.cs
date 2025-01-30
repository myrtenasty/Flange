using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    [CustomPropertyDrawer(typeof(CartesianTarget), true)]
    public class CartesianTargetDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var fieldPosition = new Vector3Field("Position [mm]").AlignedField();
            var fieldRotation = new Vector3Field("Rotation [deg]").AlignedField();
            var fieldConfiguration = new PropertyField(property.FindPropertyRelative("_configuration"));
            var fieldExtJoint = new PropertyField(property.FindPropertyRelative("_extJoint"));

            var poseProperty = property.FindPropertyRelative("_pose");
            var pose = GetMatrixPropertyValue(poseProperty);
            
            fieldPosition.SetValueWithoutNotify(pose.GetPosition());
            fieldRotation.SetValueWithoutNotify(pose.rotation.eulerAngles);

            fieldPosition.RegisterValueChangedCallback(
                evt => 
                {
                    pose = pose.SetPosition(evt.newValue);
                    SetMatrixPropertyValue(poseProperty, pose);
                }
            );
            
            fieldRotation.RegisterValueChangedCallback(
                evt => 
                {
                    pose = pose.SetRotation(evt.newValue);
                    SetMatrixPropertyValue(poseProperty, pose);
                }
            );

            container.Add(fieldPosition);
            container.Add(fieldRotation);
            container.Add(fieldConfiguration);
            container.Add(fieldExtJoint);

            container.Bind(property.serializedObject);
            
            return container;
        }

        private Matrix4x4 GetMatrixPropertyValue(SerializedProperty property)
        {
            var result = Matrix4x4.identity;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    result[i, j] = property.FindPropertyRelative("e" + i + j).floatValue;
                }
            }
            return result;
        }

        private void SetMatrixPropertyValue(SerializedProperty property, Matrix4x4 value)
        {
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    property.FindPropertyRelative("e" + i + j).floatValue = value[i, j];
                }
            }

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
