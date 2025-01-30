using System;
using UnityEngine;

namespace Preliy.Flange
{
    public static class HomogeneousMatrix
    {
        /// <summary>
        /// Create homogeneous matrix
        /// </summary>
        /// <param name="config">Frame DH config</param>
        /// <param name="joint">Joint config</param>
        /// <param name="value">Joint value [deg] or [m]</param>
        public static Matrix4x4 Create(FrameConfig config, JointConfig joint, float value)
        {
            return joint.Type switch
            {
                TransformJoint.JointType.Rotation => Create(config, angle: joint.GetValidValue(value) * Mathf.Deg2Rad),
                TransformJoint.JointType.Displacement => Create(config, displacement: joint.GetValidValue(value)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// Create homogeneous matrix
        /// </summary>
        /// <param name="config">Frame DH config</param>
        /// <param name="angle">Joint angle [rad]</param>
        /// <param name="displacement">Joint displacement [m]</param>
        public static Matrix4x4 Create(FrameConfig config, float angle = 0f, float displacement = 0f)
        {
            if (float.IsNaN(angle)) angle = 0;
            var theta = angle + config.Theta;
            var d = config.D + displacement;
            var alpha = config.Alpha;
            return Create(d, theta, config.A, alpha);
        }
        
        /// <summary>
        /// Create homogeneous matrix
        /// </summary>
        /// <param name="theta">q angle [rad]</param>
        /// <param name="d">d offset [m]</param>
        /// <param name="a">a offset [m]</param>
        /// <param name="alpha">alpha angle [rad]</param>
        public static Matrix4x4 Create(float d, float theta, float a, float alpha)
        {
            var cosTheta = Mathf.Cos(theta);
            var sinTheta = Mathf.Sin(theta);
            var cosAlpha = Mathf.Cos(alpha);
            var sinAlpha = Mathf.Sin(alpha);

            return new Matrix4x4
            {
                m00 = cosTheta * cosAlpha,
                m10 = -sinAlpha,
                m20 = sinTheta * cosAlpha,
                m30 = 0,
                
                m01 = cosTheta * sinAlpha,
                m11 = cosAlpha,
                m21 = sinTheta * sinAlpha,
                m31 = 0,
                
                m02 = -sinTheta,
                m12 = 0,
                m22 = cosTheta,
                m32 = 0,

                m03 = -a * sinTheta,
                m13 = d,
                m23 = a * cosTheta,
                m33 = 1
            };
        }
    }
}
