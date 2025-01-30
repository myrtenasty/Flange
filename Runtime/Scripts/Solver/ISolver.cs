using System.Collections.Generic;
using UnityEngine;

namespace Preliy.Flange
{
    public interface ISolver : IForwardKinematic, IInverseKinematic
    {
        /// <summary>
        /// Compute forward kinematic.
        /// The actual joint values are used for the calculation
        /// </summary>
        /// <returns>Flange <see cref="UnityEngine.Matrix4x4"/> in robot base coordinate system (BCS)</returns>
        public Matrix4x4 ComputeForward();

        /// <summary>
        /// Get all possible solutions for specific target pose
        /// </summary>
        /// <param name="target">Pose pose in reference frame space</param>
        /// <param name="turn">Compute include axes turns</param>
        /// <param name="ignoreMask">Solution policy</param>
        /// <returns>List of valid <see cref="IKSolution"/></returns>
        public List<IKSolution> ComputeInverse(Matrix4x4 target, bool turn, SolutionIgnoreMask ignoreMask);

        /// <summary>
        /// Get the robot configuration for specific joint values
        /// </summary>
        /// <param name="jointValues">Joint values [deg]</param>
        public int GetConfigurationIndex(float[] jointValues);

        /// <summary>
        /// Get the robot configuration
        /// The actual joint values are used for the calculation 
        /// </summary>
        public int GetConfigurationIndex();
    }
}
