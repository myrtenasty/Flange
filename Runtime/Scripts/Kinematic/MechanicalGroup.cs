using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Preliy.Flange
{
    [Serializable]
    public class MechanicalGroup : IReferenceFrame
    {
        public bool IsValid => _isValid;
        [CanBeNull] 
        public Robot Robot => _robot;
        public MechanicalUnit BaseMechanicalUnit => _baseMechanicalUnit;
        public List<MechanicalUnit> ExternalMechanicalUnits => _externalMechanicalUnits;
        public List<TransformJoint> Joints => _joints;
        public List<TransformJoint> RobotJoints => _robotJoints;
        public List<TransformJoint> ExternalJoints => _externalJoints;
        public JointTarget JointState => _jointState;

        [SerializeField]
        private Robot _robot;
        [SerializeField]
        private MechanicalUnit _baseMechanicalUnit;
        [SerializeField]
        private List<MechanicalUnit> _externalMechanicalUnits = new ();

        [HideInInspector]
        [SerializeField]
        private bool _isValid;
        [HideInInspector]
        [SerializeField]
        private List<MechanicalUnit> _mechanicalUnits = new ();
        [HideInInspector]
        [SerializeField]
        private List<TransformJoint> _joints = new ();
        [HideInInspector]
        [SerializeField]
        private List<TransformJoint> _robotJoints = new ();
        [HideInInspector]
        [SerializeField]
        private List<TransformJoint> _externalJoints = new ();

        [HideInInspector]
        [SerializeField]
        private Controller _controller;
        [SerializeField]
        private JointTarget _jointState = JointTarget.Default;
        [HideInInspector]
        [SerializeField]
        private JointTarget _initJointState = JointTarget.Default;

        public event Action OnValidateAction;
        public event Action OnJointStateChanged;

        public MechanicalGroup(Controller controller)
        {
            _controller = controller;
            _robot = _controller.GetComponent<Robot>();
        }
        
        public void OnEnable()
        {
            if (_isValid) SetJoints(_jointState, notify: true);
        }
        
        public void OnValidate()
        {
            try
            {
                _isValid = false;
                _mechanicalUnits = new List<MechanicalUnit>();
                _joints = new List<TransformJoint>();
                _robotJoints = new List<TransformJoint>();
                _externalJoints = new List<TransformJoint>();

                if (_robot == null)
                {
                    throw new Exception("Robot reference is null!");
                }
            
                _mechanicalUnits.Add(_robot);
            
                if (_baseMechanicalUnit is not null)
                {
                    if (!_mechanicalUnits.Contains(_baseMechanicalUnit)) {_mechanicalUnits.Add(_baseMechanicalUnit);}
                }

                foreach (var slave in _externalMechanicalUnits.Where(slave => slave is not null))
                {
                    _mechanicalUnits.Add(slave);
                }

                foreach (var joint in from mechanicalUnit in _mechanicalUnits from joint in mechanicalUnit.Joints where joint is not null select joint)
                {
                    _joints.Add(joint);
                }

                _robotJoints = _joints.Take(_robot.Joints.Count).ToList();
                _externalJoints = _joints.Skip(_robot.Joints.Count).ToList();

                _jointState.RobJoint.Value = _robotJoints.GetJointValues();
                _jointState.ExtJoint.Value = _externalJoints.GetJointValues();
                
                _isValid = true;
                OnValidateAction?.Invoke();
            }
            catch (Exception exception)
            {
                OnValidateAction?.Invoke();
                Logger.Log(LogType.Error, $"Mechanical Group isn't valid! {exception}", _controller);
                _isValid = false;
                throw;
            }
        }

        /// <summary>
        /// Set joint position
        /// </summary>
        /// <param name="index">index of joint in group</param>
        /// <param name="value">joint position</param>
        /// <param name="notify">If true, OnStateChanged will be executed</param>
        /// <param name="ignoreException">Ignore Exception</param>
        public void SetJoint(int index, float value, bool notify = false, bool ignoreException = false)
        {
            try
            {
                switch (index)
                {
                    case >= 0 and < 6:
                        _jointState[index] = value;
                        break;
                    case >= 6 and < 12:
                        var jointTarget = _jointState with {};
                        jointTarget[index] = value;
                        var refFrameTarget = _controller.GetTcpRelativeToRefFrame();
                        var target = _controller.FrameToWorld(refFrameTarget, _controller.Frame.Value, jointTarget.ExtJoint);
                        var solution = _controller.Solver.ComputeInverse(target, _controller.Tool.Value, _controller.Configuration.Value, jointTarget.ExtJoint);
                        if (!solution.IsValid)
                        {
                            _jointState[index] = value;
                            SetJoints(_jointState, notify);
                            throw solution.Exception;
                        }
                        _jointState = solution.JointTarget with {};
                        break;
                    case >= 12 and < 15:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SetJoints(_jointState, notify);
            }
            catch (Exception exception)
            {
                if (!ignoreException) Logger.Log(LogType.Error, exception, _controller);
            }
        }

        /// <summary>
        /// Set joint position
        /// </summary>
        /// <param name="jointTarget">joints position</param>
        /// <param name="notify">If true, OnStateChanged will be executed</param>
        public void SetJoints(JointTarget jointTarget, bool notify = false)
        {
            if (!_isValid) return;
            _jointState = jointTarget with {};
            SetRobotJoints(_jointState);
            SetExternalJoints(_jointState);
            if (notify) OnJointStateChanged?.Invoke();
        }

        /// <summary>
        /// Calculate forward kinematic with actual joint values
        /// </summary>
        /// <param name="coordinateSystem">Coordinate system</param>
        /// <returns>
        /// <see cref="Matrix4x4">Pose</see> in specific coordinate system
        /// </returns>
        public Matrix4x4 ComputeForward(CoordinateSystem coordinateSystem = CoordinateSystem.Base) => ComputeForward(_jointState, coordinateSystem);

        /// <summary>
        /// Calculate forward kinematic
        /// </summary>
        /// <param name="jointTarget">Specific joint target</param>
        /// <param name="coordinateSystem">Coordinate system</param>
        /// <returns>
        /// <see cref="Matrix4x4">Pose</see> in world coordinate system (WCS)
        /// </returns>
        public Matrix4x4 ComputeForward(JointTarget jointTarget, CoordinateSystem coordinateSystem = CoordinateSystem.Base)
        {
            if (_robot == null) throw new Exception("Robot isn't defined!");

            switch (coordinateSystem)
            {
                case CoordinateSystem.World:
                    
                    var result = GetRobotBaseWorld(jointTarget.ExtJoint);
                    result *= _robot.ComputeForward(jointTarget.RobJoint.Value);
                    return result;
                case CoordinateSystem.Base:
                    return _robot.ComputeForward(jointTarget.RobJoint.Value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(coordinateSystem), coordinateSystem, null);
            }
        }

        /// <summary>
        /// Calculate robot inverse kinematic
        /// </summary>
        /// <param name="target">Target pose in base coordinate system (BCS)</param>
        /// <param name="configuration">Target robot configuration</param>
        /// <param name="ignoreMask">Ignore mask for inverse kinematic solution</param>
        /// <returns>
        /// <see cref="IKSolution">Inverse kinematic solution</see>
        /// </returns>
        public IKSolution ComputeInverse(Matrix4x4 target, Configuration configuration, SolutionIgnoreMask ignoreMask = SolutionIgnoreMask.None)
        {
            if (_robot == null) throw new Exception("Robot isn't defined!");

            return _robot.ComputeInverse(target, configuration, ignoreMask);
        }
        
        /// <summary>
        /// Calculate all possible inverse kinematic solutions
        /// </summary>
        /// <param name="target">Target pose in base coordinate system (BCS)</param>
        /// <param name="turn">Include solutions with turn</param>
        /// <param name="ignoreMask">Ignore mask for inverse kinematic solution</param>
        /// <returns>
        /// List of <see cref="IKSolution">Inverse kinematic solutions</see> 
        /// </returns>
        public List<IKSolution> ComputeInverse(Matrix4x4 target, bool turn, SolutionIgnoreMask ignoreMask = SolutionIgnoreMask.None)
        {
            if (_robot == null) throw new Exception("Robot isn't defined!");

            return _robot.ComputeInverse(target, turn, ignoreMask);
        }

        private void SetRobotJoints(JointTarget jointTarget)
        {
            _robot.JointValue = jointTarget;
        }

        private void SetExternalJoints(JointTarget jointTarget)
        {
            var index = 0;

            if (_baseMechanicalUnit != null)
            {
                for (var i = 0; i < _baseMechanicalUnit.Joints.Count; i++)
                {
                    _baseMechanicalUnit[i] = jointTarget[6 + index];
                    index++;
                }
            }

            foreach (var externalMechanical in _externalMechanicalUnits)
            {
                for (var i = 0; i < externalMechanical.Joints.Count; i++)
                {
                    externalMechanical[i] = jointTarget[6 + index];
                    index++;
                }
            }
        }

        public bool IsJointTargetValid(JointTarget jointTarget)
        {
            var exceptions = new List<Exception>();
            for (var i = 0; i < _robotJoints.Count; i++)
            {
                if (_robotJoints[i].IsInRange(jointTarget[i])) continue;
                exceptions.Add(new Exception($"Robot joint target value {jointTarget[i]} at index {i} is out of range {_joints[i].Config.Limits}"));
            }
            
            for (var i = 0; i < _externalJoints.Count; i++)
            {
                if (_externalJoints[i].IsInRange(jointTarget.ExtJoint[i])) continue;
                exceptions.Add(new Exception($"External joint target value {jointTarget.ExtJoint[i]} at index {i} is out of range {_joints[i].Config.Limits}"));
            }
            
            if (exceptions.Count > 0) throw new AggregateException("JointTarget value isn't valid!", exceptions);

            return true;
        }

        public float[] GetMechanicalUnitJointValues(MechanicalUnit mechanicalUnit, ExtJoint extJoint)
        {
            var result = new float[mechanicalUnit.Joints.Count];
            var offset = 0;

            if (_baseMechanicalUnit != null)
            {
                if (_baseMechanicalUnit == mechanicalUnit)
                {
                    result = extJoint.Value[offset..mechanicalUnit.Joints.Count];
                    return result;
                }
                offset += _baseMechanicalUnit.Joints.Count;
            }

            foreach (var unit in _externalMechanicalUnits)
            {
                if (unit == mechanicalUnit)
                {
                    result = extJoint.Value[offset..mechanicalUnit.Joints.Count];
                    return result;
                }
                offset += unit.Joints.Count;
            }

            return result;
        }

        /// <summary>
        /// Compute base transform
        /// </summary>
        /// <returns>
        /// <see cref="Matrix4x4">Pose</see> in world coordinate system (WCS)
        /// </returns>
        public Matrix4x4 GetRobotBaseWorld(CoordinateSystem frame = CoordinateSystem.Base)
        {
            return frame switch
            {
                CoordinateSystem.World => _robot.WorldTransform,
                CoordinateSystem.Base => Matrix4x4.identity,
                _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, null)
            };
        }

        /// <summary>
        /// Compute base transform
        /// </summary>
        /// <param name="extJoint">External joints</param>
        /// <returns>
        /// <see cref="Matrix4x4">Pose</see> in world coordinate system (WCS)
        /// </returns>
        public Matrix4x4 GetRobotBaseWorld(ExtJoint extJoint)
        {
            if (_baseMechanicalUnit == null)
            {
                return _robot == null ? Matrix4x4.identity : _robot.WorldTransform;
            }
            
            return _baseMechanicalUnit.WorldTransform * _baseMechanicalUnit.ComputeForward(GetMechanicalUnitJointValues(_baseMechanicalUnit, extJoint));
        }
        
        /// <summary>
        /// Get robot configuration from actual state
        /// </summary>
        /// <returns>
        /// Configuration index
        /// </returns>
        public int GetConfigurationIndex()
        {
            if (_robot == null) throw new Exception("Robot isn't defined!");

            return _robot.GetConfigurationIndex(_jointState.Value);
        }

        /// <summary>
        /// Get robot configuration 
        /// </summary>
        /// <param name="jointTarget">Target joint values</param>
        /// <returns>
        /// Configuration index
        /// </returns>
        public int GetConfigurationIndex(JointTarget jointTarget)
        {
            if (_robot == null) throw new Exception("Robot isn't defined!");

            return _robot.GetConfigurationIndex(jointTarget.Value);
        }
        
        /// <summary>
        /// Save mechanical group joint values
        /// </summary>
        public void SaveState()
        {
            _initJointState = _jointState with {};
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(_controller);
#endif
        }

        /// <summary>
        /// Load and set saved mechanical group joint values
        /// </summary>
        public void LoadState()
        {
            SetJoints(_initJointState, true);
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(_controller);
#endif
        }
        
        public Matrix4x4 GetWorldFrame()
        {
            return GetRobotBaseWorld(CoordinateSystem.World);
        }
        
        public Matrix4x4 GetWorldFrame(Controller controller, ExtJoint extJoint)
        {
            return controller.MechanicalGroup.GetRobotBaseWorld(extJoint);
        }
    }
}
