using System;

namespace Preliy.Flange
{
    public interface IProperty
    {
        
    }
    
    public interface IReadOnlyProperty<T> : IProperty
    {
        public T Value { get; }
        public event Action<T> OnValueChanged;
        public void Subscribe(Action<T> action);
        public void Unsubscribe(Action<T> action);
    }

    public interface IProperty<T> : IReadOnlyProperty<T>
    {
        public new T Value { get; set; }
        public void SetValueWithoutNotify(T value);
    }
}
