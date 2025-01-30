using System;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    public sealed class JointSlider : Slider, IDisposable
    {
        private readonly TransformJoint _transformJoint;
        private readonly Action<float> _callback;

        public JointSlider(TransformJoint transformJoint, string name, Action<float> callback = null)
        {
            _transformJoint = transformJoint;
            
            label = _transformJoint.Config.Type switch
            {
                TransformJoint.JointType.Rotation => $"{name} [°]",
                TransformJoint.JointType.Displacement => $"{name} [m]",
                _ => throw new ArgumentOutOfRangeException()
            };
            
            lowValue = _transformJoint.Config.Limits.x;
            highValue = _transformJoint.Config.Limits.y;
            showInputField = true;

            _callback = callback;

            _transformJoint.Position.Subscribe(SetValueWithoutNotify);
            
            this.SetDelayed(true);
            this.RegisterValueChangedCallback(ChangeSliderValueCallback);
            this.AlignedField();
        }
        
        public void Dispose()
        {
            _transformJoint.Position.Unsubscribe(SetValueWithoutNotify);
            this.RegisterValueChangedCallback(ChangeSliderValueCallback);
        }
        
        private void ChangeSliderValueCallback(ChangeEvent<float> changeEvent)
        {
            _callback?.Invoke(changeEvent.newValue);
        }
    }
}
