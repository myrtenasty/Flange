using System;
using UnityEngine;

namespace Preliy.Flange
{
    [Serializable]
    public class ReferenceFrame : MonoBehaviour, IReferenceFrame
    {
        public string Name => gameObject.name;

        [SerializeField]
        private Matrix4x4 _local;
        [SerializeField]
        private Matrix4x4 _global;
        [SerializeField]
        private MechanicalUnit _mechanicalUnit;

        private void OnValidate()
        {
            OnTransformChanged();
            FindMechanicalUnit();
        }

        private void Reset()
        {
            FindMechanicalUnit();
        }

        private void Update()
        {
            OnTransformChanged();
        }

        public Matrix4x4 GetWorldFrame() => transform.GetMatrix();
        
        public Matrix4x4 GetWorldFrame(Controller controller, ExtJoint extJoint)
        {
            if (_mechanicalUnit == null)
            {
                return _global;
            }
            var jointValues = controller.MechanicalGroup.GetMechanicalUnitJointValues(_mechanicalUnit, extJoint);
            return _mechanicalUnit.WorldTransform * _mechanicalUnit.ComputeForward(jointValues) * _local;
        }

        private void OnTransformChanged()
        {
            if (!transform.hasChanged) return;
            _local = transform.GetMatrixLocal();
            _global = transform.GetMatrix();
            transform.hasChanged = false;
        }

        private void FindMechanicalUnit()
        {
            var target = transform;
            while (target.parent != null)
            {
                if (target.parent.TryGetComponent(out MechanicalUnit mechanicalUnit))
                {
                    _mechanicalUnit = mechanicalUnit;
                    return;
                }
                target = target.parent.transform;
            }
        }
    }
}
