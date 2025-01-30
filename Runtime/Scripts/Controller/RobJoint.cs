using System;
using UnityEngine;

namespace Preliy.Flange
{
    [Serializable]
    public record RobJoint
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
        private float _r1;
        [SerializeField]
        private float _r2;
        [SerializeField]
        private float _r3;
        [SerializeField]
        private float _r4;
        [SerializeField]
        private float _r5;
        [SerializeField]
        private float _r6;

        public float this[int index]
        {
            get
            {
                return index switch
                {
                    0 => _r1,
                    1 => _r2,
                    2 => _r3,
                    3 => _r4,
                    4 => _r5,
                    5 => _r6,
                    _ => throw new IndexOutOfRangeException("Invalid index!")
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _r1 = value;
                        break;
                    case 1:
                        _r2 = value;
                        break;
                    case 2:
                        _r3 = value;
                        break;
                    case 3:
                        _r4 = value;
                        break;
                    case 4:
                        _r5 = value;
                        break;
                    case 5:
                        _r6 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid index!");
                }
            }
        }
        
        public RobJoint(params float[] value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (value.Length > LENGTH) throw new ArgumentOutOfRangeException();

            _r1 = 0;
            _r2 = 0;
            _r3 = 0;
            _r4 = 0;
            _r5 = 0;
            _r6 = 0;
            
            for (var i = 0; i < value.Length; i++)
            {
                this[i] = value[i];
            }
        }
        
        public static RobJoint Default => new (0, 0, 0, 0, 0, 0);

        public override string ToString()
        {
            return string.Join(", ", Value);
        }
    }
}
