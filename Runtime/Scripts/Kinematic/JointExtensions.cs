using System;
using System.Collections.Generic;
using System.Linq;

namespace Preliy.Flange
{
    public static class JointExtension
    {
        public static bool IsInRange(this TransformJoint transformJoint, float value)
        {
            return value >= transformJoint.Config.Limits.x && value <= transformJoint.Config.Limits.y;
        }

        public static bool AreInRange(this TransformJoint[] joints, float[] value)
        {
            if (joints.Length != value.Length) throw new ArgumentOutOfRangeException(nameof(joints), "Array size mismatch");
            return !joints.ToList().Where((t, i) => !t.IsInRange(value[i])).Any();
        }

        public static void VerifyRange(this IReadOnlyList<TransformJoint> joints, float[] value)
        {
            VerifyRange(joints.ToArray(), value);
        }
        
        public static void VerifyRange(this TransformJoint[] joints, float[] value)
        {
            var exceptions = new List<Exception>();

            for (var i = 0; i < joints.Length; i++)
            {
                if (!joints[i].IsInRange(value[i])) exceptions.Add(new Exception($"Pose value at index {i} is out of range {joints[i].Config.Limits}"));
            }

            if (exceptions.Count > 0) throw new AggregateException("Pose value validation is failed!", exceptions);
        }
        
        public static float[] GetJointValues(this List<MechanicalUnit> mechanicalUnits)
        {
            var joints = mechanicalUnits.SelectMany(mechanicalUnit => mechanicalUnit.Joints).ToList();
            var result = new float[joints.Count];
            for (var i = 0; i < joints.Count; i++)
            {
                result[i] = joints[i].Position.Value;
            }
            return result;
        }

        public static float[] GetJointValues(this TransformJoint[] joints)
        {
            var result = new float[joints.Length];
            for (var i = 0; i < joints.Length; i++)
            {
                result[i] = joints[i].Position.Value; 
            }
            return result;
        }

        public static void SetJointValues(this IReadOnlyList<TransformJoint> joints, JointTarget target)
        {
            for (var i = 0; i < joints.Count; i++)
            {
                joints[i].Position.Value = target[i]; 
            }
        }

        public static void SetJointValues(this TransformJoint[] joints, float[] value)
        {
            if (joints == null || joints.Length == 0) return;
            if (value.Length != joints.Length) return;
            for (var i = 0; i < joints.Length; i++)
            {
                joints[i].Position.Value = value[i]; 
            }
        }

        public static float[] GetJointValues(this MechanicalUnit mechanicalUnit, float[] value = null)
        {
            return mechanicalUnit.Joints.GetJointValues(value);
        }
        
        public static float[] GetJointValues(this IReadOnlyList<TransformJoint> joints, float[] value = null)
        {
            value ??= new float[joints.Count];

            if (joints.Count != value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Is not equal to joint length!");
            }

            for (var i = 0; i < joints.Count; i++)
            {
                value[i] = joints[i].Position.Value; 
            }
            
            return value;
        }
        
        public static IReadOnlyList<TransformJoint> GetJoints(this IReadOnlyList<MechanicalUnit> mechanicalUnits)
        {
            return mechanicalUnits.SelectMany(mechanicalUnit => mechanicalUnit.Joints).ToList();
        }

        public static void SetJointValues(this IReadOnlyList<TransformJoint> joints, float[] value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                joints[i].Position.Value = value[i]; 
            }
        }
        
        public static int GetJointCount(this IEnumerable<MechanicalUnit> mechanicalUnits)
        {
            return mechanicalUnits.SelectMany(mechanicalUnit => mechanicalUnit.Joints).ToList().Count;
        }
    }
}
