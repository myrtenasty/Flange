using UnityEngine;

namespace Preliy.Flange
{
    public class Arc
    {
        public Vector3 Center { get; private set; }
        public float Angle { get; private set; }
        public float Length { get; private set; }
        
        private readonly Vector3 _start;
        private readonly Vector3 _normal;
        
        public Arc(Vector3 start, Vector3 end, Vector3 wayPoint)
        {
            _start = start;
            
            var v1 = end - _start;
            var v2 = wayPoint - _start;
            
            var normal = Vector3.Cross(v1, v2);
            normal.Normalize();
            
            var perpendicular1 = Vector3.Cross(v1, normal).normalized;
            var perpendicular2 = Vector3.Cross(v2, normal).normalized;
            var r = (v1 - v2) * 0.5f;
            var angle = Vector3.Angle(perpendicular1, perpendicular2);
            var a = Vector3.Angle(r, perpendicular1);
            var d = r.magnitude * Mathf.Sin(a * Mathf.Deg2Rad) / Mathf.Sin(angle * Mathf.Deg2Rad);
            if (Vector3.Dot(v1, wayPoint - end) > 0)
            {
                Center =  _start + v2 * 0.5f - perpendicular2 * d;
            }
            else
            {
                Center =  _start + v2 * 0.5f + perpendicular2 * d;
            }
            _normal = normal;
            Angle = Math.Angle360(Center - _start, Center - end, -normal);
            Length = Vector3.Distance(_start, Center) * Mathf.Deg2Rad * Angle;
        }

        public Vector3 GetPoint(float t)
        {
            return Math.GetPointOnArc(Center, _start, -_normal, Angle, t);
        }

        public float GetProgress(Vector3 point)
        {
            var angle = Math.Angle360(Center - _start, Center - point, -_normal);
            return Mathf.InverseLerp(0, Angle, angle);
        }
    }
}
