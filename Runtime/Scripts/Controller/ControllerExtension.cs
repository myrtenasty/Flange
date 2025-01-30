using System;
using System.Linq;
using UnityEngine;

namespace Preliy.Flange
{
    public static class ControllerExtension
    {
        public static Matrix4x4 AddToolOffset(this Controller controller, Matrix4x4 matrix, int index)
        {
            return matrix * GetToolOffset(controller, index);
        }

        public static Matrix4x4 RemoveToolOffset(this Controller controller, Matrix4x4 matrix, int index)
        {
            return matrix * GetToolOffset(controller, index).inverse;
        }
        
        public static Matrix4x4 GetToolOffset(this Controller controller, int index)
        {
            index = GetValidToolIndex(controller, index);
            return index > 0 ? controller.Tools[index - 1].Offset : Matrix4x4.identity;
        }

        public static int GetValidToolIndex(this Controller controller, int index)
        {
            switch (index)
            {
                case < 0:
                    return Mathf.Max(0, controller.Tool.Value);
                case 0:
                    return 0;
                default:
                    try
                    {
                        if (index > controller.Tools.Count)
                        {
                            throw new IndexOutOfRangeException($"Tool Index {index} is out of range!");
                        }
                        return index;
                    }
                    catch (Exception exception)
                    {
                        Logger.Log(LogType.Error, exception.Message, controller);
                        return 0;
                    }
            }
        }

        public static Matrix4x4 FrameToWorld(this Controller controller, Matrix4x4 pose, int frame)
        {
            return controller.GetFrame(frame).GetWorldFrame() * pose;
        }
        
        public static Matrix4x4 WorldToFrame(this Controller controller, Matrix4x4 pose, int frame)
        {
            return controller.GetFrame(frame).GetWorldFrame().inverse * pose;
        }

        public static Matrix4x4 FrameToWorld(this Controller controller, Matrix4x4 pose, int frame, ExtJoint extJoint)
        {
            return controller.GetFrame(frame).GetWorldFrame(controller, extJoint) * pose;
        }

        public static Matrix4x4 WorldToFrame(this Controller controller, Matrix4x4 pose, int frame, ExtJoint extJoint)
        {
            return controller.GetFrame(frame).GetWorldFrame(controller, extJoint).inverse * pose;
        }

        public static Matrix4x4 ConvertFrame(this Controller controller, Matrix4x4 pose, int from, int to, ExtJoint extJoint)
        {
            var world = controller.FrameToWorld(pose, from, extJoint);
            return controller.WorldToFrame(world, to, extJoint);
        }

        public static Matrix4x4 GetTcpWorld(this Controller controller)
        {
            return controller.ConvertFrame(controller.PoseObserver.ToolCenterPointBase.Value, (int)CoordinateSystem.Base, (int)CoordinateSystem.World, controller.MechanicalGroup.JointState.ExtJoint);
        }

        public static Matrix4x4 GetTcpRelativeToRefFrame(this Controller controller)
        {
            return controller.ConvertFrame(controller.PoseObserver.ToolCenterPointBase.Value, (int)CoordinateSystem.Base, controller.Frame.Value, controller.MechanicalGroup.JointState.ExtJoint);
        }

        public static CartesianTarget ConvertFrame(this Controller controller, CartesianTarget target, int from = (int)CoordinateSystem.Base, int to = (int)CoordinateSystem.Base)
        {
            return target with {Pose = controller.ConvertFrame(target.Pose, from, to, target.ExtJoint)};
        }
        
        public static IReferenceFrame GetFrameActual(this Controller controller) => GetFrame(controller, controller.Frame.Value);
        
        public static IReferenceFrame GetFrame(this Controller controller, int index)
        {
            switch (index)
            {
                case < 0:
                    return new ReferenceFrameWorld();
                case 0:
                    return controller.MechanicalGroup;
                case > 0:
                    try
                    {
                        if (controller.Frames.ElementAt(index-1) == null)
                        {
                            throw new IndexOutOfRangeException($"Frame Index {index} is out of range!");
                        }
                        return controller.Frames[index-1];
                    }
                    catch (Exception exception)
                    {
                        Logger.Log(LogType.Error, exception.Message, controller);
                        return new ReferenceFrameWorld();
                    }
            }
        }
        
        public static Transform GetFrameTransform(this Controller controller, int index)
        {
            switch (index)
            {
                case < 0:
                    return null;
                case 0:
                    return controller.MechanicalGroup.Robot == null ? null : controller.MechanicalGroup.Robot.transform;
                case > 0:
                    try
                    {
                        if (controller.Frames.ElementAt(index) == null)
                        {
                            throw new IndexOutOfRangeException($"Frame Index {index} is out of range!");
                        }
                        return controller.Frames[index].transform;
                    }
                    catch (Exception exception)
                    {
                        Logger.Log(LogType.Error, exception.Message, controller);
                        return null;
                    }
            }
        }
    }
}
