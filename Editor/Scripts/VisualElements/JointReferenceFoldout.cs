using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    public class JointReferenceFoldout : Foldout
    {
        public JointReferenceFoldout(IRobot robot, SerializedObject serializedObject)
        {
            text = "Joints References";

            for (var i = 0; i < robot.Joints.Count; i++)
            {
                var frameField = new ObjectField($"Joint_{i}")
                {
                    objectType = typeof(TransformJoint)
                };
                frameField.BindProperty(serializedObject.FindProperty("_joints").GetArrayElementAtIndex(i));
                frameField.AlignedField();
                Add(frameField);
            }
        }
    }
}
