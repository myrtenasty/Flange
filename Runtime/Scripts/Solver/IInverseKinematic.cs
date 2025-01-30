using UnityEngine;

namespace Preliy.Flange
{
    public interface IInverseKinematic
    {
        /// <summary>
        /// Compute inverse kinematic
        /// </summary>
        /// <param name="target">Flange target in base coordinate system (BCS)</param>
        /// <param name="configuration">Pose <see cref="Configuration"/></param>
        /// <param name="ignoreMask">Optional <see cref="SolutionIgnoreMask"/></param>
        /// <returns><see cref="IKSolution"/> for target pose. Check the status of the solution</returns>
        public IKSolution ComputeInverse(Matrix4x4 target, Configuration configuration, SolutionIgnoreMask ignoreMask);
        
        public CartesianLimit CartesianLimit { get; }
    }
}
