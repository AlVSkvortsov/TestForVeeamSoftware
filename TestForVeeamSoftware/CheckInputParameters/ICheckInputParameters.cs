namespace TestForVeeamSoftware.CheckInputParameters
{
    internal interface ICheckInputParameters
    {
        string[] Args { get; }
        bool CheckParameters();
    }
}
