using System;

namespace Preliy.Flange
{
    [Flags]
    public enum SolutionIgnoreMask
    {
        None = 0,
        Limits = 1,
        Singularity = 2,
        All = ~0
    }
}
