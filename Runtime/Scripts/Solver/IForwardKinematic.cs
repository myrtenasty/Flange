using UnityEngine;

namespace Preliy.Flange
{
    public interface IForwardKinematic
    {
        /// <summary>
        /// Compute forward kinematic
        /// </summary>
        /// <param name="jointValues">Joint values [deg]</param>
        /// <returns>Flange <see cref="UnityEngine.Matrix4x4"/> in robot base coordinate system (BCS)</returns>
        public Matrix4x4 ComputeForward(float[] jointValues);
    }
}
