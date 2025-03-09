using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Mathf;

namespace Preliy.Flange
{
    [AddComponentMenu("Robotics/Robot 6R Spherical Wrist")]
    [DisallowMultipleComponent]
    [SelectionBase]
    public class Robot6RSphericalWrist : Robot
    {
        private const int FRAME_COUNT = 8;
        private const int JOINT_COUNT = 6;
        private const int DEFAULT_SOLUTIONS_COUNT = 8;

        private readonly static List<int> InverseIndex = new() {2, 3, 6, 7};
        private readonly static Matrix4x4 T6G = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, -90, 0), Vector3.one);
       
        private Vector4 _p65;
        private float _link1, _link2, _link2Delta, _theta0Offset, _p05XZLength;
        private float _x, _y, _l, _alpha, _beta, _gamma;
        private Matrix4x4 _t01, _t12, _t23,_t34, _t03, _t36, _target;
        private Vector4 _p05;
        private readonly float[] _theta = new float[JOINT_COUNT];

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
        
#if UNITY_EDITOR
        [ContextMenu("Create Default Hierarchy", false, 500)]
        public void CreateDefaultHierarchy()
        {
            _frames = new List<Frame>();
            _joints = new List<TransformJoint>();

            var baseFrame = transform.GetOrCreate("Base").CreateMeshPlaceholder();
            _frames.Add(baseFrame);

            var parent = baseFrame.transform;
            for (var i = 0; i < JOINT_COUNT; i++)
            {
                var frame = parent.GetOrCreate($"Joint_{i + 1}").CreateMeshPlaceholder();
                _joints.Add(frame.TryGetComponent<TransformJoint>(out var joint) ? joint : frame.gameObject.AddComponent<TransformJoint>());
                parent = frame.transform;
                _frames.Add(frame);
            }

            var flangeFrame = parent.GetOrCreate("Flange");
            _frames.Add(flangeFrame);
        }
#endif

        public override Matrix4x4 ComputeForward()
        {
            return _frames.First().transform.GetMatrix().inverse * _frames.Last().transform.GetMatrix();
        }
        
        public override IKSolution ComputeInverse(Matrix4x4 target, Configuration configuration, SolutionIgnoreMask ignoreMask)
        {
            var solutionIndex = Clamp(configuration.Index, 0, DEFAULT_SOLUTIONS_COUNT - 1);
            
            _p65 = new Vector4(0, -_frames[6].Config.D, 0, 1);
            _link1 = _frames[2].Config.A;
            _link2 = Sqrt(_frames[4].Config.D * _frames[4].Config.D + _frames[3].Config.A * _frames[3].Config.A);
            _link2Delta = Atan2(_frames[3].Config.A, _frames[4].Config.D);
            
            //NOTE: Original calculation is target = target * _flangeOffset * _t6G.inverse. Was simplified by target *= _t6G
            target *= T6G;
            
            _p05 = target * _p65;

            //NOTE: Offset on face view (typical for Stäubli)
            _p05XZLength = Sqrt(_p05.x * _p05.x + _p05.z * _p05.z);
            _theta0Offset = Asin(_frames[3].Config.D / _p05XZLength);
            
            _theta[0] = Atan2(-_p05.x, _p05.z);
            if (float.IsNaN(_theta[0])) _theta[0] = 0;
            _theta[0] = solutionIndex < 4 ? _theta[0] - _theta0Offset : _theta[0] + PI + _theta0Offset;

            _x = _p05.z * Cos(_theta[0]) - _p05.x * Sin(_theta[0]) - _frames[1].Config.A;
            _y = _p05.y - _frames[1].Config.D;
            _l = Sqrt(_x * _x + _y * _y);
            _alpha = Acos((_link1 * _link1 + _link2 * _link2 - _l * _l) / (2 * _link1 * _link2));
            _beta = Acos((_link1 * _link1 - _link2 * _link2 + _l * _l) / (2 * _link1 * _l));
            _gamma = Atan2(_y, _x);

            if (InverseIndex.Contains(solutionIndex))
            {
                _theta[1] = PI * 0.5f - _gamma + _beta;
                _theta[2] = PI * 0.5f + _link2Delta + _alpha - 2 * PI;
            }
            else
            {
                _theta[1] = PI * 0.5f - _gamma - _beta;
                _theta[2] = PI * 0.5f + _link2Delta - _alpha;
            }

            _t01 = HomogeneousMatrix.Create(_frames[1].Config, _joints[0].Config, _theta[0] * Rad2Deg);
            _t12 = HomogeneousMatrix.Create(_frames[2].Config, _joints[1].Config, _theta[1] * Rad2Deg);
            _t23 = HomogeneousMatrix.Create(_frames[3].Config, _joints[2].Config, _theta[2] * Rad2Deg);
            _t03 = _t01 * _t12 * _t23;
            _t36 = _t03.inverse * target;

            _theta[3] = Atan2(_t36.m21, _t36.m01) + PI * 0.5f;
            _theta[4] = Acos(Vector3.Dot(_t03.GetColumn(1), target.GetColumn(1)));
            _theta[4] = _theta[4].Round(6);
            _theta[5] = Atan2(-_t36.m12, _t36.m10) + PI;

            if (Abs(_theta[4]) < 1e-6)
            {
                _theta[3] = 0;
            }
            else if (_theta[4] > 0)
            {
                if (solutionIndex % 2 != 0)
                {
                    _theta[3] += PI;
                    _theta[5] += PI;
                }
            }
            else
            {
                if (solutionIndex % 2 != 0)
                {
                    _theta[3] -= PI;
                    _theta[5] -= PI;
                }
            }

            if (solutionIndex % 2 != 0) _theta[4] *= -1;
            
            _theta[3] = Math.ClampPI(_theta[3]);
            _theta[4] = Math.ClampPI(_theta[4]);
            _theta[5] = Math.ClampPI(_theta[5]);

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
            var isDown = Vector3.SignedAngle(directionJ3, directionJ5, frame1.GetUpDirection()) > 0.0f;
            var isFlip = jointValues[4] < 0.0f;

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
            var isBack = _joints[0].transform.InverseTransformPoint(_joints[4].transform.position).z < 0.0f;
            var directionJ3 = _joints[1].transform.position - _joints[0].transform.position; 
            var directionJ5 = _joints[3].transform.position - _joints[0].transform.position; 
            var isDown = Vector3.SignedAngle(directionJ3, directionJ5, _joints[0].transform.up) > 0.0f;
            var isFlip = _joints[4].Position.Value < 0.0f;

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

            if (!ignoreMask.HasFlag(SolutionIgnoreMask.Singularity))
            {
                if (Abs(theta[4]) < Math.SINGULARITY_ANGLE_LIMIT)
                {
                    return new IKSolution("Joint 5 is in singularity range");
                }

                // var xyNorm = new Vector2(_p05.x, _p05.z);
                // if (xyNorm.magnitude < Math.SINGULARITY_NORM_LIMIT)
                // {
                //     return new InvKinSolution(InvKinSolutionState.ErrorSingularity, "Joint 1 and Joint 6 is in singularity range");
                // }
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
