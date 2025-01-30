using System;
using UnityEngine;

namespace Preliy.Flange
{
    [Serializable]
    public record IKSolution
    {
        public JointTarget JointTarget => _jointTarget;
        public IKSolutionState State => _state;
        public Configuration Configuration
        {
            get => _configuration;
            set => SetConfiguration(value);
        }
        public bool IsValid => _state == IKSolutionState.Valid;
        public NotValidIKSolutionException Exception => _exception;

        [SerializeField]
        private JointTarget _jointTarget;
        [SerializeField]
        private Configuration _configuration;
        [SerializeField]
        private IKSolutionState _state;

        private NotValidIKSolutionException _exception;

        public static IKSolution IKSolutionNaN => new ("Target in not reachable!");

        /// <summary>
        /// Inverse kinematic solution
        /// </summary>
        /// <param name="target">Joint Values [deg]</param>
        /// <param name="configuration"></param>
        public IKSolution(JointTarget target, Configuration configuration)
        {
            _state = IKSolutionState.Unknown;
            _jointTarget = target with {};
            SetConfiguration(configuration);
        }

        public IKSolution(string errorMessage = null)
        {
            _state = IKSolutionState.Error;
            _jointTarget = new JointTarget();
            _configuration = Configuration.Default;
            _exception = new NotValidIKSolutionException(this, errorMessage);
        }
        
        public void Validate(MechanicalUnit mechanicalUnit)
        {
            try
            {
                mechanicalUnit.Joints.VerifyRange(_jointTarget.RobJoint.Value);
                _state = IKSolutionState.Valid;
            }
            catch (AggregateException aggregateException)
            {
                _state = IKSolutionState.Error;
                _exception = new NotValidIKSolutionException(this, string.Join(Environment.NewLine, aggregateException.InnerExceptions));
            }
            catch (Exception exception)
            {
                _state = IKSolutionState.Error;
                _exception = new NotValidIKSolutionException(this, exception.Message);
            }
        }

        public void SetExternalJoints(ExtJoint extJoint)
        {
            _jointTarget.ExtJoint = extJoint;
        }
        
        private void SetConfiguration(Configuration configuration)
        {
            _configuration = configuration;
            _jointTarget.ApplyTurn(configuration);
            _state = IKSolutionState.Unknown;
        }

        public override string ToString() => $"{_jointTarget} {_exception.Message}";
        public string GetLabel() => $"C:[{_configuration.ToString()}] R:[{_jointTarget.RobJoint}]";
    }
}
