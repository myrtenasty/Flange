using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Mathf;

namespace Preliy.Flange
{
    [AddComponentMenu("Robotics/Robot 3D Delta")]
    [DisallowMultipleComponent]
    [SelectionBase]
    public class Robot3DDelta : Robot
    {
        private const int FRAME_COUNT = 14;
        private const int JOINT_COUNT = 3;
        private const int DEFAULT_SOLUTIONS_COUNT = 1;

        private Vector4 _p65;
        private float _l1, _l2, _delta;
        private float _x, _y, _l, _alpha, _beta, _gamma;
        private Matrix4x4 _t01, _t12, _t23,_t34, _t03, _t36, _target;
        private Vector3 _p05, _p06, _p3Dist;
        private readonly float[] _theta = new float[JOINT_COUNT];

        private Matrix4x4 _t14, _t16, _t45, _t56, _t61;
        private Matrix4x4 _t5;
        // ReSharper disable once InconsistentNaming
        private float _p5xz;
        private float _psi, _phi;
        private float _sin1, _cos1, _p13Dist;
        
        private const float TAN60 = 1.73205080757f;
        private const float SIN30 = 0.5f;

        private void Reset()
        {
            _frames = new List<Frame>();
            for (var i = 0; i < FRAME_COUNT; i++)
            {
                _frames.Add(null);
            }

            _joints = new List<TransformJoint>();
            for (var i = 0; i < JOINT_COUNT; i++)
            {
                _joints.Add(null);
            }
        }
        
