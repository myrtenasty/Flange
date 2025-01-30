using System;

namespace Preliy.Flange
{
    [Serializable]
    public class NotValidIKSolutionException : Exception
    {
        public IKSolution Solution { get; }

        public NotValidIKSolutionException(IKSolution solution, string message) : base(message: message)
        {
            Solution = solution;
        }
    }
}

