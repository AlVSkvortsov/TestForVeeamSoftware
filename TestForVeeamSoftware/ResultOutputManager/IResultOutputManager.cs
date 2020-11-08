using System;
using System.Collections.Generic;

namespace TestForVeeamSoftware.ResultOutputManager
{
    internal interface IResultOutputManager
    {
        int ResultValue { get; }
        List<Exception> Errors { get; }
        void RenderResult();
        void AddError(Exception exception);
    }
}
