namespace TestForVeeamSoftware
{
    internal interface IInputParameters
    {
        Operation OperationName { get; }
        string SourceFileName { get; }
        string ResultFileName { get; }
    }
}
