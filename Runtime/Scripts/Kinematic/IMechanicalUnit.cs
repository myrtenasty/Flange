using System;
using System.Collections.Generic;
using UnityEngine;

namespace Preliy.Flange
{
    public interface IMechanicalUnit
    {
        public Component Component { get; }
        public IReadOnlyList<TransformJoint> Joints { get; }
        public IReadOnlyList<Frame> Frames { get; }
        
        public event Action OnJointStateChanged;

        float this[int index]
        {
            get;
            set;
        }
    }
}
