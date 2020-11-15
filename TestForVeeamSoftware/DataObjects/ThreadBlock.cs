namespace TestForVeeamSoftware.DataObjects
{
    internal class ThreadBlock : IThreadBlock
    {
        public int blockNumber { get; }
        public int threadNumber { get; }
        public byte[] buffer { get; }
        public int actualLength { get; }
        
        internal ThreadBlock(int blkNum, int tnum, byte[] buf, int len)
        {
            blockNumber = blkNum;
            threadNumber = tnum;
            buffer = buf;
            actualLength = len;
        }
    }
}
