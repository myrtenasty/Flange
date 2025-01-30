using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    public class ConfigurationPopup : PopupWindowContent
    {
        private readonly Controller _controller;
        private readonly List<IKSolution> _solutions;
        private readonly RadioButtonGroup _radioButtonGroup;
        
        public ConfigurationPopup(Controller controller, bool turn)
        {
            _controller = controller;
            var robotTarget = new CartesianTarget(_controller.GetTcpRelativeToRefFrame(), _controller.Configuration.Value, _controller.MechanicalGroup.JointState.ExtJoint);
            _solutions = controller.Solver.GetAllSolutions(robotTarget, _controller.Tool.Value, _controller.Frame.Value, turn);
            _radioButtonGroup = new RadioButtonGroup("", _solutions.Select(solution => solution.GetLabel()).ToList());
        }
        
        public override Vector2 GetWindowSize()
        {
            return new Vector2(480, _solutions.Count * 20f);
        }

        public override void OnGUI(Rect rect)
        {
            
        }

        public override void OnOpen()
        {
            if (_controller == null) return;
            
            _radioButtonGroup.RegisterValueChangedCallback(ConfigurationChangedCallback);
            var actualSolution = _solutions.Find(item => item.Configuration == _controller.Configuration.Value);
            if (actualSolution is not null) _radioButtonGroup.SetValueWithoutNotify(_solutions.IndexOf(actualSolution));

            var scrollView = new ScrollView();
            scrollView.Add(_radioButtonGroup);
            editorWindow.rootVisualElement.Add(scrollView);
        }
        
        public override void OnClose()
        {
            _radioButtonGroup.UnregisterValueChangedCallback(ConfigurationChangedCallback);
        }

        private void ConfigurationChangedCallback(ChangeEvent<int> evt)
        {
            _controller.Solver.TryApplySolution(_solutions[evt.newValue]);
        }
    }
}
