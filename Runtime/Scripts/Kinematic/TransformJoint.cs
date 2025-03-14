using System;
using System.Collections.Generic;
using UnityEngine;

namespace Preliy.Flange
{
    [ExecuteAlways]
    [AddComponentMenu("Robotics/Transform Joint")]
    [RequireComponent(typeof(Frame))]
    [DisallowMultipleComponent]
    public class TransformJoint : MonoBehaviour, IEqualityComparer<TransformJoint>
    {
        public IProperty<float> Position => _position;
        
        public Frame Frame => _frame ??= GetComponent<Frame>();

        public JointConfig Config
        {
            get => _config;
            set => _config = value;
        }

        [SerializeField]
        private Property<float> _position = new();

        [SerializeField]
        private JointConfig _config = JointConfig.Default;

        private Frame _frame;

        private void OnEnable()
        {
            _position.Subscribe(SetValue);
        }

        private void OnDisable()
        {
            _position.Unsubscribe(SetValue);
        }

        private void OnValidate()
        {
            _position.OnValidate();
        }

        private void SetValue(float value)
        {
            var target = _config.GetValidValue(value);

            switch (_config.Type)
            {
                case JointType.Rotation:
                    transform.SetLocalMatrix(HomogeneousMatrix.Create(Frame.Config, target * Mathf.Deg2Rad));
                    break;
                case JointType.Displacement:
                    transform.SetLocalMatrix(HomogeneousMatrix.Create(Frame.Config, 0f, target));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public enum JointType
        {
            Rotation,
            Displacement
        }
        
        public bool Equals(TransformJoint x, TransformJoint y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return Equals(x.name, y.name);
        }
        
        public int GetHashCode(TransformJoint obj)
        {
            return HashCode.Combine(obj._position, obj._config, obj._frame);
        }
    }
}
