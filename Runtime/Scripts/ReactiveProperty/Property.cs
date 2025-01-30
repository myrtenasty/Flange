using System;
using System.Collections.Generic;
using UnityEngine;

namespace Preliy.Flange
{
    [Serializable]
    public class Property<T> : IProperty<T>
    {
        public event Action<T> OnValueChanged;
        
        public T Value
        {
            get => _value;
            set => CheckValue(value);
        }

        [SerializeField]
        private T _value;
        
        private T _lastValue;
        private EqualityComparer<T> _comparer;
        
        public Property() : this(default)
        {
        }

        public Property(T value)
        {
            _value = value;
        }

        private bool CheckValue(T value)
        {
            _comparer ??= EqualityComparer<T>.Default;
            if (_comparer.Equals(_value, value)) return false;

            SetValue(value);
            return true;
        }
        
        public void OnValidate()
        {
            _comparer ??= EqualityComparer<T>.Default;
            if (_comparer.Equals(_value, _lastValue)) return;
            SetValue(_value);
        }

        public void SetValue(T value)
        {
            SetValueWithoutNotify(value);
            OnValueChanged?.Invoke(value);
        }

        public void SetValueWithoutNotify(T value)
        {
            _value = value;
            _lastValue = value;
        }

        public void Subscribe(Action<T> action)
        {
            OnValueChanged += action;
            action?.Invoke(_value);
        }
        
        public void Unsubscribe(Action<T> action)
        {
            OnValueChanged -= action;
        }

        public static implicit operator T(Property<T> binding) => binding._value;

        public override string ToString() =>  _value.ToString();
    }
}
