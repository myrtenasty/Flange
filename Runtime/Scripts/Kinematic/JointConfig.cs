using UnityEngine;

namespace Preliy.Flange
{
    [System.Serializable]
    public record JointConfig
    {
        public string Name
        {
            get => _name;
            set => _name = value;
        }
        
        public TransformJoint.JointType Type => _type;
        public Vector2 Limits => _limits;
        public float Offset => _offset;
        public float Factor => _factor;
        public float SpeedMax => _speedMax;
        public float AccMax => _accMax;
        
        [SerializeField]
        private string _name;
        [SerializeField]
        private TransformJoint.JointType _type;
        [Tooltip("Limits [deg] or [m]")]
        [SerializeField]
        private Vector2 _limits;
        [Tooltip("Offset [deg] or [m]")]
        [SerializeField]
        private float _offset;
        [SerializeField]
        private float _factor;
        [Tooltip("Speed max [deg/s] or [m/s]")]
        [SerializeField]
        private float _speedMax;
        [Tooltip("Acc max [deg/s^2] or [m/s^2]")]
        [SerializeField]
        private float _accMax;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="JointConfig"/> class with the specified joint configuration parameters.
        /// </summary>
        /// <param name="type">The type of the transform joint (e.g., revolute or prismatic).</param>
        /// <param name="limits">The joint limits as a <see cref="Vector2"/>, representing the minimum and maximum values in [deg] or [m].</param>
        /// <param name="offset">The offset value for the joint, specified in [deg] or [m].</param>
        /// <param name="factor">A scaling factor applied to the joint’s configuration.</param>
        /// <param name="speedMax">The maximum speed of the joint in [deg/s] or [m/s].</param>
        /// <param name="accMax">The maximum acceleration of the joint in [deg/s^2] or [m/s^2].</param>
        /// <param name="name">An optional identifier for the joint configuration.</param>
        public JointConfig(TransformJoint.JointType type, Vector2 limits, float offset, float factor, float speedMax, float accMax, string name = null)
        {
            _name = name;
            _type = type;
            _limits = limits;
            _offset = offset;
            _factor = factor;
            _speedMax = speedMax;
            _accMax = accMax;
        }
        
        public bool IsInRange(float value)
        {
            return value >= Limits.x && value <= Limits.y;
        }

        public float GetValidValue(float value)
        {
            value = Mathf.Clamp(value, _limits.x, _limits.y);
            return (value + _offset) * _factor;
        }
        
        public static JointConfig Default => new (TransformJoint.JointType.Rotation, new Vector2(-180, 180), 0, 1f, 100f,500f);
    }
}
