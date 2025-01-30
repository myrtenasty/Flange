using UnityEngine;

namespace Preliy.Flange
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class TargetFollower : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Controller _controller;
        
        [Header("Target Configuration")]
        [SerializeField]
        private Configuration _configuration;
        
        [Header("External Joints")]
        [SerializeField]
        private float _e1;
        [SerializeField]
        private float _e2;
        [SerializeField]
        private float _e3;
        [SerializeField]
        private float _e4;
        [SerializeField]
        private float _e5;
        [SerializeField]
        private float _e6;
        
        [Header("Settings")]
        [SerializeField]
        private bool _runInEditor;
        [SerializeField]
        private bool _showErrorMassage;

        [HideInInspector]
        [SerializeField]
        private Property<Matrix4x4> _target = new ();

        private void OnEnable()
        {
            _target.OnValueChanged += JumpToTarget;
        }

        private void Update()
        {
            if (!Application.isPlaying && _runInEditor)
            {
                _target.Value = transform.GetMatrix();
            }
        }

        private void OnAnimatorMove()
        {
            _target.Value = transform.GetMatrix();
        }

        // private void OnValidate()
        // {
        //     _target.Value = transform.GetMatrix();
        // }

        private void OnDisable()
        {
            _target.OnValueChanged -= JumpToTarget;
        }

        private void JumpToTarget(Matrix4x4 target)
        {
            if (_controller == null) return;
            JumpTo(_controller, target, SolutionIgnoreMask.All, _showErrorMassage);
        }

        /// <summary>
        /// Jump to specific <see cref="CartesianTarget"/>
        /// </summary>
        /// <param name="controller"> <see cref="Controller"/></param>
        /// <param name="target">Target pose in world space</param>
        /// <param name="ignoreMask"> <see cref="SolutionIgnoreMask"/></param>
        /// <param name="showErrorMassage">Show error message, if error occured</param>
        private void JumpTo(Controller controller, Matrix4x4 target, SolutionIgnoreMask ignoreMask, bool showErrorMassage = true)
        {
            var externalJoints = new ExtJoint(_e1, _e2, _e3, _e4, _e5, _e6);
            var solution = controller.Solver.ComputeInverse(target, controller.Tool.Value, _configuration, externalJoints, ignoreMask);
            controller.Solver.TryApplySolution(solution, showErrorMassage);
        }
    }
}
