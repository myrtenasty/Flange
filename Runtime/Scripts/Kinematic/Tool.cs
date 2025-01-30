using UnityEngine;

namespace Preliy.Flange
{
    public class Tool : MonoBehaviour
    {
        public ToolMountType MountType => _mountType;
        public Transform Point
        {
            get => _point;
            set => _point = value;
        }
        public Matrix4x4 Offset => _offset;

        [Header("Settings")]
        [SerializeField]
        private ToolMountType _mountType;
        [SerializeField]
        private Transform _point;

        [Header("Gizmos")]
        [SerializeField]
        private bool _showGizmos;
        [SerializeField]
        private bool _showFrame;
        [SerializeField]
        private bool _showName;
        [SerializeField]
        private Matrix4x4 _offset;

        private void Start()
        {
            _offset = GetOffset();
        }

        private void OnValidate()
        {
            _offset = GetOffset();
        }
        
        public Matrix4x4 GetOffset()
        {
            if (_point == null)
            {
                return Matrix4x4.identity;
            }
            
            return transform.GetMatrix().inverse * _point.GetMatrix();
        }

        public enum ToolMountType
        {
            OnRobot,
            Extern
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!_showGizmos) return;
            if (_point == null) return;
            if (_showFrame) UnityEditor.Handles.PositionHandle(_point.position, _point.rotation);
            if (_showName) UnityEditor.Handles.Label(_point.position, name);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_point.position, 0.005f);
#endif
        }
    }
}
