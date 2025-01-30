using UnityEngine;

namespace Preliy.Flange
{
    public static class CartesianTargetExtension
    {
        public static CartesianTarget AddOffset(this CartesianTarget cartesianTarget, Matrix4x4 offset)
        {
            return cartesianTarget with {Pose = cartesianTarget.Pose * offset};
        }
        
        public static CartesianTarget AddOffset(this CartesianTarget cartesianTarget, Vector3 offset)
        {
            return cartesianTarget with {Pose = cartesianTarget.Pose * Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one)};
        }
        
        public static CartesianTarget AddOffset(this CartesianTarget cartesianTarget, Quaternion offset)
        {
            return cartesianTarget with {Pose = cartesianTarget.Pose * Matrix4x4.TRS(Vector3.zero, offset, Vector3.one)};
        }
    }
}