        protected override void OnStateChanged()
        {
            base.OnStateChanged();
            
            try
            {
                var pose = _frames.First().transform.GetMatrixLocal() * ComputeForward();
                _frames.Last().transform.SetLocalMatrix(pose);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        
        public override Matrix4x4 ComputeForward()
        {
            return ComputeForward(_joints.GetJointValues());
        }

        public override Matrix4x4 ComputeForward(float[] value)
        {
            for (var i = 0; i < _theta.Length; i++)
            {
                _theta[i] = value[i] * Deg2Rad;
            }

            var t = _frames[1].Config.A - _frames[10].Config.A;
 
            var y1 = -(t + _frames[2].Config.A * Cos(_theta[0]));
            var z1 = -_frames[2].Config.A * Sin(_theta[0]);
 
            var y2 = (t + _frames[2].Config.A * Cos(_theta[1])) * SIN30;
            var x2 = y2 * TAN60;
            var z2 = -_frames[2].Config.A * Sin(_theta[1]);
 
            var y3 = (t + _frames[2].Config.A * Cos(_theta[2])) * SIN30;
            var x3 = -y3 * TAN60;
            var z3 = -_frames[2].Config.A * Sin(_theta[2]);
 
            var dnm = (y2 - y1) * x3 - (y3 - y1) * x2;
 
            var w1 = y1 * y1 + z1 * z1;
            var w2 = x2 * x2 + y2 * y2 + z2 * z2;
            var w3 = x3 * x3 + y3 * y3 + z3 * z3;
     
            // x = (a1*z + b1)/dnm
            var a1 = (z2 - z1) * (y3 - y1) - (z3 - z1) * (y2 - y1);
            var b1 = -((w2 - w1) * (y3 - y1) - (w3 -w1) * (y2 -y1)) / 2.0f;
 
            // y = (a2*z + b2)/dnm;
            var a2 = -(z2 -z1) * x3 + (z3 -z1) * x2;
            var b2 = ((w2 - w1) *x3 - (w3 - w1) * x2) / 2.0f;
 
            // a*z^2 + b*z + c = 0
            var a = a1 * a1 + a2 * a2 + dnm * dnm;
            var b = 2 * (a1 * b1 + a2 * (b2 - y1 * dnm) - z1 * dnm * dnm);
            var c = (b2 - y1 * dnm) * (b2 -y1 * dnm) + b1 * b1 + dnm * dnm * (z1 * z1 - _frames[3].Config.A * _frames[3].Config.A);

            var d = b * b - 4.0f * a * c;
            if (d < 0)
            {
                throw new Exception("No solution");
            }
 
            var z0 = -0.5f * (b + Sqrt(d)) / a;
            var x0 = (a1 * z0 + b1) / dnm;
            var y0 = (a2 * z0 + b2) / dnm;
            
            return Matrix4x4.TRS(new Vector3(x0, z0 + _frames[1].Config.D, y0), Quaternion.identity, Vector3.one);
        }

        private float GetAngleOnZYPlane(Vector3 position)
        {
            var x0 = position.x;
            var y0 = position.y - _frames[10].Config.A;
            var z0 = position.z;
            
            var y1 = -_frames[1].Config.A;

            var a = (x0 * x0 + y0 * y0 + z0 * z0 + _frames[2].Config.A * _frames[2].Config.A - _frames[3].Config.A * _frames[3].Config.A - y1 * y1)/(2 * z0);
            var b = (y1 - y0) / z0;
            var d = -(a + b * y1) * (a + b * y1) + _frames[2].Config.A * (b * b * _frames[2].Config.A + _frames[2].Config.A);
            if (d < 0)
            {
                return float.NaN;
            }

            var y = (y1 - a * b - Sqrt(d)) / (b * b + 1);
            var z = a + b * y;
            var angle = Atan2(-z, y1 - y);
            return angle;
        }

        public override IKSolution ComputeInverse(Matrix4x4 target, Configuration configuration, SolutionIgnoreMask ignoreMask)
        {
            var position = target.GetPosition();
            position = new Vector3(position.x, position.z, position.y - _frames[1].Config.D);

            _theta[0] = GetAngleOnZYPlane(position);
            _theta[1] = GetAngleOnZYPlane(Quaternion.AngleAxis(-120, Vector3.forward) * position);
            _theta[2] = GetAngleOnZYPlane(Quaternion.AngleAxis(120, Vector3.forward) * position);

            return CreateSolution(_theta, configuration, ignoreMask);
        }
        
        public override List<IKSolution> ComputeInverse(Matrix4x4 target, bool turn, SolutionIgnoreMask ignoreMask)
        {
            var solutions = new List<IKSolution>();
            for (var i = 0; i < DEFAULT_SOLUTIONS_COUNT; i++)
            {
                try
                {
                    var configuration = new Configuration(0,0,0,i);
                    var solution = ComputeInverse(target, configuration, ignoreMask);
                    if (solution.IsValid)
                    {
                        if (turn)
                        {
                            solutions.AddRange(GetSolutionsWithTurns(solution, i));
                        }
                        else
                        {
                            solutions.Add(solution);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
            return solutions;
        }
        
        public override int GetConfigurationIndex(float[] jointValues)
        {
            var frame1 = HomogeneousMatrix.Create(_frames[1].Config, _joints[0].Config, jointValues[0]);
            var frame2 = frame1 * HomogeneousMatrix.Create(_frames[2].Config, _joints[1].Config, jointValues[1]);
            var frame3 = frame2 * HomogeneousMatrix.Create(_frames[3].Config, _joints[2].Config, jointValues[2]);
            var frame4 = frame3 * HomogeneousMatrix.Create(_frames[4].Config, _joints[3].Config, jointValues[3]);
            var frame5 = frame4 * HomogeneousMatrix.Create(_frames[5].Config, _joints[4].Config, jointValues[4]);
            var frame6 = frame5 * HomogeneousMatrix.Create(_frames[6].Config, _joints[5].Config, jointValues[5]);

            var diff = frame1.inverse.MultiplyPoint3x4(frame6.GetPosition());
            var isBack = diff.z < 0.0f;
            var directionJ3 = frame2.GetPosition() - frame1.GetPosition(); 
            var directionJ5 = frame4.GetPosition() - frame1.GetPosition(); 
            var isDown = Vector3.SignedAngle(directionJ3, directionJ5, frame1.GetUpDirection()) < 0.0f;
            var isFlip = Abs(jointValues[3]) > 0f;

            var bitArray = new BitArray(3)
            {
                [2] = isBack,
                [1] = isDown,
                [0] = isFlip
            };
            var array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];
        }
        
        public override int GetConfigurationIndex()
        {
            var isBack = _joints[0].transform.InverseTransformPoint(_joints[4].transform.position).z > 0.0f;
            var directionJ3 = _joints[1].transform.position - _joints[0].transform.position; 
            var directionJ5 = _joints[3].transform.position - _joints[0].transform.position; 
            var isDown = Vector3.SignedAngle(directionJ3, directionJ5, _joints[0].transform.up) > 0.0f;
            var isFlip = Abs(_joints[3].Position.Value) < 0f;

            var bitArray = new BitArray(3)
            {
                [2] = isBack,
                [1] = isDown,
                [0] = isFlip
            };
            var array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];
        }
        
#if UNITY_EDITOR
        public override void DrawDebugGizmos()
        {
            
        }
#endif
        
        private IKSolution CreateSolution(float[] theta, Configuration configuration, SolutionIgnoreMask ignoreMask)
        {
            for (var i = 0; i < theta.Length; i++)
            {
                if (float.IsNaN(_theta[i])) return IKSolution.IKSolutionNaN;
                theta[i] *= Rad2Deg;
                theta[i] = theta[i].Round(3);
            }
            
            var jointTarget = new JointTarget(theta);

            for (var i = 0; i < theta.Length; i++)
            {
                theta[i] = theta[i].Round(3);

                if (!_joints[i].IsInRange(theta[i]))
                {
                    return new IKSolution($"Joint {i + 1} value {theta[i]} is out of range [{_joints[i].Config.Limits.ToString()}]");
                }
            }
            
            //TODO Solution Check
            if (!ignoreMask.HasFlag(SolutionIgnoreMask.Singularity))
            {
                //Some check
            }
            
            var solution = new IKSolution(jointTarget, configuration);
            solution.Validate(this);
            return solution;
        }
        
        private IEnumerable<IKSolution> GetSolutionsWithTurns(IKSolution defaultSolution, int cfx)
        {
            var solutions = new List<IKSolution>();
            var configurations = new List<Configuration>();

            List<int> turnsJ0 = JointUtils.GetTurns(_joints[0].Config, defaultSolution.JointTarget[0]);
            List<int> turnsJ3 = JointUtils.GetTurns(_joints[3].Config, defaultSolution.JointTarget[3]);
            List<int> turnsJ5 = JointUtils.GetTurns(_joints[5].Config, defaultSolution.JointTarget[5]);

            foreach (var j0 in turnsJ0)
            {
                foreach (var j3 in turnsJ3)
                {
                    foreach (var j5 in turnsJ5)
                    {
                        var configuration = new Configuration(j0, j3, j5, cfx);
                        if (configurations.Contains(configuration)) continue;
                        configurations.Add(configuration);
                    }
                }
            }

            foreach (var configuration in configurations)
            {
                var solution = new IKSolution(defaultSolution.JointTarget, configuration);
                solution.Validate(this);
                if (solution.IsValid) solutions.Add(solution);
            }

            return solutions;
        }
    }
}
