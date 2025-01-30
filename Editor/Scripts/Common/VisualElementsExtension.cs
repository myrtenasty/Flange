using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    public static class VisualElementsExtension
    {
        public static VisualElement SetDisplay(this VisualElement visualElement, bool display)
        {
            visualElement.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
            return visualElement;
        }
        
        public static Vector3Field SetDelayed(this Vector3Field field, bool isDelayed)
        {
            var floatFields = field.Query<FloatField>().ToList();
            foreach (var item in floatFields)
            {
                item.isDelayed = isDelayed;
            }
            return field;
        }
        
        public static Vector3Field SetReadOnly(this Vector3Field field, bool isReadOnly)
        {
            var floatFields = field.Query<FloatField>().ToList();
            foreach (var item in floatFields)
            {
                item.isReadOnly = isReadOnly;
            }
            return field;
        }

        public static Vector3Field SetFormatString(this Vector3Field field, string format)
        {
            var floatFields = field.Query<FloatField>().ToList();
            foreach (var item in floatFields)
            {
                item.formatString = format;
            }
            return field;
        }
        
        public static void SetDelayed(this Slider field, bool isDelayed)
        {
            field.Q<TextField>("unity-text-field").isDelayed = isDelayed;
        }

        public static VisualElement FlexDirection(this VisualElement ve, FlexDirection flexDirection)
        {
            ve.style.flexDirection = new StyleEnum<FlexDirection>(flexDirection);
            return ve;
        }
        
        public static VisualElement JustifyContent(this VisualElement ve, Justify justify)
        {
            ve.style.justifyContent = new StyleEnum<Justify>(justify);
            return ve;
        }
    }
}
