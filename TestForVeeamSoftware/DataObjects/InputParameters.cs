using System;

namespace TestForVeeamSoftware.DataObjects
{
    internal sealed class InputParameters : IInputParameters
    {
        public Operation OperationName { get; }

        public string SourceFileName { get; }

        public string ResultFileName { get; }

        internal InputParameters(string[] args)
        {
            OperationName = (Operation)Enum.Parse(typeof(Operation), args[0]);
            SourceFileName = args[1];
            ResultFileName = args[2];
        }
    }
}
