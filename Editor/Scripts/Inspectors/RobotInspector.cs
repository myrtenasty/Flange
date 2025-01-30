using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    [CustomEditor(typeof(Robot), true)]
    public class RobotInspector : UnityEditor.Editor
    {
        private JointFoldout _jointFoldout;
        
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            var robot = target as Robot;
            if (robot == null) return container;
            
            _jointFoldout = new JointFoldout(robot.Joints);
            
            container.Add(_jointFoldout);
            container.Add(new FrameReferenceFoldout(robot, serializedObject));
            container.Add(new JointReferenceFoldout(robot, serializedObject));
            container.Add(new PropertyField(serializedObject.FindProperty("_cartesianLimit")));
            container.Add(new PropertyField(serializedObject.FindProperty("_showGizmos")));
            container.Add(new PropertyField(serializedObject.FindProperty("_gizmosConfig")));
            return container;
        }

        private void OnDisable()
        {
            _jointFoldout?.Dispose();
        }
    }
}
