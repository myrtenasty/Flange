using System.Collections.Generic;
using UnityEngine;

namespace Preliy.Flange
{
    public static class Math
    {
        public const float TOLERANCE_FLOAT = 1e-6f;
        public const float SINGULARITY_ANGLE_LIMIT = 10f;
        public const float SINGULARITY_NORM_LIMIT = 0.2f;
        public const float FLOAT_MAX = 9e9f;
        
        public static bool IsEqual(float a1, float a2)
        {
            return Mathf.Abs(a1 - a2) < TOLERANCE_FLOAT;
        }

        public static bool IsEqual(Matrix4x4 m1, Matrix4x4 m2, float allowedError = 1e-6f)
        {
            var deltaPositionOk = Vector3.Distance(m1.GetPosition(), m2.GetPosition()) < allowedError;
            var deltaRotationOk = Quaternion.Angle(m1.rotation, m2.rotation) < allowedError;
            return deltaPositionOk && deltaRotationOk;
        }
        
        public static float Round(this float value, int digits)
        {
            var mult = Mathf.Pow(10.0f, digits);
            return Mathf.Round(value * mult) / mult;
        }
        
        public static Vector3 Round(this Vector3 value, int digits)
        {
            var mult = Mathf.Pow(10.0f, digits);
            for (var i = 0; i < 3; i++)
            {
                value[i] = Mathf.Round(value[i] * mult) / mult;
            }
            return value;
        }
        
        public static void SetMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.SetPositionAndRotation(matrix.GetPosition(),matrix.rotation);
        }

        public static void SetLocalMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.SetLocalPositionAndRotation(matrix.GetPosition(),matrix.rotation);
        }

        public static Matrix4x4 GetMatrix(this Transform transform)
        {
            return Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        }
        
        public static Matrix4x4 GetMatrixLocal(this Transform transform)
        {
            return Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        }

        public static Vector3 GetUpDirection(this Matrix4x4 matrix)
        {
            return new Vector3(matrix.m01, matrix.m11, matrix.m21).normalized;
        }
        
        public static Vector3 GetForwardDirection(this Matrix4x4 matrix)
        {
            return new Vector3(matrix.m02, matrix.m12, matrix.m22).normalized;
        }
        
        public static bool IsSphereOverlap(Vector3 point, Vector3 center, float radius)
        {
            return Vector3.Distance(point, center) < radius;
        }

        public static Vector3 GetPointOnArc(Vector3 center, Vector3 start, Vector3 normal, float angle, float t)
        {
            var value = Mathf.Lerp(0, angle, t);
            var point = start - center;
            var rotation = Quaternion.AngleAxis(value, normal.normalized);
            point = center + rotation * point;
            return point;
        }
        
        public static float Angle360(Vector3 from, Vector3 to,Vector3 normal)
        {
            var angle = SignedAngle(from, to, normal);
            return angle < 0 ? angle + 360f : angle;
        }
        
        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 normal)
        {
            var angle = Vector3.Angle( from, to );
            var sign = Mathf.Sign( Vector3.Dot( normal, Vector3.Cross( from, to ) ) );
            return angle * sign;
        }

        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }
        
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            var ab = b - a;
            var av = value - a;
            var result = Vector3.Dot(av, ab) / Vector3.Dot(ab, ab);
            return Mathf.Clamp01(result);
        }
        
        public static Quaternion Diff(this Quaternion to, Quaternion from)
        {
            return to * Quaternion.Inverse(from);
        }
        
        public static Quaternion Add(this Quaternion start, Quaternion diff)
        {
            return diff * start;
        }
        
        /// <summary>
        /// Get index in quadrant space
        /// </summary>
        /// <param name="value">Joint value</param>
        /// <param name="interval">Interval value</param>
        public static int GetQuadrant(float value, float interval)
        {
            var quadrant = value / interval;
            if (quadrant < 0)
            {
                return Mathf.CeilToInt(quadrant) - 1;
                
            }
            
            return Mathf.FloorToInt(quadrant);
        }

        /// <summary>
        /// Apply quadrant to angle value
        /// </summary>
        /// <param name="value">angle [deg]</param>
        /// <param name="quadrant">Target quadrant</param>
        public static float ApplyQuadrant(this float value, int quadrant)
        {
            var delta = quadrant - GetQuadrant(value, 90f);
            var turn = Mathf.FloorToInt(Mathf.Abs(delta) / 4f);
            return value + turn * 360f * Mathf.Sign(delta);
        }

        /// <summary>
        /// Get turn count in range [-180...180] deg
        /// </summary>
        /// <param name="value">angle [deg]</param>
        public static int GetTurn(this float value)
        {
            var turn = Mathf.Abs(value) / 360f;
            return (int)(Mathf.Sign(value) * turn);
        }

        /// <summary>
        /// Round angle in range +-PI
        /// </summary>
        /// <param name="value">Value in rad</param>
        public static float ClampPI(float value)
        {
            value %= 2 * Mathf.PI;
            return value switch
            {
                > Mathf.PI => value - 2 * Mathf.PI,
                < -Mathf.PI => value + 2 * Mathf.PI,
                _ => value
            };
        }

        public static float Get3PointSplineLength(Vector3 p0, Vector3 p1, Vector3 waypoint, int iterationsCount = 64)
        {
            var points = new List<Vector3>();
            
            for (var i = 0; i < iterationsCount; i++)
            {
                var t = Mathf.InverseLerp(0, iterationsCount, i);
                var inputBlendPosition = Vector3.Lerp(p0, waypoint, t);
                var outputBlendPosition = Vector3.Lerp(waypoint, p1, t);
                var result = Vector3.Lerp(inputBlendPosition, outputBlendPosition, t);
                
                points.Add(result);
            }

            var length = 0f;
            for (var i = 1; i < points.Count; i++)
            {
                length += Vector3.Distance(points[i - 1], points[i]);
            }

            return length;
        }

        public static ExtJoint Lerp(ExtJoint group0, ExtJoint group1, float t)
        {
            var result = new ExtJoint();
            for (var i = 0; i < ExtJoint.LENGTH; i++)
            {
                result[i] = Mathf.Lerp(group0[i], group1[i], t);
            }
            return result;
        }

        public static Matrix4x4 SetPosition(this Matrix4x4 matrix, Vector3 position)
        {
            matrix.m03 = position.x;
            matrix.m13 = position.y;
            matrix.m23 = position.z;
            return matrix;
        }
        
        public static Matrix4x4 SetRotation(this Matrix4x4 matrix, Vector3 eulerAngles)
        {
            matrix.SetTRS(matrix.GetPosition(), Quaternion.Euler(eulerAngles), matrix.lossyScale);
            return matrix;
        }
    }
}


