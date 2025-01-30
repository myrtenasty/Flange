using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    [CustomEditor(typeof(MechanicalUnit), true)]
    public class MechanicalUnitInspector : UnityEditor.Editor
    {
        private JointFoldout _jointFoldout;
        
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            var mechanicalUnit = target as MechanicalUnit;
            if (mechanicalUnit == null) return container;

            _jointFoldout = new JointFoldout(mechanicalUnit.Joints);

            container.Add(_jointFoldout);
            container.Add(new PropertyField(serializedObject.FindProperty("_frames")));
            container.Add(new PropertyField(serializedObject.FindProperty("_joints")));
            container.Add(new PropertyField(serializedObject.FindProperty("_cartesianLimit")));
            container.Add(new PropertyField(serializedObject.FindProperty("_showGizmos")));
            container.Add(new PropertyField(serializedObject.FindProperty("_gizmosConfig")));
            container.Add(new PropertyField(serializedObject.FindProperty("_debug")));
            return container;
        }
        
        private void OnDisable()
        {
            _jointFoldout?.Dispose();
        }
    }
}
