using UnityEngine;

namespace Preliy.Flange
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Robotics/Frame")]
    [DefaultExecutionOrder(-10)]
    public class Frame : MonoBehaviour
    {
        public Transform Transform => transform;
        
        public FrameConfig Config
        {
            get => _config;
            set
            {
                _config = value;
                SetTransform();
            }
        }

        [SerializeField]
        private FrameConfig _config = FrameConfig.Default;

        private void OnValidate()
        {
            SetTransform();
        }

        private void SetTransform()
        {
            transform.SetLocalMatrix(HomogeneousMatrix.CreateRaw(_config));
        }
    }
}
