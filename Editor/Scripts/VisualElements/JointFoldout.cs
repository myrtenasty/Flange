using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Preliy.Flange.Editor
{
    public class JointFoldout : Foldout, IDisposable
    {
        private const string USS = "USS/Inspector";
        private const string CONTAINER_USS = "container";
        private readonly List<JointSlider> _sliders;

        public JointFoldout(IReadOnlyList<TransformJoint> joints, string prefix = "J", string label = null)
        {
            if (string.IsNullOrEmpty(label)) label = "Joints";

            text = label;
            name = "jointsFoldout";

            styleSheets.Add(Resources.Load<StyleSheet>(USS));
            AddToClassList(CONTAINER_USS);

            _sliders = new List<JointSlider>();
            for (var i = 0; i < joints.Count; i++)
            {
                if (joints[i] == null) continue;

                var index = i;
                var slider = new JointSlider(joints[i], $"{prefix}{i+1}", sliderValue => joints[index].Position.Value = sliderValue);
                Add(slider);
                _sliders.Add(slider);
            }
        }

        public void Dispose()
        {
            foreach (var slider in _sliders)
            {
                slider.Dispose();
            }
        }
    }
}
