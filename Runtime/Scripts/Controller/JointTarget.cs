using System;
using UnityEngine;

namespace Preliy.Flange
{
    /// <summary>
    /// <see cref="JointTarget"/> defined the position of robot and external axes
    /// </summary>
    [Serializable]
    public record JointTarget
    {
        public const int LENGTH = 12;
        
        public RobJoint RobJoint
        {
            get => _robJoint;
            set => _robJoint = value with { };
        }
        
        public ExtJoint ExtJoint
        {
            get => _extJoint;
            set => _extJoint = value with { };
        }

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
        }

        [SerializeField]
        private RobJoint _robJoint;
        [SerializeField]
        private ExtJoint _extJoint;
        
        public float this[int index]
        {
            get
            {
                return index switch
                {
                    0 => _robJoint[0],
                    1 => _robJoint[1],
                    2 => _robJoint[2],
                    3 => _robJoint[3],
                    4 => _robJoint[4],
                    5 => _robJoint[5],
                    6 => _extJoint[0],
                    7 => _extJoint[1],
                    8 => _extJoint[2],
                    9 => _extJoint[3],
                    10 => _extJoint[4],
                    11 => _extJoint[5],
                    _ => throw new IndexOutOfRangeException("Invalid index!")
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _robJoint[0] = value;
                        break;
                    case 1:
                        _robJoint[1] = value;
                        break;
                    case 2:
                        _robJoint[2] = value;
                        break;
                    case 3:
                        _robJoint[3] = value;
                        break;
                    case 4:
                        _robJoint[4] = value;
                        break;
                    case 5:
                        _robJoint[5] = value;
                        break;
                    case 6:
                        _extJoint[0] = value;
                        break;
                    case 7:
                        _extJoint[1] = value;
                        break;
                    case 8:
                        _extJoint[2] = value;
                        break;
                    case 9:
                        _extJoint[3] = value;
                        break;
                    case 10:
                        _extJoint[4] = value;
                        break;
                    case 11:
                        _extJoint[5] = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid index!");
                }
            }
        }

        public JointTarget(params float[] value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (value.Length > LENGTH) throw new ArgumentOutOfRangeException();

            _robJoint = RobJoint.Default;
            _extJoint = ExtJoint.Default;
            
            for (var i = 0; i < value.Length; i++)
            {
                this[i] = value[i];
            }
        }

        public JointTarget(RobJoint robJoint, ExtJoint extJoint)
        {
            _robJoint = robJoint with { };
            _extJoint = extJoint with { };
        }

        protected JointTarget(JointTarget other)
        {
            _robJoint = other.RobJoint with { };
            _extJoint = other.ExtJoint with { };
        }

        public static JointTarget Default => 
            new (0, 0, 0, 0, 0, 0, 
            Math.FLOAT_MAX, Math.FLOAT_MAX, Math.FLOAT_MAX, Math.FLOAT_MAX, Math.FLOAT_MAX, Math.FLOAT_MAX);
        
        public static JointTarget Null => 
            new (0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0);

        public override string ToString() => $"[{_robJoint}] [{_extJoint}]";
    }
}
