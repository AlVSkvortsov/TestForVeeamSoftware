using System;
using System.Collections.Generic;
using System.Linq;

namespace TestForVeeamSoftware.ResultOutputManager
{
    internal sealed class ConsoleResultOutputManager : IResultOutputManager
    {
        public int ResultValue { get { return Errors.Count == 0 ? 0 : 1; } }

        public List<Exception> Errors { get; } = new List<Exception>();

        public void AddError(Exception exception) => Errors.Add(exception);

        public void RenderResult()
        {
            if (ResultValue == 1)
            {
                string errorMessage = string.Join(Environment.NewLine, Errors.Select(error => error.Message));
                Console.WriteLine($"Errors: {errorMessage}");
            }
            Console.WriteLine($"Result: {ResultValue}");
        }
    }
}
