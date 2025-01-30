using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    public class FrameReferenceFoldout : Foldout
    {
        public FrameReferenceFoldout(IRobot robot, SerializedObject serializedObject)
        {
            text = "Frames References";
            
            var baseFrameField = new ObjectField("Base")
            {
                objectType = typeof(Frame)
            };
            baseFrameField.BindProperty(serializedObject.FindProperty("_frames").GetArrayElementAtIndex(0));
            baseFrameField.AlignedField();
            Add(baseFrameField);

            for (var i = 1; i < robot.Frames.Count-1; i++)
            {
                var frameField = new ObjectField($"Frame_{i}")
                {
                    objectType = typeof(Frame)
                };
                frameField.BindProperty(serializedObject.FindProperty("_frames").GetArrayElementAtIndex(i));
                frameField.AlignedField();
                Add(frameField);
            }
            
            var flangeFrameField = new ObjectField("Flange")
            {
                objectType = typeof(Frame)
            };
            flangeFrameField.BindProperty(serializedObject.FindProperty("_frames").GetArrayElementAtIndex(robot.Frames.Count-1));
            flangeFrameField.AlignedField();
            Add(flangeFrameField);
        }
    }
}
