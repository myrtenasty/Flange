using System.Collections.Generic;
using UnityEngine;

namespace Preliy.Flange.Common
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Gripper : MonoBehaviour
    {
        public bool Gripped => _gripped;
        
        [SerializeField]
        private bool _gripped;
        [SerializeField]
        private List<Part> _parts = new ();

        public void Grip(bool grip)
        {
            if (grip)
            {
                Grip();
            }
            else
            {
                Release();
            }
        }
        
        public void Grip()
        {
            if (_parts.Count == 0) return;

            foreach (var part in _parts)
            {
                part.GetComponent<Rigidbody>().isKinematic = true;
                part.transform.parent = transform;
            }
            
            _gripped = true;
        }

        public void Release()
        {
            if (_parts.Count == 0) return;

            foreach (var part in _parts)
            {
                if (part.Type == Part.PartPhysicsType.Physics) part.GetComponent<Rigidbody>().isKinematic = false;
                part.transform.parent = null;
            }

            _gripped = false; 
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.TryGetComponent<Part>(out var part)) return;
            if(!_parts.Contains(part)) _parts.Add(part);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.TryGetComponent<Part>(out var part)) return;
            if (_parts.Contains(part)) _parts.Remove(part);
        }
    }
}
