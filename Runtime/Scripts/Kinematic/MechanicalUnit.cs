using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
    using System.IO;
    using UnityEditor;
#endif

namespace Preliy.Flange
{
    [DisallowMultipleComponent]
    [SelectionBase]
    [ExecuteAlways]
    public class MechanicalUnit : MonoBehaviour, IMechanicalUnit
    {
        public Component Component => this;
        public IReadOnlyList<TransformJoint> Joints => _joints;
        public IReadOnlyList<Frame> Frames => _frames;
        public Matrix4x4 WorldTransform => _worldTransform;

        public JointTarget JointValue
        {
            get => new (_joints.GetJointValues());
            set
            {
                _joints.SetJointValues(value);
                CacheWorldTransform();
            }
        }
        
        public float this[int index]
        {
            get => _joints[index].Position.Value;
            set
            {
                _joints[index].Position.Value = value;
                CacheWorldTransform();
            }
        }
        
        public event Action OnJointStateChanged;

        [Header("References")]
        [SerializeField]
        protected List<Frame> _frames = new ();
        [SerializeField]
        protected List<TransformJoint> _joints = new ();

        [Header("Gizmos")]
        [SerializeField]
        private bool _showGizmos;
        [SerializeField]
        private GizmosConfig _gizmosConfig = GizmosConfig.Default;
        [SerializeField]
        private bool _debug;

        [HideInInspector]
        [SerializeField]
        private Matrix4x4 _worldTransform;

        private bool _isJointStateChanged;
        private List<TransformJoint> _lastJoints = new ();

        private void OnEnable()
        {
            CacheWorldTransform();
            SubscribeJoints();
        }

        private void OnDisable()
        {
            UnsubscribeJoints();
        }

        private void Update()
        {
            if (!Application.isPlaying) OnStateChanged();
            CheckTransformChange();
        }

        private void FixedUpdate()
        {
            if (Application.isPlaying) OnStateChanged();
        }

        private void OnValidate()
        {
            CacheWorldTransform();
            OnValidateJoints();
        }

        private void Reset()
        {
            _frames = GetComponentsInChildren<Frame>().ToList();
            _joints = GetComponentsInChildren<TransformJoint>().ToList();

            foreach (var frame in _frames)
            {
                frame.Config = frame.Config with { };
            }
            
            foreach (var joint in _joints)
            {
                joint.Config = joint.Config with { };
            }
        }
        
        private void CheckTransformChange()
        {
            if (!transform.hasChanged) return;
            transform.hasChanged = false;
            CacheWorldTransform();
        }
        
        private void CacheWorldTransform()
        {
            _worldTransform = transform.GetMatrix();
        }
        
        private void OnValidateJoints()
        {
            if (_joints.SequenceEqual(_lastJoints)) return;
            _lastJoints = new List<TransformJoint>(_joints);
            SubscribeJoints();
        }

        protected virtual void OnStateChanged()
        {
            if (!_isJointStateChanged) return;
            _isJointStateChanged = false;
            OnJointStateChanged?.Invoke();
        }

        private void SubscribeJoints()
        {
            UnsubscribeJoints();
            foreach (var joint in _joints)
            {
                if (joint == null) continue;
                joint.Position.Subscribe(SetJointStateDirty); 
            }
        }

        private void SetJointStateDirty(float value)
        {
            _isJointStateChanged = true;
        }
        
        private void UnsubscribeJoints()
        {
            foreach (var joint in _joints)
            {
                if (joint == null) continue;
                joint.Position.Unsubscribe(SetJointStateDirty); 
            }
        }
        
        /// <summary>
        /// Calculate forward kinematic with actual joint values
        /// </summary>
        /// <returns>
        /// <see cref="Matrix4x4">Pose</see> in base coordinate system (BCS)
        /// </returns>
        public virtual Matrix4x4 ComputeForward(float[] value)
        {
            try
            {
                if (value is null) throw new ArgumentNullException(nameof(value));
                
                var result = HomogeneousMatrix.CreateRaw(_frames.First().Config);
                for (var i = 0; i < _joints.Count; i++)
                {
                    result *= HomogeneousMatrix.Create(_frames[i+1].Config, _joints[i].Config, value[i]);
                }
                result *= HomogeneousMatrix.CreateRaw(_frames.Last().Config);
                return result;
            }
            catch (Exception exception)
            {
                Logger.Log(LogType.Error, exception.Message, this);
                throw;
            }
        }

#if UNITY_EDITOR
        public virtual void DrawDebugGizmos()
        {
            
        }
#endif

#if UNITY_EDITOR
        [ContextMenu("Reset joints to zero", false, 520)]
        public void ResetJointsToZero()
        {
            foreach (var joint in _joints)
            {
                joint.Position.Value = 0;
            }
        }
#endif
        
#if UNITY_EDITOR
        [ContextMenu("Save Configuration", false, 510)]
        public void SaveConfiguration()
        {
            try
            {
                var defaultName =  $"{gameObject.name} Config";
                var path = EditorUtility.SaveFilePanelInProject("Save Configuration", defaultName, "asset", "Please enter a file name to save the configuration to");
                if (string.IsNullOrEmpty(path)) return;

                var framesConfig = _frames.Select(frame => frame.Config).ToList();
                var jointsConfig = _joints.Select(joint => joint.Config).ToList();

                for (var i = 0; i < _frames.Count; i++)
                {
                    framesConfig[i].Name = _frames[i].name;
                }
            
                for (var i = 0; i < _joints.Count; i++)
                {
                    jointsConfig[i].Name = _joints[i].name;
                }

                var asset = ScriptableObject.CreateInstance<MechanicalUnitConfig>();
                asset.name = Path.GetFileName(path);
                asset.Initialize(framesConfig, jointsConfig, null);
                
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                EditorGUIUtility.PingObject(asset);
            }
            catch (Exception exception)
            {
                Logger.Log(LogType.Error, exception.Message, this);
            }
        }

        [ContextMenu("Load Configuration", false, 511)]
        public void LoadConfiguration()
        {
            try
            {
                var path = EditorUtility.OpenFilePanel("Load Configuration", "Assets", "asset");
                if (string.IsNullOrEmpty(path)) return;

                path = Path.GetRelativePath(Application.dataPath.Replace("/Assets", ""), path);
                var config = (MechanicalUnitConfig)AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
                if (config == null) throw new Exception("Selected Asset type isn't valid!");

                if (config.Frames.Count != _frames.Count) throw new Exception("Frames count isn't match with config data!");
                if (config.Joints.Count != _joints.Count) throw new Exception("Joints count isn't match with config data!");

                for (var i = 0; i < _frames.Count; i++)
                {
                    _frames[i].Config = config.Frames[i];
                }
                
                for (var i = 0; i < _joints.Count; i++)
                {
                    _joints[i].Config = config.Joints[i];
                }
                
                EditorUtility.SetDirty(this);
            }
            catch (Exception exception)
            {
                Logger.Log(LogType.Error, exception.Message, this);
            }
        }
#endif

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR 
            if (_debug) DrawDebugGizmos();
            
            if (!_showGizmos) return;

            foreach (var frame in _frames)
            {
                if (frame == null) continue;
                Gizmos.matrix = frame.transform.localToWorldMatrix;
                Handles.matrix = frame.transform.localToWorldMatrix;
                GizmosUtils.DrawHandle(Matrix4x4.identity, _gizmosConfig.Scale);
                GizmosUtils.DrawFrameOffset(frame, _gizmosConfig.PointColor, _gizmosConfig.LineColor, _gizmosConfig.Scale);
            }
#endif
        }
    }
}
