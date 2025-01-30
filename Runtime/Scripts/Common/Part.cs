using System;
using UnityEngine;

namespace Preliy.Flange.Common
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Part: MonoBehaviour
    {
        public PartPhysicsType Type
        {
            get => _type;
            set => SetPhysicsType(value);
        }
        
        [SerializeField]
        private PartPhysicsType _type;

        private Collider _collider;
        private Rigidbody _rigidbody;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void SetPhysicsType(PartPhysicsType type)
        {
            if (_type == type) return;
            _type = type;
            switch (type)
            {
                case PartPhysicsType.Physics:
                    _collider.isTrigger = false;
                    _rigidbody.isKinematic = false;
                    break;
                case PartPhysicsType.Kinematic:
                    _collider.isTrigger = false;
                    _rigidbody.isKinematic = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        public enum PartPhysicsType
        {
            Physics,
            Kinematic
        }
    }
}
