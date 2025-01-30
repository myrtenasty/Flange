using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Search;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    [Overlay(typeof(SceneView), "Controller", true)]
    public class ControllerOverlay : Overlay, ITransientOverlay
    {
        public bool visible => Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Controller>() != null;

        private Controller _controller;
        private Toggle _showHandlesToggle;

        public override VisualElement CreatePanelContent()
        {
            if (Selection.activeGameObject == null)
            {
                displayed = false;
                return new VisualElement();
            }

            _controller = Selection.activeGameObject.GetComponent<Controller>();
            if (_controller == null)
            {
                displayed = false;
                return new VisualElement();
            }

            SessionState.SetString("Selected_Controller", SearchUtils.GetHierarchyPath(_controller.gameObject, false));
            
            var container = new VisualElement();

            _showHandlesToggle = new Toggle("Show Handles")
            {
                value = EditorPrefs.GetBool("ControllerShowHandles")
            };
            _showHandlesToggle.RegisterValueChangedCallback(OnShowHandleChangeEvent);

            container.Add(_showHandlesToggle);
            return container;
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
            _showHandlesToggle.UnregisterValueChangedCallback(OnShowHandleChangeEvent);
        }
        
        private static void OnShowHandleChangeEvent(ChangeEvent<bool> evt)
        {
            EditorPrefs.SetBool("ControllerShowHandles", evt.newValue);
        }
    }
}
