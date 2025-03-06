using UnityEngine;

namespace Preliy.Flange
{
    /// <summary>
    /// Frame configuration using the Denavit-Hartenberg parameters
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
        [Tooltip("A [m]")]
        [SerializeField]
        private float _a;
        [Tooltip("D [m]")]
        [SerializeField]
        private float _d;
        [Tooltip("Theta [rad]")]
        [SerializeField]
        private float _theta;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameConfig"/> class using the Denavit-Hartenberg parameters
        /// </summary>
        /// <param name="alpha">The link twist (α) [rad], representing the angle between the z-axes of consecutive frames.</param>
        /// <param name="a">The link length (a) [m], which is the distance between the z-axes along the common normal.</param>
        /// <param name="d">The link offset (d) [m], the distance along the previous z-axis to the common normal.</param>
        /// <param name="theta">The joint angle (θ) [rad], the rotation about the previous z-axis to align the x-axes.</param>
        /// <param name="name">An optional identifier for the frame configuration.</param>
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
