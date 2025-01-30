using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Preliy.Flange
{
    public static class GizmosUtils
    {
        private const float DEFAULT_HANDLE_SCALE = 0.1f;

        public static void DrawHandle(Matrix4x4 matrix, float scale = 1f)
        {
            if (!matrix.ValidTRS()) return;
            var position = matrix.GetPosition();
            var rotation = matrix.rotation;
            
            Handles.color = Handles.xAxisColor;
            Handles.ArrowHandleCap(
                0,
                position,
                rotation * Quaternion.LookRotation(Vector3.right),
                DEFAULT_HANDLE_SCALE * scale,
                EventType.Repaint
            );
            Handles.color = Handles.yAxisColor;
            Handles.ArrowHandleCap(
                0,
                position,
                rotation * Quaternion.LookRotation(Vector3.up),
                DEFAULT_HANDLE_SCALE * scale,
                EventType.Repaint
            );
            Handles.color = Handles.zAxisColor;
            Handles.ArrowHandleCap(
                0,
                position,
                rotation * Quaternion.LookRotation(Vector3.forward),
                DEFAULT_HANDLE_SCALE * scale,
                EventType.Repaint
            );
        }
        
        public static void DrawFrameOffset(Frame frame, Color pointColor, Color lineColor, float scale = 1f)
        {
            var style = new GUIStyle
            {
                normal =
                {
                    textColor = lineColor
                }
            };

            var handleSize = HandleUtility.GetHandleSize(frame.transform.position);
            var pivotPoint = Vector3.back * frame.Config.A;
            var parentPoint = Quaternion.Inverse(frame.transform.localRotation) * -frame.transform.localPosition;
            
            Handles.color = lineColor;
            
            if (Mathf.Abs(frame.Config.A) > 1e-5)
            {
                Handles.DrawLine(Vector3.zero, pivotPoint, 1);
                Handles.Label(pivotPoint * 0.5f, frame.Config.A.ToString("F3"), style);
            }
            
            if (Mathf.Abs(frame.Config.D) > 1e-5)
            {
                Handles.DrawLine(pivotPoint, parentPoint, 1);
                Handles.Label((pivotPoint + parentPoint) * 0.5f, frame.Config.D.ToString("F3"), style);
            }
            
            Gizmos.color = pointColor;
            Gizmos.DrawSphere(Vector3.zero,  handleSize * 0.05f * scale);
        }
    }
}
#endif
