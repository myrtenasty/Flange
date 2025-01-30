using UnityEngine;

namespace Preliy.Flange
{
    [ExecuteAlways]
    public class LookAtTarget : MonoBehaviour
    {
        [SerializeField]
        private Transform _target;
        [SerializeField]
        private bool _classic;
        [SerializeField]
        private Vector3 _offset;
        
        private void LateUpdate()
        {
            if (_target == null) return;
            if (_classic)
            {
                transform.LookAt(_target);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(_target.TransformPoint(_offset) - transform.position, _target.forward);
            }
        }
    }
}
