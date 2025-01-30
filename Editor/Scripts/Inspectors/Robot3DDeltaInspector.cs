using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    [CustomEditor(typeof(Robot3DDelta), true)]
    public class Robot3DDeltaInspector : UnityEditor.Editor 
    {
        private JointFoldout _jointFoldout;
        
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            var robot = target as Robot;
            if (robot == null) return container;
            
            _jointFoldout = new JointFoldout(robot.Joints);
            
            container.Add(_jointFoldout);
            container.Add(CreateFrameFoldout(robot));
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

        private Foldout CreateFrameFoldout(IRobot robot)
        {
            var foldout = new Foldout
            {
                text = "Frames References"
            };

            var baseFrameField = new ObjectField("Base")
            {
                objectType = typeof(Frame)
            };
            baseFrameField.BindProperty(serializedObject.FindProperty("_frames").GetArrayElementAtIndex(0));
            baseFrameField.AlignedField();
            foldout.Add(baseFrameField);

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    var frameField = new ObjectField($"Frame_{i + 1}{j}") { objectType = typeof(Frame) };
                    frameField.BindProperty(serializedObject.FindProperty("_frames").GetArrayElementAtIndex(1 + i * 3 + j));
                    frameField.AlignedField();
                    foldout.Add(frameField); 
                }
            }
            
            for (var i = 0; i < 3; i++)
            {
                var frameField = new ObjectField($"Flange_{i + 1}") { objectType = typeof(Frame) };
                frameField.BindProperty(serializedObject.FindProperty("_frames").GetArrayElementAtIndex(robot.Frames.Count-1-3+i));
                frameField.AlignedField();
                foldout.Add(frameField);
            }
            var flangeFrameField = new ObjectField("Flange")
            {
                objectType = typeof(Frame)
            };
            flangeFrameField.BindProperty(serializedObject.FindProperty("_frames").GetArrayElementAtIndex(robot.Frames.Count-1));
            flangeFrameField.AlignedField();
            foldout.Add(flangeFrameField);

            return foldout;
        }
    }
}
