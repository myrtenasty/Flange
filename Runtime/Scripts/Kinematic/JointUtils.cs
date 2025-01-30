using System.Collections.Generic;
using System.Linq;

namespace Preliy.Flange
{
    public static class JointUtils
    {
        public static List<int> GetTurns(JointConfig config, float value)
        {
            var turns = new List<int>();
            if (config.Limits.y - config.Limits.x < 360f)
            {
                turns.Add(0);
                return turns;
            }
            
            var minTurn = (int)((config.Limits.x - value) / 360f);
            var maxTurn = (int)((config.Limits.y - value) / 360f);
            turns.AddRange(Enumerable.Range(minTurn, maxTurn - minTurn + 1));
            return turns;
        }

        public static void ApplyTurn(this JointTarget jointTarget, Configuration configuration)
        {
            jointTarget[0] += configuration.Turn1 * 360f;
            jointTarget[3] += configuration.Turn4 * 360f;
            jointTarget[5] += configuration.Turn6 * 360f;
        }
    }
}

