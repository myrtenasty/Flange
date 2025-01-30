using System;
using System.Collections.Generic;
using UnityEngine;

namespace Preliy.Flange
{
    public class Solver
    {
        private readonly Controller _controller;

        public Solver(Controller controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Compute forward kinematic
        /// </summary>
        /// <param name="jointTarget">Joint Pose</param>
        /// <param name="toolIndex">Tool index</param>
        /// <returns>
        /// <see cref="Matrix4x4">Pose</see> in world coordinate system (WCS)
        /// </returns>
        public Matrix4x4 ComputeForward(JointTarget jointTarget, int toolIndex)
        {
            var result = _controller.MechanicalGroup.ComputeForward(jointTarget, CoordinateSystem.World);
            result *= _controller.GetToolOffset(toolIndex);
            return result;
        }

        /// <summary>
        /// Calculate forward kinematic
        /// </summary>
        /// <param name="jointTarget"> <see cref="JointTarget"/></param>
        /// <returns>
        /// <see cref="Matrix4x4">Pose</see> in base coordinate system (BCS)
        /// </returns>
        public Matrix4x4 ComputeForward(JointTarget jointTarget) => _controller.MechanicalGroup.ComputeForward(jointTarget);

        /// <summary>
        /// Compute possible <see cref="IKSolution">solution</see>
        /// </summary>
        /// <param name="target"> <see cref="CartesianTarget"/></param>
        /// <param name="tool">Tool index</param>
        /// <param name="frame">Frame index</param>
        /// <param name="ignoreMask"> <see cref="SolutionIgnoreMask"/></param>
        /// <returns>
        /// <see cref="IKSolution"/> for specified <see cref="CartesianTarget"/>
        /// </returns>
        public IKSolution ComputeInverse(CartesianTarget target, int tool, int frame, SolutionIgnoreMask ignoreMask = SolutionIgnoreMask.None)
        {
            var worldPose = _controller.FrameToWorld(target.Pose, frame, target.ExtJoint);
            return ComputeInverse(worldPose, tool, target.Configuration, target.ExtJoint, ignoreMask);
        }

        /// <summary>
        /// Compute possible <see cref="IKSolution">solution</see>
        /// </summary>
        /// <param name="target"> Target matrix in world space</param>
        /// <param name="tool">Tool index</param>
        /// <param name="configuration">Target configuration</param>
        /// <param name="extJoint">Target external joint values</param>
        /// <param name="ignoreMask"> <see cref="SolutionIgnoreMask"/></param>
        /// <returns>
        /// <see cref="IKSolution"/> for specified <see cref="CartesianTarget"/>
        /// </returns>
        public IKSolution ComputeInverse(Matrix4x4 target, int tool, Configuration configuration, ExtJoint extJoint, SolutionIgnoreMask ignoreMask = SolutionIgnoreMask.None)
        {
            var @base = _controller.MechanicalGroup.GetRobotBaseWorld(extJoint);
            var flange = @base.inverse * target;
            flange = _controller.RemoveToolOffset(flange, tool);
            var solution = _controller.MechanicalGroup.ComputeInverse(flange, configuration, ignoreMask);
            solution.SetExternalJoints(extJoint);
            return solution;
        }
        
        /// <summary>
        /// Compute all possible <see cref="IKSolution">solutions</see>
        /// </summary>
        /// <param name="target"> <see cref="CartesianTarget"/></param>
        /// <param name="tool">Tool index</param>
        /// <param name="frame">Frame index</param>
        /// <param name="turn">Compute include axes turns</param>
        /// <param name="ignoreMask"> <see cref="SolutionIgnoreMask"/></param>
        /// <returns>
        /// List of <see cref="IKSolution"/> for specified <see cref="CartesianTarget"/>
        /// </returns>
        public List<IKSolution> GetAllSolutions(CartesianTarget target, int tool, int frame, bool turn, SolutionIgnoreMask ignoreMask = SolutionIgnoreMask.None)
        {
            var @base = _controller.MechanicalGroup.GetRobotBaseWorld(target.ExtJoint);
            var flange = @base.inverse * _controller.FrameToWorld(target.Pose, frame, target.ExtJoint);
            flange = _controller.RemoveToolOffset(flange, tool);
            List<IKSolution> solutions = _controller.MechanicalGroup.ComputeInverse(flange, turn, ignoreMask);
            foreach (var solution in solutions)
            {
                solution.SetExternalJoints(target.ExtJoint);
            }
            
            return solutions;
        }
        
        /// <summary>
        /// Jump to specific <see cref="CartesianTarget"/>
        /// </summary>
        /// <param name="target"> <see cref="CartesianTarget"/></param>
        /// <param name="ignoreMask"> <see cref="SolutionIgnoreMask"/></param>
        /// <param name="showErrorMassage">Show error message, if error occured</param>
        public bool TryJumpToTarget(Matrix4x4 target, SolutionIgnoreMask ignoreMask, bool showErrorMassage = true)
        {
            var worldTarget = _controller.FrameToWorld(target, _controller.Frame.Value);
            var solution = ComputeInverse(worldTarget, _controller.Tool.Value, _controller.Configuration.Value, _controller.MechanicalGroup.JointState.ExtJoint, ignoreMask);
            return TryApplySolution(solution, showErrorMassage);
        }

        /// <summary>
        /// Try apply inverse kinematic solution
        /// </summary>
        /// <param name="solution">target <see cref="IKSolution">solution</see></param>
        /// <param name="log">log exceptions in console</param>
        public bool TryApplySolution(IKSolution solution, bool log = true)
        {
            try
            {
                if (!_controller.IsValid.Value) throw new Exception("Controller isn't valid!");
                if (!solution.IsValid) throw solution.Exception;
                _controller.Configuration.Value = solution.Configuration;
                _controller.MechanicalGroup.SetJoints(solution.JointTarget, true);
#if UNITY_EDITOR
                if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(_controller);
#endif
                return true;
            }
            catch (Exception exception)
            {
                if (!log) return false;
                Logger.Log(LogType.Error, exception.Message);
                return false;
            }
        }

        /// <summary>
        /// Get robot configuration 
        /// </summary>
        /// <param name="jointTarget">Target joint values</param>
        public Configuration GetConfiguration(JointTarget jointTarget)
        { 
            var configuration = _controller.Configuration.Value;
            configuration.SetIndex(_controller.MechanicalGroup.GetConfigurationIndex(jointTarget));
            return configuration;
        }
        
        /// <summary>
        /// Get robot configuration base on actual robot state
        /// </summary>
        public Configuration GetConfiguration()
        {
            var configuration = _controller.Configuration.Value;
            configuration.SetIndex(_controller.MechanicalGroup.GetConfigurationIndex());
            return configuration;
        }
    }
}
