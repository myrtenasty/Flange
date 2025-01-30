using System;
using UnityEngine;

namespace Preliy.Flange
{
    [Serializable]
    public class ReferenceFrameWorld : IReferenceFrame 
    {
        public string Name => _name;
        
        [SerializeField]
        private string _name = "World";
        
        public Matrix4x4 GetWorldFrame()
        {
            return Matrix4x4.identity;
        }
        
        public Matrix4x4 GetWorldFrame(Controller controller, ExtJoint extJoint)
        {
            return Matrix4x4.identity;
        }
    }
}
