using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Preliy.Flange
{
    [Serializable]
    public record CartesianTarget
    {
        public Object Object => _object;
        public Matrix4x4 Pose
        {
            get => _pose;
            set => _pose = value;
        }
        public Configuration Configuration
        {
            get => _configuration;
            set => _configuration = value;
        }
        public ExtJoint ExtJoint
        {
            get => _extJoint;
            set => _extJoint = value with { };
        }

        [SerializeField]
        private Object _object;
        [SerializeField]
        private Matrix4x4 _pose;
        [SerializeField]
        private Configuration _configuration;
        [SerializeField]
        private ExtJoint _extJoint;

        public static CartesianTarget Default => new (Matrix4x4.identity, Configuration.Default, ExtJoint.Default);

        public CartesianTarget(Matrix4x4 pose, Configuration configuration, ExtJoint extJoint, Object @object = null)
        {
            _object = @object;
            _pose = pose;
            _configuration = configuration;
            _extJoint = extJoint with { };
        }

        protected CartesianTarget(CartesianTarget other)
        {
            _object = other.Object;
            _pose = other.Pose;
            _configuration = other.Configuration;
            _extJoint = other.ExtJoint with { };
        }
    }
}
