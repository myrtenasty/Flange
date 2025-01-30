using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Preliy.Flange
{    
    [AddComponentMenu("Robotics/Controller")]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [SelectionBase]
    public class Controller : MonoBehaviour
    {
        public IProperty<bool> IsValid => _isValid;
        public MechanicalGroup MechanicalGroup => _mechanicalGroup ?? new MechanicalGroup(this);
        public Solver Solver => _solver ?? new Solver(this);
        public IProperty<int> Tool => _tool;
        public IProperty<int> Frame => _frame;
        public IProperty<Configuration> Configuration => _configuration;
        public List<ReferenceFrame> Frames => _frames;
        public IReadOnlyList<Tool> Tools => _tools;
        public PoseObserver PoseObserver => _poseObserver;
        public float SampleTime => 0.02f;

        [SerializeField]
        private Property<int> _tool = new (0);
        [SerializeField]
        private Property<int> _frame = new (0);
        [SerializeField]
        private Property<Configuration> _configuration = new (Flange.Configuration.Default);

        [SerializeField]
        private List<Tool> _tools = new ();
        [SerializeField]
        private List<ReferenceFrame> _frames = new ();
        [SerializeField]
        private MechanicalGroup _mechanicalGroup;

        [HideInInspector]
        [SerializeField]
        private PoseObserver _poseObserver;
        
        [HideInInspector]
        [SerializeField]
        private Property<bool> _isValid = new ();

        private Solver _solver;
        
        private CancellationTokenSource _cancellationTokenSource;

        private void OnEnable()
        {
            MechanicalGroup.OnEnable();
            _poseObserver = new PoseObserver(this);
            
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _poseObserver?.Dispose();
            _cancellationTokenSource?.Cancel();
        }

        private void OnValidate()
        {
            try
            {
                MechanicalGroup.OnValidate();
                _isValid.Value = true;
            }
            catch (Exception exception)
            {
                Logger.Log(LogType.Error, $"Controller isn't valid! {exception}", this);
                _isValid.Value = false;
            }
        }

        private void Reset()
        {
            OnDisable();
            _mechanicalGroup = new MechanicalGroup(this);
            OnEnable();
        }
    }
}

