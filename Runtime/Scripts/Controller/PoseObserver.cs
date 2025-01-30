using System;
using UnityEngine;

namespace Preliy.Flange
{
    [Serializable]
    public class PoseObserver
    {
        public IProperty<Matrix4x4> Flange => _flange;
        public IProperty<Matrix4x4> ToolCenterPointBase => _toolCenterPointBase;
        public IProperty<Matrix4x4> ToolCenterPointWorld => _toolCenterPointWorld;
        public IProperty<Matrix4x4> ToolCenterPointFrame => _toolCenterPointFrame;
        
        private readonly Controller _controller;
        [SerializeField]
        private Property<Matrix4x4> _flange = new(); 
        [SerializeField]
        private Property<Matrix4x4> _toolCenterPointBase = new();
        [SerializeField]
        private Property<Matrix4x4> _toolCenterPointWorld = new();
        [SerializeField]
        private Property<Matrix4x4> _toolCenterPointFrame = new();

        public event Action OnPoseChanged;
        
        public PoseObserver(Controller controller)
        {
            _controller = controller;
            _controller.MechanicalGroup.OnJointStateChanged += OnJointStateChanged;
            _controller.Tool.Subscribe(OnToolIndexChanged);
            _controller.Frame.Subscribe(OnFrameIndexChanged);
            OnJointStateChanged();
        }

        public void Dispose()
        {
            if (_controller == null) return;
            _controller.MechanicalGroup.OnJointStateChanged -= OnJointStateChanged;
            _controller.Tool.Unsubscribe(OnToolIndexChanged);
            _controller.Frame.Unsubscribe(OnFrameIndexChanged);
        }
        
        private void OnJointStateChanged()
        {
            if (!_controller.IsValid.Value) return;
            _flange.Value = _controller.MechanicalGroup.ComputeForward();
            Refresh();
            OnPoseChanged?.Invoke();
        }
        
        private void OnToolIndexChanged(int toolIndex)
        {
            Refresh();
            OnPoseChanged?.Invoke();
        }
        
        private void OnFrameIndexChanged(int frameIndex)
        {
            Refresh();
            OnPoseChanged?.Invoke();
        }

        private void Refresh()
        {
            _toolCenterPointBase.Value = _controller.AddToolOffset(_flange.Value, _controller.Tool.Value);
            _toolCenterPointWorld.Value = _controller.FrameToWorld(_toolCenterPointBase.Value, (int)CoordinateSystem.Base, _controller.MechanicalGroup.JointState.ExtJoint);
            _toolCenterPointFrame.Value = _controller.WorldToFrame(_toolCenterPointWorld.Value, _controller.Frame.Value, _controller.MechanicalGroup.JointState.ExtJoint);
        }
    }
}
