using System.Collections.Generic;
using UnityEngine;

namespace Preliy.Flange
{
    [CreateAssetMenu(fileName = "Mechanical Unit Configuration", menuName = "Robotics/Mechanical Unit Configuration")]
    public class MechanicalUnitConfig : ScriptableObject
    {
        public IReadOnlyList<FrameConfig> Frames => _frames;
        public IReadOnlyList<JointConfig> Joints => _joints;
        public CartesianLimit CartesianLimit => _cartesianLimit;

        [SerializeField]
        private List<FrameConfig> _frames;
        [SerializeField]
        private List<JointConfig> _joints;
        [SerializeField]
        private CartesianLimit _cartesianLimit;

        public void Initialize(List<FrameConfig> frames, List<JointConfig> joints, CartesianLimit cartesianLimit)
        {
            _frames = frames;
            _joints = joints;
            _cartesianLimit = cartesianLimit;
        }

        public MechanicalUnitConfig Clone() => Instantiate(this);
    }
}


