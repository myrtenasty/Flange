using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Preliy.Flange.Editor
{
    public class ControllerInspectorPanel : VisualElement
    {
        private const string UXML = "UXML/ControllerInspector";
        private const string USS = "USS/Inspector";
        private const float METER_TO_MILLIMETER = 1000f;
        private const float MILLIMETER_TO_METER = 0.001f;
        
        private Controller _controller;
        
        private readonly VisualElement _emptyContainer;
        private readonly VisualElement _controlContainer;
        private readonly VisualElement _jointContainer;
        private readonly List<JointSlider> _jointSliders = new();
        private readonly IntegerField _toolField;
        private readonly IntegerField _frameField;
        private readonly Vector3Field _positionField;
        private readonly Vector3Field _rotationField;
        private readonly TextField _configurationField;
        private readonly Button _configurationSelectButton;
        private readonly Toggle _configurationTurn;
        private readonly Button _saveJointsButton;
        private readonly Button _loadJointsButton;
        private readonly ObjectField _baseMechanicalUnitField;

        public ControllerInspectorPanel()
        {
            Resources.Load<VisualTreeAsset>(UXML).CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>(USS));
            
            _emptyContainer = this.Q<VisualElement>("empty");
            _controlContainer = this.Q<VisualElement>("control");
            
            _jointContainer = this.Q("joints");
            _toolField = this.Q<IntegerField>("tool");
            _frameField = this.Q<IntegerField>("frame");
            _configurationField = this.Q<TextField>("configuration");
            
            _positionField = this.Q<Vector3Field>("position").SetDelayed(true).SetFormatString("f1");
            _rotationField = this.Q<Vector3Field>("rotation").SetDelayed(true).SetFormatString("f3");
            
            _baseMechanicalUnitField = this.Q<ObjectField>("base");
            
            _configurationSelectButton = this.Q<Button>("configurationSelect");
            _configurationTurn = this.Q<Toggle>("configurationTurn");
            _saveJointsButton = this.Q<Button>("save");
            _loadJointsButton = this.Q<Button>("home");

            _toolField.AlignedField();
            _frameField.AlignedField();
            _configurationField.AlignedField();
            _configurationTurn.AlignedField();
            _positionField.AlignedField();
            _rotationField.AlignedField();

            this.Q<ObjectField>("robot").AlignedField(); 
            this.Q<ObjectField>("base").AlignedField(); 
        }

        public void Bind(Controller controller, SerializedObject serializedObject)
        {
            _controller = controller;
            this.Bind(serializedObject);

            CreateJointSlider();

            _saveJointsButton.clicked += SaveJointValue;
            _loadJointsButton.clicked += LoadJointValue;

            _configurationSelectButton.clicked += () => 
            {
                UnityEditor.PopupWindow.Show(_configurationSelectButton.worldBound, new ConfigurationPopup(_controller, _configurationTurn.value));
            };
            
            _controller.IsValid.Subscribe(SetControl);
            _controller.Tool.Subscribe(value => _toolField.SetValueWithoutNotify(value));
            _controller.Frame.Subscribe(value => _frameField.SetValueWithoutNotify(value));
            _controller.Configuration.Subscribe(value => _configurationField.SetValueWithoutNotify(value.ToString()));
            _controller.PoseObserver.ToolCenterPointFrame.Subscribe(OnTcpChanged);
            
            _baseMechanicalUnitField.SetValueWithoutNotify(_controller.MechanicalGroup.BaseMechanicalUnit);
            _baseMechanicalUnitField.RegisterValueChangedCallback(OnBaseMechanicalUnitChanged);

            _toolField.RegisterValueChangedCallback(ToolFieldCallback);
            _frameField.RegisterValueChangedCallback(FrameFieldCallback);
            _positionField.RegisterCallback<ChangeEvent<Vector3>>(PoseFieldCallback);
            _rotationField.RegisterCallback<ChangeEvent<Vector3>>(PoseFieldCallback);
        }

        public void Dispose()
        {
            foreach (var slider in _jointSliders)
            {
                slider.Dispose();
            }

            _saveJointsButton.clicked -= SaveJointValue;
            _loadJointsButton.clicked -= LoadJointValue;
            
            _controller.IsValid.Unsubscribe(SetControl);
            _controller.Tool.Unsubscribe(value => _toolField.SetValueWithoutNotify(value));
            _controller.Frame.Unsubscribe(value => _frameField.SetValueWithoutNotify(value));
            _controller.Configuration.Unsubscribe(value => _configurationField.SetValueWithoutNotify(value.ToString()));
            _controller.PoseObserver.ToolCenterPointFrame.Unsubscribe(OnTcpChanged);
            
            _toolField.UnregisterValueChangedCallback(ToolFieldCallback);
            _frameField.UnregisterValueChangedCallback(FrameFieldCallback);
            _positionField.UnregisterCallback<ChangeEvent<Vector3>>(PoseFieldCallback);
            _rotationField.UnregisterCallback<ChangeEvent<Vector3>>(PoseFieldCallback);
        }

        private void SetControl(bool enable)
        {
            _emptyContainer.SetEnabled(!enable);
            _emptyContainer.SetDisplay(!enable);
            _controlContainer.SetEnabled(enable);
            _controlContainer.SetDisplay(enable);
        }

        private void ToolFieldCallback(ChangeEvent<int> evt)
        {
            var value = Mathf.Clamp(evt.newValue, 0, _controller.Tools.Count);
            _toolField.SetValueWithoutNotify(value);
            Undo.RecordObject(_controller, "Tool index changed");
            _controller.Tool.Value = value;
        }
        
        private void FrameFieldCallback(ChangeEvent<int> evt)
        {
            var value = Mathf.Clamp(evt.newValue, -1, _controller.Frames.Count);
            _frameField.SetValueWithoutNotify(value);
            Undo.RecordObject(_controller, "Frame index changed");
            _controller.Frame.Value = value;
        }

        private void PoseFieldCallback(ChangeEvent<Vector3> evt)
        {
            var target = Matrix4x4.TRS(_positionField.value * MILLIMETER_TO_METER, Quaternion.Euler(_rotationField.value), Vector3.one);
            Undo.RecordObject(_controller, $"Target pose changed {target}");
            _controller.Solver.TryJumpToTarget(target, SolutionIgnoreMask.None, false);
        }

        private void OnTcpChanged(Matrix4x4 tcp)
        {
            if (!tcp.ValidTRS()) return;
            _positionField.SetValueWithoutNotify(tcp.GetPosition() * METER_TO_MILLIMETER);
            _rotationField.SetValueWithoutNotify(tcp.rotation.eulerAngles);
        }

        private void CreateJointSlider()
        {
            foreach (var slider in _jointSliders)
            {
                slider.RemoveFromHierarchy();
                slider.Dispose();
            }
            _jointSliders.Clear();


            if (!_controller.MechanicalGroup.IsValid)
            {
                SetControl(false);
                return;
            }
            
            SetControl(true);
            
            if (_controller.MechanicalGroup.RobotJoints.Count == 0)
            {
                _jointContainer.SetDisplay(false);
                return;
            }
            
            _jointContainer.SetDisplay(true);

            for (var i = 0; i < _controller.MechanicalGroup.RobotJoints.Count; i++)
            {
                var index = i;
                var slider = new JointSlider(_controller.MechanicalGroup.RobotJoints[i], $"J{i + 1}"
                    , value => {
                        Undo.RecordObject(_controller, $"Set Robot joint {index} value to {value}");
                        _controller.MechanicalGroup.SetJoint(index, value, true, true);
                    });
                _jointContainer.Add(slider);
                _jointSliders.Add(slider);
            }

            for (var i = 0; i < _controller.MechanicalGroup.ExternalJoints.Count; i++)
            {
                var index = i;
                var slider = new JointSlider(_controller.MechanicalGroup.ExternalJoints[i], $"E{i + 1}"
                    , value => {
                        Undo.RecordObject(_controller, $"Set External joint {index} value to {value}");
                        _controller.MechanicalGroup.SetJoint(6 + index, value, true, true);
                    });
                _jointContainer.Add(slider);
                _jointSliders.Add(slider);
            }
        }
        
        private void SaveJointValue()
        {
            if (_controller != null) _controller.MechanicalGroup.SaveState();
        }

        private void LoadJointValue()
        {
            if (_controller != null) _controller.MechanicalGroup.LoadState();
        }
        
        private void OnBaseMechanicalUnitChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue == evt.previousValue) return;
            var baseUnit = (MechanicalUnit)evt.newValue;
            
            if (baseUnit == null)
            {
                var oldBaseUnit = (MechanicalUnit)evt.previousValue;
                if (oldBaseUnit == null) return;
                if (_controller.MechanicalGroup.Robot == null) return;
                _controller.MechanicalGroup.Robot.transform.parent = oldBaseUnit.transform.parent;
            }
            else
            {
                if (_controller.MechanicalGroup.Robot == null) return;
                _controller.MechanicalGroup.Robot.transform.parent = baseUnit.Frames.Last().Transform;
                _controller.MechanicalGroup.Robot.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            
            _controller.MechanicalGroup.OnValidate();
            CreateJointSlider();
        }
    }
}
