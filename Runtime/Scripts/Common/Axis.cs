using System;
using UnityEngine;

namespace Preliy.Flange.Common
{
    public class Axis : MonoBehaviour
    {
        public IProperty<float> Target => _target;
        public IProperty<float> Value => _value;

        public AxisMotionType MotionType
        {
            get => _motionType;
            set => _motionType = value;
        }
        
        public AxisControlMode ControlMode
        {
            get => _controlMode;
            set => _controlMode = value;
        }
        
        public Vector3 Direction
        {
            get => _direction;
            set => _direction = value;
        }
        
        public float Factor
        {
            get => _factor;
            set => _factor = value;
        }
        
        [Header("Control")]
        [SerializeField]
        private Property<float> _target = new (0f);
        
        [Header("State")]
        [SerializeField]
        private Property<float> _value = new (0f);
        
        [Header("Settings")]
        [SerializeField]
        private Vector3 _direction = Vector3.forward;
        [SerializeField]
        private AxisMotionType _motionType = AxisMotionType.Translation;
        [SerializeField]
        private AxisControlMode _controlMode = AxisControlMode.Position;
        [SerializeField]
        protected float _factor = 1f;
        [SerializeField]
        protected UpdateLoop _updateLoop = UpdateLoop.Update;
        
        private Transform _transform;
        private Vector3 _initPosition;
        private Quaternion _initRotation;

        private void OnEnable()
        {
            _transform = transform;
            _initPosition = _transform.localPosition;
            _initRotation = _transform.localRotation;
            _target.Subscribe(OnTargetChanged);
            _value.Subscribe(OnValueChanged);
        }

        private void OnDisable()
        {
            _target.Unsubscribe(OnTargetChanged);
            _value.Unsubscribe(OnValueChanged);
        }

        private void Reset()
        {
            _direction = Vector3.forward;
            _motionType = AxisMotionType.Translation;
            _controlMode = AxisControlMode.Position;
            _factor = 1f;
            _updateLoop = UpdateLoop.Update;
        }

        private void Update()
        {
            if (_updateLoop != UpdateLoop.Update) return;
            SetTargetSpeed(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_updateLoop != UpdateLoop.FixedUpdate) return;
            SetTargetSpeed(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            if (_updateLoop != UpdateLoop.LateUpdate) return;
            SetTargetSpeed(Time.deltaTime);
        }

        private void OnTargetChanged(float target)
        {
            if (_controlMode != AxisControlMode.Position) return;
            _value.Value = target * _factor;
        }

        private void SetTargetSpeed(float deltaTime)
        {
            if (_controlMode != AxisControlMode.Speed) return;
            _value.Value += _target.Value * _factor * deltaTime;
        }

        private void OnValueChanged(float value)
        {
            switch (_motionType)
            {
                case AxisMotionType.Translation:
                    _transform.localPosition = _initPosition + _transform.localRotation * _direction * value;
                    break;
                case AxisMotionType.Rotation:
                    _transform.localRotation = _initRotation * Quaternion.Euler(_direction * value);
                    break;
                case AxisMotionType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_motionType), _motionType, null);
            }
        }

        public enum AxisMotionType
        {
            Translation,
            Rotation,
            None
        }

        public enum AxisControlMode
        {
            Position,
            Speed
        }
    }

    public enum UpdateLoop
    {
        Update,
        FixedUpdate,
        LateUpdate
    }
}
