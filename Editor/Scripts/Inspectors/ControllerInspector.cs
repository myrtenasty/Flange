using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    [CustomEditor(typeof(Controller))]
    public class ControllerInspector : UnityEditor.Editor
    {
        private Matrix4x4 _target;
        private Matrix4x4 _handle;
        private bool _handleIsHot;
        private int _handleId;
        private Controller _controller;
        private ControllerInspectorPanel _controllerInspectorPanel;

        private void OnEnable()
        {
            _controller = target as Controller;
            if (_controller != null)
            {
                _controller.PoseObserver.ToolCenterPointFrame.Subscribe(GetTarget); 
            }
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override VisualElement CreateInspectorGUI()
        {
            _controllerInspectorPanel = new ControllerInspectorPanel();
            _controllerInspectorPanel.Bind(_controller, serializedObject);
            return _controllerInspectorPanel;
        }

        private void OnDisable()
        {
            _controllerInspectorPanel?.Dispose();
            _controller.PoseObserver.ToolCenterPointFrame.Unsubscribe(GetTarget);
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void GetTarget(Matrix4x4 matrix)
        {
            _target = matrix;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (Application.isPlaying) return;
            if (_controller.gameObject.activeInHierarchy && EditorPrefs.GetBool("ControllerShowHandles")) Handle();
        }

        private void Handle()
        {
            if (!_target.ValidTRS()) return;
            
            SceneToolUtils.Snap();
            EditorGUI.BeginChangeCheck();

            Handles.matrix = _controller.GetFrameActual().GetWorldFrame();

            switch (Tools.pivotRotation)
            {
                case PivotRotation.Local:
                    _handle = GetHandleLocal(_target);
                    break;
                case PivotRotation.Global:
                    _handle = GetHandleGlobal(_target);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (EditorGUI.EndChangeCheck())
            {
                _target = _handle;
                Undo.RecordObject(_controller, $"Pose changed {_target}");
                _controller.Solver.TryJumpToTarget(_target, SolutionIgnoreMask.None, false);

                if (!_handleIsHot && GUIUtility.hotControl != 0)
                {
                    _handleIsHot = true;
                }
            }

            var tcp = _controller.PoseObserver.ToolCenterPointFrame.Value;
            
            if (_handleIsHot && GUIUtility.hotControl == 0)
            {
                _handleIsHot = false;
                _target = tcp;
            }

            DrawOffsetLine(_target, tcp);
            GizmosUtils.DrawHandle(Matrix4x4.identity, 0.2f);
        }

        private static Matrix4x4 GetHandleLocal(Matrix4x4 handle)
        {
            var position = handle.GetPosition();
            var rotation = handle.rotation;

            switch (Tools.current)
            {
                case UnityEditor.Tool.Move:
                {
                    var newPosition = Handles.PositionHandle(position, rotation);
                    return Matrix4x4.TRS(newPosition, rotation, Vector3.one);
                }
                case UnityEditor.Tool.Rotate:
                {
                    var newRotation = Handles.RotationHandle(rotation, position);
                    return Matrix4x4.TRS(position, newRotation, Vector3.one);
                }
                case UnityEditor.Tool.Transform:
                {
                    Handles.TransformHandle(ref position, ref rotation);
                    return Matrix4x4.TRS(position, rotation, Vector3.one);
                }
                default:
                    return Matrix4x4.zero;
            }
        }

        private Matrix4x4 GetHandleGlobal(Matrix4x4 handle)
        {
            var position = handle.GetPosition();
            var rotation = handle.rotation;
            var globalRotation = Quaternion.Inverse(_controller.GetFrameActual().GetWorldFrame().rotation);

            switch (Tools.current)
            {
                case UnityEditor.Tool.Move:
                {
                    var newPosition = Handles.PositionHandle(position, globalRotation);
                    return Matrix4x4.TRS(newPosition, rotation, Vector3.one);
                }
                case UnityEditor.Tool.Rotate:
                {
                    var newRotation = Handles.RotationHandle(rotation, position);
                    return Matrix4x4.TRS(position, newRotation, Vector3.one);
                }
                case UnityEditor.Tool.Transform:
                {
                    Handles.TransformHandle(ref position, ref globalRotation);
                    return Matrix4x4.TRS(position, rotation, Vector3.one);
                }
                default:
                    return Matrix4x4.zero;
            }
        }

        private void DrawOffsetLine(Matrix4x4 m0, Matrix4x4 m1)
        {
            var distance = Vector3.Distance(m0.GetPosition(), m1.GetPosition());
            if (distance < 1e-3) return;

            GizmosUtils.DrawHandle(_controller.GetTcpRelativeToRefFrame(), scale: 0.05f);
            Handles.color = Color.yellow;
            Handles.SphereHandleCap(0, m0.GetPosition(), m0.rotation, 0.01f, EventType.Repaint);
            Handles.SphereHandleCap(0, m1.GetPosition(), m1.rotation, 0.01f, EventType.Repaint);
            Handles.DrawLine(m0.GetPosition(), m1.GetPosition(), 2);
        }
    }
}
