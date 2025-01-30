using UnityEngine;

namespace Preliy.Flange
{
    public interface IReferenceFrame
    {
        public Matrix4x4 GetWorldFrame();
        public Matrix4x4 GetWorldFrame(Controller controller, ExtJoint extJoint);
    }
}
