using System;
using UnityEngine;

namespace Preliy.Flange
{
    [Serializable]
    public struct GizmosConfig
    {
        public float Scale => _scale;
        public Color PointColor => _pointColor;
        public Color LineColor => _lineColor;

        [Range(0.01f, 10f)]
        [SerializeField]
        private float _scale;
        [SerializeField]
        private Color _pointColor;
        [SerializeField]
        private Color _lineColor;

        public static GizmosConfig Default => new (1, Color.red, Color.white);

        public GizmosConfig(float scale, Color pointColor, Color lineColor)
        {
            _scale = scale;
            _pointColor = pointColor;
            _lineColor = lineColor;
        }
    }
}
