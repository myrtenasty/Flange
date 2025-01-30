using UnityEngine;

namespace Preliy.Flange
{
    /// <summary>
    /// Denavit–Hartenberg frame configuration
    /// </summary>
    [System.Serializable]
    public record FrameConfig
    {
        public string Name
        {
            get => _name;
            set => _name = value;
        }
        
        public float Alpha => _alpha;
        public float A => _a;
        public float D => _d;
        public float Theta => _theta;

        [SerializeField]
        private string _name;
        [Tooltip("Alpha [rad]")]
        [SerializeField]
        private float _alpha;
        [SerializeField]
        private float _a;
        [SerializeField]
        private float _d;
        [Tooltip("Theta [rad]")]
        [SerializeField]
        private float _theta;

        public FrameConfig(float alpha, float a, float d, float theta, string name = null)
        {
            _name = name;
            _alpha = alpha;
            _a = a;
            _d = d;
            _theta = theta;
        }

        public static FrameConfig Default => new (0, 0, 0, 0);
    }
}
