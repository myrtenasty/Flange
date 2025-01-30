using System.Collections.Generic;
using UnityEngine;

namespace Preliy.Flange
{
    public abstract class Robot : MechanicalUnit, IRobot
    {
        public abstract Matrix4x4 ComputeForward();
        public abstract IKSolution ComputeInverse(Matrix4x4 target, Configuration configuration, SolutionIgnoreMask ignoreMask);
        public abstract List<IKSolution> ComputeInverse(Matrix4x4 target, bool turn, SolutionIgnoreMask ignoreMask);
        public abstract int GetConfigurationIndex(float[] jointValues);
        public abstract int GetConfigurationIndex();
        public CartesianLimit CartesianLimit => _cartesianLimit;
        
        [SerializeField]
        protected CartesianLimit _cartesianLimit;
    }
}

