namespace TestForVeeamSoftware.DataObjects
{
    internal interface IThreadBlock
    {
        byte[] buffer { get; }
        int actualLength { get; }
        int threadNumber { get; }
        int blockNumber { get; }
    }
}
