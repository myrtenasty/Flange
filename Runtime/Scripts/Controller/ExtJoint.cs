using System;
using UnityEngine;

namespace Preliy.Flange
{
    [Serializable]
    public record ExtJoint
    {
        public const int LENGTH = 6;

        public float[] Value
        {
            get
            {
                var array = new float[LENGTH];
                for (var i = 0; i < LENGTH; i++)
                {
                    array[i] = this[i];
                }
                return array;
            }

            set
            {
                for (var i = 0; i < value.Length; i++)
                {
                    this[i] = value[i];
                }
            }
        }
        
        [SerializeField]
        private float _e1;
        [SerializeField]
        private float _e2;
        [SerializeField]
        private float _e3;
        [SerializeField]
        private float _e4;
        [SerializeField]
        private float _e5;
        [SerializeField]
        private float _e6;

        public float this[int index]
        {
            get
            {
                return index switch
                {
                    0 => _e1,
                    1 => _e2,
                    2 => _e3,
                    3 => _e4,
                    4 => _e5,
                    5 => _e6,
                    _ => throw new IndexOutOfRangeException("Invalid index!")
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _e1 = value;
                        break;
                    case 1:
                        _e2 = value;
                        break;
                    case 2:
                        _e3 = value;
                        break;
                    case 3:
                        _e4 = value;
                        break;
                    case 4:
                        _e5 = value;
                        break;
                    case 5:
                        _e6 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid index!");
                }
            }
        }
        
        public ExtJoint(params float[] value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (value.Length > LENGTH) throw new ArgumentOutOfRangeException();

            _e1 = Math.FLOAT_MAX;
            _e2 = Math.FLOAT_MAX;
            _e3 = Math.FLOAT_MAX;
            _e4 = Math.FLOAT_MAX;
            _e5 = Math.FLOAT_MAX;
            _e6 = Math.FLOAT_MAX;
            
            for (var i = 0; i < value.Length; i++)
            {
                this[i] = value[i];
            }
        }
        
        public static ExtJoint Default => 
            new (Math.FLOAT_MAX, Math.FLOAT_MAX, Math.FLOAT_MAX, Math.FLOAT_MAX, Math.FLOAT_MAX, Math.FLOAT_MAX);
        
        public override string ToString()
        {
            return string.Join(", ", Value);
        }
    }
}
