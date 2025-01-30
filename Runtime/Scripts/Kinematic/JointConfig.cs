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
        [SerializeField]
        private Vector2 _limits;
        [Tooltip("Offset [deg]")]
        [SerializeField]
        private float _offset;
        [SerializeField]
        private float _factor;
        [Tooltip("Speed max [deg/s]")]
        [SerializeField]
        private float _speedMax;
        [Tooltip("Acc max [deg/s^2]")]
        [SerializeField]
        private float _accMax;

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
        
        public bool IsInRange(float angle)
        {
            return angle >= Limits.x && angle <= Limits.y;
        }

        public float GetValidValue(float value)
        {
            value = Mathf.Clamp(value, _limits.x, _limits.y);
            return _offset + value * _factor;
        }
        
        public static JointConfig Default => new (TransformJoint.JointType.Rotation, new Vector2(-180, 180), 0, 1f, 100f,500f);
    }
}
