using System;
using UnityEngine;

namespace Preliy.Flange
{
    /// <summary>
    /// Represents information about the robot arm configuration, to be used when reaching a target.
    /// Given a tool and a target is is usually possible for the robot to reach it using different set of axes angles.
    /// </summary>
    [Serializable]
    public struct Configuration
    {
        /// <summary>
        /// The turn of axis 1.
        /// </summary>
        /// <value>
        /// An integer specifying the turn of axis 1.
        /// </value>
        public int Turn1 => _turn1;

        /// <summary>
        /// The turn of axis 4.
        /// </summary>
        /// <value>
        /// An integer specifying the turn of axis 4.
        /// </value>
        public int Turn4 => _turn4;

        /// <summary>
        /// The turn of axis 6.
        /// </summary>
        /// <value>
        /// An integer specifying the turn of axis 6.
        /// </value>
        public int Turn6 => _turn6;

        /// <summary>
        /// The integer of specific config parameter. Its usage depends on the robot model. For many robot models this value is not used.
        /// </summary>
        public int Index => _index;

        [SerializeField]
        private int _turn1;
        [SerializeField]
        private int _turn4;
        [SerializeField]
        private int _turn6;
        [SerializeField]
        private int _index;

        public static Configuration Default => new (0, 0, 0, 0);

        public Configuration(int turn1, int turn4, int turn6, int index)
        {
            _turn1 = turn1;
            _turn4 = turn4;
            _turn6 = turn6;
            _index = index;
        }

        public Configuration(JointTarget robotJoints, int index = 0)
        {
            _turn1 = robotJoints[0].GetTurn();
            _turn4 = robotJoints[3].GetTurn();
            _turn6 = robotJoints[5].GetTurn();
            _index = index;
        }

        public override string ToString()
        {
            return $"{_turn1} {_turn4} {_turn6} {_index}";
        }

        public void SetIndex(int value)
        {
            _index = value;
        }
        
        public static bool operator ==(Configuration c1, Configuration c2) 
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(Configuration c1, Configuration c2) 
        {
            return !c1.Equals(c2);
        }
        
        public bool Equals(Configuration other)
        {
            return _turn1 == other._turn1 && _turn4 == other._turn4 && _turn6 == other._turn6 && _index == other._index;
        }
        
        public override bool Equals(object obj)
        {
            return obj is Configuration other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(_turn1, _turn4, _turn6, _index);
        }
    }
}
