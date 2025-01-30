using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Preliy.Flange.Common
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Cylinder : MonoBehaviour
    {
        public bool JogPlus
        {
            get => _jogPlus;
            set => _jogPlus = value;
        }

        public bool JogMinus
        {
            get => _jogMinus;
            set => _jogMinus = value;
        }
        
        private IProperty<float> Progress => _progress;
        private IProperty<float> Value => _value;
        private IProperty<bool> LimitMin => _limitMin;
        private IProperty<bool> LimitMax => _limitMax;

        [Space(10)]
        [Header("Control")]
        [SerializeField]
        private bool _jogMinus;
        [SerializeField]
        private bool _jogPlus;

        [Space(10)]
        [Header("Status")]
        [SerializeField]
        private Property<float> _progress = new (0f);
        [SerializeField]
        private Property<float> _value = new (0f);
        [SerializeField]
        private Property<bool> _limitMin = new (false);
        [SerializeField]
        private Property<bool> _limitMax = new (false);

        [Header("Settings")]
        [SerializeField]
        private List<Axis> _axes = new List<Axis>();
        [Tooltip("Duration to Max Limit (X) [s]")]
        [SerializeField]
        private float _timeToMin = 1.0f;
        [Tooltip("Duration to Max Limit (Y) [s]")]
        [SerializeField]
        private float _timeToMax = 1.0f;
        [Tooltip("Limits [mm]")]
        [SerializeField]
        private Vector2 _limits = new Vector2(0, 100);
        [SerializeField]
        private CylinderType _type;
        [SerializeField]
        private AnimationCurve _profile = AnimationCurve.Linear(0, 0, 1, 1);

        
        public UnityEvent<float> OnProgressChanged;
        public UnityEvent<float> OnValueChanged;
        public UnityEvent<bool> OnLimitMinChanged;
        public UnityEvent<bool> OnLimitMaxChanged;

        private void OnEnable()
        {
            _progress.Subscribe(OnProgressChangedAction);
            _value.Subscribe(OnValueChangedAction);
            _limitMin.Subscribe(OnLimitMinChangedAction);
            _limitMax.Subscribe(OnLimitMaxChangedAction);
        }

        private void OnDisable()
        {
            _progress.Unsubscribe(OnProgressChangedAction);
            _value.Unsubscribe(OnValueChangedAction);
            _limitMin.Unsubscribe(OnLimitMinChangedAction);
            _limitMax.Unsubscribe(OnLimitMaxChangedAction);
        }

        private void Update()
        {
            switch (_type)
            {
                case CylinderType.DoubleActing:
                    if (_jogMinus ^ _jogPlus) MoveTo(_jogPlus ? _timeToMax : -_timeToMin);
                    break;
                case CylinderType.SingleActingPositive:
                    MoveTo(_jogPlus ? _timeToMax : -_timeToMin);
                    break;
                case CylinderType.SingleActingNegative:
                    MoveTo(_jogMinus ? -_timeToMin : _timeToMax);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void MoveTo(float duration)
        {
            _progress.Value += 1/duration * Time.deltaTime;
            _progress.Value = Mathf.Clamp01(_progress.Value);
        }

        private void OnProgressChangedAction(float value)
        {
            _value.Value = Mathf.Lerp(_limits.x, _limits.y, _profile.Evaluate(value));
            _limitMin.Value = !(_value.Value > _limits.x);
            _limitMax.Value = !(_value.Value < _limits.y);
            OnProgressChanged?.Invoke(value);
        }

        private void OnValueChangedAction(float value)
        {
            foreach (var axis in _axes)
            {
                axis.Target.Value = value;
            }
            
            OnValueChanged?.Invoke(value);
        }

        private void OnLimitMinChangedAction(bool value)
        {
            OnLimitMinChanged?.Invoke(value);
        }
        
        private void OnLimitMaxChangedAction(bool value)
        {
            OnLimitMaxChanged?.Invoke(value);
        }

        private enum CylinderType
        {
            DoubleActing,
            SingleActingPositive,
            SingleActingNegative
        }
    }
}
