using System;
using UnityEngine;

namespace Preliy.Flange
{
    [Serializable]
    public class TaskGizmosConfig
    {
        public bool Enable => _enable;
        public float Scale => _scale;
        public Color ColorPoints => _colorPoints;
        public Color ColorLines => _colorLines;
        public bool ShowBlendZone => _showBlendZone;
        public bool ShowDescription => _showDescription;
        public bool ShowTrajectory => _showTrajectory;
        
        [SerializeField]
        private bool _enable;
        [Range(0.01f, 10f)]
        [SerializeField]
        private float _scale = 1f;
        [SerializeField]
        private Color _colorPoints = Color.red;
        [SerializeField]
        private Color _colorLines = Color.white;
        [SerializeField]
        private bool _showBlendZone;
        [SerializeField]
        private bool _showDescription;
        [SerializeField]
        private bool _showTrajectory;
    }
}
