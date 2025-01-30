using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Preliy.Flange.Editor
{
    public static class SceneToolUtils
    {
        public static void Snap()
        {
            var currentEvent = Event.current;
            if (!currentEvent.isKey) return;
            switch (currentEvent.type)
            {
                case EventType.KeyDown when currentEvent.keyCode == KeyCode.V:
                {
                    if (Tools.current == UnityEditor.Tool.Move) SetHidden(true);
                    break;
                }
                case EventType.KeyUp when currentEvent.keyCode == KeyCode.V:
                    SetHidden(false);
                    break;
            }
        }
        
        private static void SetHidden(bool value)
        {
            var type = typeof(Tools);
            var field = type.GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null) field.SetValue(null, value);
        }
    }
}
